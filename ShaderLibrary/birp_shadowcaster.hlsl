#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_shadowcaster
#pragma multi_compile_instancing

#include "UnityCG.cginc"
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
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#include "Packages/jp.lilxyzw.shadercore/ShaderLibrary/autoconvert.hlsl"
#include "sc_common.hlsl"

void SCOutputSVPosition(inout v2f o, SCVertexData vertex, SCPositionAndDirection camera, SCPositionAndDirection head, SCPositionAndDirection headBone)
{
    float3 L = normalize(UnityWorldSpaceLightDir(vertex.position));
    SCVertexPost(vertex, camera, head, headBone, L);

    if(unity_LightShadowBias.z != 0.0)
    {
        float3 normalWS = normalize(vertex.N);
        float shadowCos = dot(normalWS, L);
        float shadowSine = sqrt(1-shadowCos*shadowCos);
        float normalBias = unity_LightShadowBias.z * shadowSine;
        vertex.position -= normalWS * normalBias;
    }
    o.pos = UnityWorldToClipPos(vertex.position);
    o.pos = UnityApplyLinearShadowBias(o.pos);
}

v2f vert(appdata v)
{
    v2f o;
    UNITY_INITIALIZE_OUTPUT(v2f, o);

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
    SCOutputSVPosition(o, vertex, camera, head, headBone);

    #ifdef SC_CUSTOM_V2F
    SCCustomV2FFunc(o, vertex, camera, head, headBone);
    #endif

    return o;
}

half4 frag(v2f i, bool isFront : SV_IsFrontFace) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    SCPixelClip(i, isFront, unity_WorldTransformParams.w);
    SHADOW_CASTER_FRAGMENT(i);
}
