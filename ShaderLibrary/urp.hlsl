#include "Packages/jp.lilxyzw.shadercore/ShaderLibrary/warnings.hlsl"
#include "Packages/jp.lilxyzw.shadercore/ShaderLibrary/structs.hlsl"

// Property
#define SC_Box
#define SC_BoxEnd
#define SC_Foldout(name)
#define SC_FoldoutEnd
#define SC_SamplerState(name) SamplerState name;
#define SC_ScaleOffset(name) float4 name##_ST;
#define SC_Texture2D(name,defaultvalue,drawer,displayname,description) Texture2D name;
#define SC_Texture2DArray(name,defaultvalue,drawer,displayname,description) Texture2DArray name;
#define SC_Texture3D(name,defaultvalue,drawer,displayname,description) Texture3D name;
#define SC_TextureCube(name,defaultvalue,drawer,displayname,description) TextureCube name;
#define SC_TextureCubeArray(name,defaultvalue,drawer,displayname,description) TextureCubeArray name;
#define SC_float(name,defaultvalue,drawer,displayname,description) float name;
#define SC_float4(name,defaultvalue,drawer,displayname,description) float4 name;
#define SC_uint(name,defaultvalue,drawer,displayname,description) uint name;
#define SC_uint4(name,defaultvalue,drawer,displayname,description) uint4 name;
#define SC_int(name,defaultvalue,drawer,displayname,description) int name;
#define SC_int4(name,defaultvalue,drawer,displayname,description) int4 name;
#define SC_color(name,defaultvalue,drawer,displayname,description) float4 name;

#if 1
SamplerState sampler_trilinear_repeat;
SamplerState sampler_trilinear_clamp;
#define sampler_linear_repeat sampler_trilinear_repeat
#define sampler_linear_clamp sampler_trilinear_clamp
#else
SamplerState sampler_linear_repeat;
SamplerState sampler_linear_clamp;
#endif

bool SCIsGamma()
{
    #ifdef UNITY_COLORSPACE_GAMMA
        return true;
    #else
        return false;
    #endif
}

float4 SCSample(Texture2D tex, SamplerState smp, float2 uv)
{
    return tex.Sample(smp, uv);
}

float4 SCSampleRepeat(Texture2D tex, float2 uv)
{
    return tex.Sample(sampler_linear_repeat, uv);
}

float4 SCSampleClamp(Texture2D tex, float2 uv)
{
    return tex.Sample(sampler_linear_clamp, uv);
}

float4 SCSample(Texture2DArray tex, SamplerState smp, float2 uv, float index)
{
    return tex.Sample(smp, float3(uv,index));
}

float4 SCSampleRepeat(Texture2DArray tex, float2 uv, float index)
{
    return tex.Sample(sampler_linear_repeat, float3(uv,index));
}

float4 SCSampleClamp(Texture2DArray tex, float2 uv, float index)
{
    return tex.Sample(sampler_linear_clamp, float3(uv,index));
}

half3 SCUnpackNormal(half4 tex, half scale)
{
    #if defined(UNITY_NO_DXT5nm)
        half3 normal = tex.xyz * 2 - 1;
        normal.xy *= scale;
    #else
        tex.x *= tex.w;
        half3 normal;
        normal.xy = (tex.xy * 2 - 1) * scale;
        normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
    #endif
    return normal;
}

half3 SCUnpackNormalAndRoughness(half4 tex, half scale, inout half2 roughness, bool normalMapWithRoughness)
{
    if (normalMapWithRoughness)
    {
        half2 scaled = saturate(tex.xz * scale);
        roughness = roughness + scaled - roughness * scaled;
    }
    else
    {
        return SCUnpackNormal(tex, scale);
    }
    half3 normal;
    normal.xy = (tex.wy * 2 - 1) * scale;
    normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
    return normal;
}

SCPositionAndDirection SCGetCameraData()
{
    SCPositionAndDirection camera = (SCPositionAndDirection)0;
    #ifdef SHADOWS_SCREEN
    camera.position = _WorldSpaceCameraPos.xyz;
    #else
    camera.position = UNITY_MATRIX_I_V._m03_m13_m23;
    #endif
    camera.right = UNITY_MATRIX_V._m00_m01_m02;
    camera.up = UNITY_MATRIX_V._m10_m11_m12;
    camera.forward = UNITY_MATRIX_V._m20_m21_m22;
    camera.left = -camera.right;
    camera.down = -camera.up;
    camera.back = -camera.forward;
    return camera;
}

