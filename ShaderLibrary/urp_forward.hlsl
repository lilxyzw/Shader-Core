#pragma vertex vert
#pragma fragment frag

#if defined(UNITY_PLATFORM_META_QUEST)
#pragma multi_compile _ META_QUEST_LIGHTUNROLL
#endif
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
#pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
#pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
#pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
#pragma multi_compile_fragment _ _LIGHT_COOKIES
#pragma multi_compile _ _LIGHT_LAYERS
#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
#if defined(UNITY_PLATFORM_META_QUEST)
#pragma multi_compile _ META_QUEST_ORTHO_PROJ
#pragma multi_compile _ META_QUEST_NO_SPOTLIGHTS_LIGHT_LOOP
#endif
#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
#pragma multi_compile _ SHADOWS_SHADOWMASK
#pragma multi_compile _ DIRLIGHTMAP_COMBINED
#pragma multi_compile _ LIGHTMAP_ON
#pragma multi_compile_fragment _ LIGHTMAP_BICUBIC_SAMPLING
#pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
#pragma multi_compile _ DYNAMICLIGHTMAP_ON
#pragma multi_compile _ USE_LEGACY_LIGHTMAPS
#pragma multi_compile _ LOD_FADE_CROSSFADE
#pragma multi_compile_fragment _ DEBUG_DISPLAY
#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"

#pragma multi_compile_instancing
#pragma instancing_options renderinglayer
#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
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
    float fogFactor : TEXCOORD7;
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        float4 shadowCoord : TEXCOORD8;
    #endif
    #ifdef SC_CUSTOM_V2F
        SCCustomV2F customV2f : TEXCOORD9;
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

    o.pos = TransformWorldToHClip(vertex.position);
    #if !defined(_FOG_FRAGMENT)
        o.fogFactor = ComputeFogFactor(o.pos.z);
    #endif
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        VertexPositionInputs vertexInput;
        vertexInput.positionWS = vertex.position;
        vertexInput.positionVS = TransformWorldToView(vertex.position);
        vertexInput.positionCS = o.pos;
        float4 ndc = vertexInput.positionCS * 0.5f;
        vertexInput.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
        vertexInput.positionNDC.zw = vertexInput.positionCS.zw;
        o.shadowCoord = GetShadowCoord(vertexInput);
    #endif

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

    o.fogFactor = i[0].fogFactor * blend.x + i[1].fogFactor * blend.y + i[2].fogFactor * blend.z;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        o.shadowCoord = i[0].shadowCoord * blend.x + i[1].shadowCoord * blend.y + i[2].shadowCoord * blend.z;
    #endif

    return o;
}

// ピクセルシェーダーは自由に書く
