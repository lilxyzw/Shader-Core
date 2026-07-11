#pragma vertex vert
#pragma fragment frag

#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
#pragma multi_compile _ LOD_FADE_CROSSFADE
#pragma multi_compile_instancing
#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
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
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#include "Packages/jp.lilxyzw.shadercore/ShaderLibrary/autoconvert.hlsl"
#include "sc_common.hlsl"

void SCOutputSVPosition(inout v2f o, SCVertexData vertex, SCPositionAndDirection camera, SCPositionAndDirection head, SCPositionAndDirection headBone)
{
    SCVertexPost(vertex, camera, head, headBone);
    o.pos = TransformWorldToHClip(vertex.position);
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

    return o;
}

void frag(v2f i, bool isFront : SV_IsFrontFace, out half4 outNormalWS : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out uint outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    SCPixelClip(i, isFront, GetOddNegativeScale());
    outNormalWS = half4(normalize(i.N.xyz), 0.0);

    #ifdef _WRITE_RENDERING_LAYERS
    outRenderingLayers = EncodeMeshRenderingLayer();
    #endif
}
