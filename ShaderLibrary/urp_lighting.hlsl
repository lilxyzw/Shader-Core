// void SCCalculateEnvironmentLight(inout SCLightData lightSum, inout half3 env, inout SCShadingData sd, inout SCCustomData cd, SCVertexData vertex, half4 SHAr, half4 SHAg, half4 SHAb, half4 SHBr, half4 SHBg, half4 SHBb, half4 SHC)
// void SCCalculateLight(inout SCLightData lightSum, inout SCShadingData sd, inout SCCustomData cd, SCVertexData vertex, SCLightData light)
// の2つを作ってこれをincludeして
// void SCCalculateAllLights(inout SCLightData lightSum, inout half3 env, inout SCShadingData sd, inout SCCustomData cd, SCVertexData vertex, v2f i, half3 vertexLighting)
// をピクセルシェーダーで実行すると自動でなんとかする

// 指向性を無視して頂点ライティング
// VertexLighting() in Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderLibrary/Lighting.hlsl
half3 SCVertexLighting(float3 positionWS)
{
    half3 vertexLightColor = half3(0.0, 0.0, 0.0);

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    uint lightsCount = GetAdditionalLightsCount();
    uint meshRenderingLayers = GetMeshRenderingLayer();

    LIGHT_LOOP_BEGIN(lightsCount)
        Light light = GetAdditionalLight(lightIndex, positionWS);

#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
    {
#if defined(UNITY_PLATFORM_META_QUEST)
        if(light.distanceAttenuation > 0.0)
#endif
        vertexLightColor += light.color * light.distanceAttenuation;
    }

    LIGHT_LOOP_END
#endif

    return vertexLightColor;
}

void SCCalculateLight(inout SCLightData lightSum, inout SCShadingData sd, inout SCCustomData cd, SCVertexData vertex, Light lightIn)
{
    SCLightData light;
    light.direction = lightIn.direction;
    light.color = lightIn.color * (lightIn.distanceAttenuation * lightIn.shadowAttenuation);
    SCCalculateLight(lightSum, sd, cd, vertex, light);
}

float3 GetOffsetPosition(uint index, SCVertexData vertex)
{
    #if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    float4 lightPositionWS = _AdditionalLightsBuffer[index].position;
    #else
    float4 lightPositionWS = _AdditionalLightsPosition[index];
    #endif
    if (lightPositionWS.w == 0) return vertex.position + lightPositionWS.xyz * vertex.shadowOffset;

    float3 lightVector = lightPositionWS.xyz - vertex.position * lightPositionWS.w;
    float distanceSqr = max(dot(lightVector, lightVector), 1.1);
    return vertex.position + lightVector * rsqrt(distanceSqr) * vertex.shadowOffset;
}