SCPositionAndDirection SCGetHeadData()
{
    SCPositionAndDirection head = SCGetCameraData();
    #if defined(USING_STEREO_MATRICES)
    head.position = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5;
    head.right = unity_StereoMatrixV[0]._m00_m01_m02 * 0.5 + unity_StereoMatrixV[1]._m00_m01_m02 * 0.5;
    head.up = unity_StereoMatrixV[0]._m10_m11_m12 * 0.5 + unity_StereoMatrixV[1]._m10_m11_m12 * 0.5;
    head.forward = unity_StereoMatrixV[0]._m20_m21_m22 * 0.5 + unity_StereoMatrixV[1]._m20_m21_m22 * 0.5;
    head.left = -head.right;
    head.down = -head.up;
    head.back = -head.forward;
    #endif
    return head;
}

SCPositionAndDirection SCGetHeadBoneData()
{
    SCPositionAndDirection headBone = (SCPositionAndDirection)0;
    headBone.position = UNITY_MATRIX_M._m03_m13_m23;
    headBone.right = UNITY_MATRIX_M._m00_m10_m20;
    headBone.up = UNITY_MATRIX_M._m01_m11_m21;
    headBone.forward = UNITY_MATRIX_M._m02_m12_m22;
    headBone.left = -headBone.right;
    headBone.down = -headBone.up;
    headBone.back = -headBone.forward;
    return headBone;
}

#if defined(SHADER_STAGE_RAY_TRACING)
float4x4 SC_O2W(){return ObjectToWorld3x4();}
float4x4 SC_W2O(){return WorldToObject3x4();}
#else
float4x4 SC_O2W(){return UNITY_MATRIX_M;}
float4x4 SC_W2O(){return UNITY_MATRIX_I_M;}
#endif

float4x4 SC_W2V(){return UNITY_MATRIX_V;}
float4x4 SC_V2P(){return UNITY_MATRIX_P;}

bool SCIsPerspective()
{
    return unity_OrthoParams.w == 0;
}

bool SCIsVR()
{
    return abs(UNITY_MATRIX_P._m02) > 0.000001;
}

float SCTangentScale()
{
    return GetOddNegativeScale();
}

// Depth
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#if defined(UNITY_SINGLE_PASS_STEREO)
    float2 ClampScreenUV(float2 uv){ return UnityStereoClamp(uv, unity_StereoScaleOffset[unity_StereoEyeIndex]); }
#else
    float2 ClampScreenUV(float2 uv){ return saturate(uv); }
#endif

#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
    bool SCIsFrameDepthGenerated()
    {
        return true;
    }

    bool SCIsFrameColorGenerated()
    {
        return true;
    }
#else
    bool SCIsFrameDepthGenerated()
    {
        return true;
    }

    bool SCIsFrameColorGenerated()
    {
        return true;
    }
#endif

float SCGetFrameDepth(float2 uv)
{
    if(SCIsFrameDepthGenerated())
    {
        #if UNITY_UV_STARTS_AT_TOP
            if(_ProjectionParams.x > 0) uv.y = _ScreenParams.y - uv.y;
        #else
            if(_ProjectionParams.x < 0) uv.y = _ScreenParams.y - uv.y;
        #endif
        float2 uv2 = ClampAndScaleUVForBilinear(UnityStereoTransformScreenSpaceTex(uv), _CameraDepthTexture_TexelSize.xy);
        float cameraDepthTexture = SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, sampler_PointClamp, uv2, 0).r;
        #if UNITY_REVERSED_Z
            if(cameraDepthTexture == 0) return 1.0/0.0;
        #else
            if(cameraDepthTexture == 1) return 0;
        #endif

        float2 pos = uv * 2.0 - 1.0;
        #if UNITY_UV_STARTS_AT_TOP
            pos.y = -pos.y;
        #endif
        return UNITY_MATRIX_P._m23 / (cameraDepthTexture + UNITY_MATRIX_P._m22
            - UNITY_MATRIX_P._m20 / UNITY_MATRIX_P._m00 * (pos.x + UNITY_MATRIX_P._m02)
            - UNITY_MATRIX_P._m21 / UNITY_MATRIX_P._m11 * (pos.y + UNITY_MATRIX_P._m12)
        );
    }
    else
    {
        return 0;
    }
}

float4 SCGetFrameColor(float2 uv)
{
    if(SCIsFrameColorGenerated())
    {
        return half4(SampleSceneColor(uv), 1);
    }
    else
    {
        return 0;
    }
}

__SC_INCLUDES__
