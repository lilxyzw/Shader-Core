#pragma vertex vert
#pragma fragment frag

#pragma multi_compile _ LOD_FADE_CROSSFADE
#pragma multi_compile _ APPLICATION_SPACE_WARP_MOTION_TRANSPARENT
#pragma shader_feature_local_vertex _ADD_PRECOMPUTED_VELOCITY
#define APPLICATION_SPACE_WARP_MOTION 1

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MotionVectorsCommon.hlsl"
#include "Packages/jp.lilxyzw.shadercore/ShaderLibrary/urp.hlsl"

__SC_URP_properties__

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float4 color : COLOR;
    float4 uv[8] : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv[4] : TEXCOORD0;
    float3 position : TEXCOORD4;
    float3 N : TEXCOORD5;
    float4 T : TEXCOORD6;
    #ifdef SC_CUSTOM_V2F
        SCCustomV2F customV2f : TEXCOORD7;
    #endif
    float4 positionCSNoJitter         : POSITION_CS_NO_JITTER;
    float4 previousPositionCSNoJitter : PREV_POSITION_CS_NO_JITTER;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#include "Packages/jp.lilxyzw.shadercore/ShaderLibrary/autoconvert.hlsl"
#include "sc_common.hlsl"

void SCOutputSVPosition(inout v2f o, SCVertexData vertex, SCPositionAndDirection camera, SCPositionAndDirection head, SCPositionAndDirection headBone)
{
    SCVertexPost(vertex, camera, head, headBone);
    #if defined(APPLICATION_SPACE_WARP_MOTION)
        float4 positionOS = mul(SC_W2O(), float4(vertex.position, 1));
        o.pos = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, positionOS));
    #else
        o.pos = TransformWorldToHClip(vertex.position);
    #endif
}

v2f vert(appdata v)
{
    v2f o = (v2f)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    SCPositionAndDirection camera = SCGetCameraData();
    SCPositionAndDirection head = SCGetHeadData();
    SCPositionAndDirection headBone = SCGetHeadBoneData();
    SCVertexData vertex = FromVertexInput(v, camera, head, headBone, TransformObjectToWorldNormal(v.normal, false), TransformObjectToWorldDir(v.tangent.xyz, false), GetOddNegativeScale());

    SCVertexMorph(vertex, camera, head, headBone);
    ToVertexOutput(o, vertex);
    SCVertexPost(vertex, camera, head, headBone);

    float4 positionOS = mul(SC_W2O(), float4(vertex.position, 1));

    #if defined(APPLICATION_SPACE_WARP_MOTION)
        o.positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, positionOS));;
        o.pos = o.positionCSNoJitter;
    #else
        o.pos = TransformWorldToHClip(vertex.position);
        o.positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, positionOS));
    #endif

    vertex = FromVertexInput(v, camera, head, headBone, TransformObjectToWorldNormal(v.normal, false), TransformObjectToWorldDir(v.tangent.xyz, false), GetOddNegativeScale());
    vertex.position = mul(SC_O2W(), float4(v.uv[4].xyz, 1)).xyz;

    SCVertexMorph(vertex, camera, head, headBone);
    SCVertexPost(vertex, camera, head, headBone);

    float4 positionOld = mul(SC_W2O(), float4(vertex.position, 1));

    float4 prevPos = (unity_MotionVectorsParams.x == 1) ? positionOld : positionOS;

    #if _ADD_PRECOMPUTED_VELOCITY
        prevPos = prevPos - float4(v.uv[5].xyz, 0);
    #endif

    o.previousPositionCSNoJitter = mul(_PrevViewProjMatrix, mul(UNITY_PREV_MATRIX_M, prevPos));

    #ifdef SC_CUSTOM_V2F
    SCCustomV2FFunc(o, vertex, camera, head, headBone);
    #endif

    return o;
}

v2f SCInterpolateV2F(v2f i[3], float3 blend)
{
    v2f o = (v2f)0;

    UNITY_SETUP_INSTANCE_ID(i[0]);
    UNITY_TRANSFER_INSTANCE_ID(i[0], o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.pos = i[0].pos * blend.x + i[1].pos * blend.y + i[2].pos * blend.z;
    o.uv[0] = i[0].uv[0] * blend.x + i[1].uv[0] * blend.y + i[2].uv[0] * blend.z;
    o.uv[1] = i[0].uv[1] * blend.x + i[1].uv[1] * blend.y + i[2].uv[1] * blend.z;
    o.uv[2] = i[0].uv[2] * blend.x + i[1].uv[2] * blend.y + i[2].uv[2] * blend.z;
    o.uv[3] = i[0].uv[3] * blend.x + i[1].uv[3] * blend.y + i[2].uv[3] * blend.z;
    o.position = i[0].position * blend.x + i[1].position * blend.y + i[2].position * blend.z;
    o.N = i[0].N * blend.x + i[1].N * blend.y + i[2].N * blend.z;
    o.T = i[0].T * blend.x + i[1].T * blend.y + i[2].T * blend.z;

    o.positionCSNoJitter = i[0].positionCSNoJitter * blend.x + i[1].positionCSNoJitter * blend.y + i[2].positionCSNoJitter * blend.z;
    o.previousPositionCSNoJitter = i[0].previousPositionCSNoJitter * blend.x + i[1].previousPositionCSNoJitter * blend.y + i[2].previousPositionCSNoJitter * blend.z;

    return o;
}

float4 frag(v2f i, bool isFront : SV_IsFrontFace) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    SCPixelClip(i, isFront, GetOddNegativeScale());

    #if defined(APPLICATION_SPACE_WARP_MOTION)
        return float4(CalcAswNdcMotionVectorFromCsPositions(i.positionCSNoJitter, i.previousPositionCSNoJitter), 1);
    #else
        return float4(CalcNdcMotionVectorFromCsPositions(i.positionCSNoJitter, i.previousPositionCSNoJitter), 0, 0);
    #endif
}