void SCCalculateAllLights(inout SCLightData lightSum, inout half3 env, inout SCShadingData sd, inout SCCustomData cd, SCVertexData vertex, v2f i, half3 vertexLighting)
{
    // InitializeInputData() in Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitForwardPass.hlsl
    InputData inputData = (InputData)0;
    inputData.positionWS = vertex.position;
    inputData.positionCS = vertex.positionRaw;
    inputData.tangentToWorld = half3x3(vertex.T,vertex.B,vertex.N);
    inputData.normalWS = vertex.N;
    inputData.viewDirectionWS = vertex.V;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = i.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS + _MainLightPosition.xyz * vertex.shadowOffset);
    #endif

    inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), i.fogFactor);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(inputData.positionCS);

    float2 staticLightmapUV = 0;
    float2 dynamicLightmapUV = 0;
    #if defined(LIGHTMAP_ON)
    OUTPUT_LIGHTMAP_UV(vertex.uv[1].xy, unity_LightmapST, staticLightmapUV);
    #endif

    #if defined(DYNAMICLIGHTMAP_ON)
    dynamicLightmapUV = vertex.uv[2].xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;;
    #endif

    float4 probeOcclusion = 1;
    uint meshRenderingLayers = GetMeshRenderingLayer();

    // Baked Lights
    // InitializeBakedGIData() in Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitForwardPass.hlsl
    // SAMPLE_GI() in Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl
    #if defined(_SCREEN_SPACE_IRRADIANCE) && !defined(_SURFACE_TYPE_TRANSPARENT) || defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    half4 SHAr = 0;
    half4 SHAg = 0;
    half4 SHAb = 0;
    half4 SHBr = 0;
    half4 SHBg = 0;
    half4 SHBb = 0;
    half4 SHC  = 0;
    #elif AMBIENT_PROBE_BUFFER
    half4 SHAr = SHCoefficients[0];
    half4 SHAg = SHCoefficients[1];
    half4 SHAb = SHCoefficients[2];
    half4 SHBr = SHCoefficients[3];
    half4 SHBg = SHCoefficients[4];
    half4 SHBb = SHCoefficients[5];
    half4 SHC  = SHCoefficients[6];
    #else
    half4 SHAr = unity_SHAr;
    half4 SHAg = unity_SHAg;
    half4 SHAb = unity_SHAb;
    half4 SHBr = unity_SHBr;
    half4 SHBg = unity_SHBg;
    half4 SHBb = unity_SHBb;
    half4 SHC  = unity_SHC ;
    #endif

    #if defined(_SCREEN_SPACE_IRRADIANCE) && !defined(_SURFACE_TYPE_TRANSPARENT)
        inputData.bakedGI = SampleScreenSpaceGI(inputData.positionCS.xy);
    #elif defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        inputData.bakedGI = SampleLightmap(staticLightmapUV, dynamicLightmapUV, inputData.normalWS);
        inputData.shadowMask = SAMPLE_SHADOWMASK(staticLightmapUV);
    #elif defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
        half3 bakedGI;
        if (_EnableProbeVolumes)
        {
            float3 noisedPos = AddNoiseToSamplingPosition(GetAbsolutePositionWS(vertex.position), vertex.positionRaw.xy, vertex.V);
            APVSample apvSample = SampleAPV(noisedPos, vertex.N, meshRenderingLayers, vertex.V);

            if (apvSample.status != APV_SAMPLE_STATUS_INVALID)
            {
                apvSample.Decode();

                if (_APVSkyOcclusionWeight > 0)
                    env += EvaluateOccludedSky(apvSample, vertex.Head);

                SHAr = half4(apvSample.L1_R, apvSample.L0.r);
                SHAg = half4(apvSample.L1_G, apvSample.L0.g);
                SHAb = half4(apvSample.L1_B, apvSample.L0.b);
                #ifdef PROBE_VOLUMES_L2
                    SHBr = apvSample.L2_R;
                    SHBg = apvSample.L2_G;
                    SHBb = apvSample.L2_B;
                    SHC  = half4(apvSample.L2_C, 0.0f);
                #endif
            }

            #ifdef USE_APV_PROBE_OCCLUSION
                probeOcclusion = apvSample.probeOcclusion;
            #endif
        }
    #endif


    // Realtime Lights
    // UniversalFragmentBlinnPhong() in Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderLibrary/Lighting.hlsl
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = (AmbientOcclusionFactor)1; // Ignore AO
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, aoFactor);
    env += inputData.bakedGI;

#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
#endif
    {
        SCCalculateLight(lightSum, sd, cd, vertex, mainLight);
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_CLUSTER_LIGHT_LOOP
    [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK

        inputData.positionWS = GetOffsetPosition(lightIndex, vertex);
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
        #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
        {
            SCCalculateLight(lightSum, sd, cd, vertex, light);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        inputData.positionWS = GetOffsetPosition(lightIndex, vertex);
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
        #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
        {
#if defined(UNITY_PLATFORM_META_QUEST)
            if(light.distanceAttenuation > 0.0)
#endif
            SCCalculateLight(lightSum, sd, cd, vertex, light);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    env += vertexLighting;
    #endif

    SCCalculateEnvironmentLight(lightSum, env, sd, cd, vertex, SHAr, SHAg, SHAb, SHBr, SHBg, SHBb, SHC);
}

void SCCalculateAllLights(inout SCLightData lightSum, inout half3 env, inout SCShadingData sd, inout SCCustomData cd, SCVertexData vertex, v2f i)
{
    SCCalculateAllLights(lightSum, env, sd, cd, vertex, i, SCVertexLighting(vertex.position));
}
