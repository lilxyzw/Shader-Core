#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdadd_fullshadows
#pragma multi_compile_fog
#pragma multi_compile_instancing

#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Packages/jp.lilxyzw.shadercore/ShaderLibrary/birp.hlsl"

__SC_BIRP_properties__

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
    UNITY_LIGHTING_COORDS(7,8)
    UNITY_FOG_COORDS(9)
    #ifdef SC_CUSTOM_V2F
        SCCustomV2F customV2f : TEXCOORD10;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#include "Packages/jp.lilxyzw.shadercore/ShaderLibrary/autoconvert.hlsl"
#include "sc_common.hlsl"

void SCOutputSVPosition(inout v2f o, SCVertexData vertex, SCPositionAndDirection camera, SCPositionAndDirection head, SCPositionAndDirection headBone)
{
    SCVertexPost(vertex, camera, head, headBone);
    o.pos = UnityWorldToClipPos(vertex.position);
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
    SCVertexData vertex = FromVertexInput(v, camera, head, headBone,
        #ifdef UNITY_ASSUME_UNIFORM_SCALING
            mul((float3x3)unity_ObjectToWorld, v.normal)
        #else
            mul(v.normal, (float3x3)unity_WorldToObject)
        #endif
        , mul((float3x3)unity_ObjectToWorld, v.tangent.xyz), unity_WorldTransformParams.w);

    SCVertexMorph(vertex, camera, head, headBone);
    ToVertexOutput(o, vertex);

    o.pos = UnityWorldToClipPos(vertex.position);
    UNITY_TRANSFER_LIGHTING(o, v.uv[1].xy);
    UNITY_TRANSFER_FOG(o,o.pos);

    SCOutputSVPosition(o, vertex, camera, head, headBone);

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

    #if defined(POINT) || defined(SPOT) || defined(POINT_COOKIE) || defined(DIRECTIONAL_COOKIE)
    o._LightCoord = i[0]._LightCoord * blend.x + i[1]._LightCoord * blend.y + i[2]._LightCoord * blend.z;
    #endif

    #if defined (SHADOWS_SCREEN) || defined(SHADOWS_DEPTH) && defined(SPOT) || defined(SHADOWS_CUBE)
    o._ShadowCoord = i[0]._ShadowCoord * blend.x + i[1]._ShadowCoord * blend.y + i[2]._ShadowCoord * blend.z;
    #endif

    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    o.fogCoord = i[0].fogCoord * blend.x + i[1].fogCoord * blend.y + i[2].fogCoord * blend.z;
    #endif

    return o;
}

// ピクセルシェーダーは自由に書く
