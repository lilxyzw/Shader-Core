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

    #if !LIGHTMAP_ON && UNITY_SHOULD_SAMPLE_SH && VERTEXLIGHT_ON
    float4 toLightX = unity_4LightPosX0 - positionWS.x;
    float4 toLightY = unity_4LightPosY0 - positionWS.y;
    float4 toLightZ = unity_4LightPosZ0 - positionWS.z;

    float4 lengthSq = toLightX * toLightX + 0.000001;
    lengthSq += toLightY * toLightY;
    lengthSq += toLightZ * toLightZ;

    float4 atten = saturate(saturate((25.0 - lengthSq * unity_4LightAtten0) * 0.111375) / (0.987725 + lengthSq * unity_4LightAtten0));

    [unroll]
    for(int i = 0; i < 4; i++)
    {
        half3 L = half3(toLightX[i], toLightY[i], toLightZ[i]) * rsqrt(lengthSq[i]);
        half3 lightColor = unity_LightColor[i].rgb * atten[i];
        vertexLightColor += lightColor;
    }
    #endif

    return vertexLightColor;
}

void SCCalculateAllLights(inout SCLightData lightSum, inout half3 env, inout SCShadingData sd, inout SCCustomData cd, SCVertexData vertex, v2f i, half3 vertexLighting)
{
    UNITY_LIGHT_ATTENUATION(attenuation, i, vertex.position);

    // Baked Lights
    #if !defined(LIGHTMAP_ON) && UNITY_SHOULD_SAMPLE_SH
        half4 SHAr = unity_SHAr;
        half4 SHAg = unity_SHAg;
        half4 SHAb = unity_SHAb;
        half4 SHBr = unity_SHBr;
        half4 SHBg = unity_SHBg;
        half4 SHBb = unity_SHBb;
        half4 SHC  = unity_SHC ;
    #else
        half4 SHAr = 0;
        half4 SHAg = 0;
        half4 SHAb = 0;
        half4 SHBr = 0;
        half4 SHBg = 0;
        half4 SHBb = 0;
        half4 SHC  = 0;
    #endif

    half3 lightmap = 0;
    #if defined(LIGHTMAP_ON)
        float2 staticLightmapUV = vertex.uv[1].xy * unity_LightmapST.xy + unity_LightmapST.zw;
        half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, staticLightmapUV);
        half3 bakedColor = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, staticLightmapUV));

        #ifdef DIRLIGHTMAP_COMBINED
            fixed4 bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, staticLightmapUV);
            lightmap += DecodeDirectionalLightmap(bakedColor, bakedDirTex, vertex.N);
        #else
            lightmap += bakedColor;
        #endif

        #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN)
            half ndotl = saturate(dot(vertex.N, _WorldSpaceLightPos0.xyz));
            half3 subtractedLightmap = lightmap - ndotl * (1-attenuation) * _LightColor0.rgb;
            half3 realtimeShadow = max(subtractedLightmap, unity_ShadowColor.rgb);
            realtimeShadow = lerp(realtimeShadow, lightmap, _LightShadowData.x);
            lightmap = min(lightmap, realtimeShadow);
        #endif
    #endif

    #ifdef DYNAMICLIGHTMAP_ON
        float2 dynamicLightmapUV = vertex.uv[2].xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
        fixed4 realtimeColorTex = UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, dynamicLightmapUV);
        half3 realtimeColor = DecodeRealtimeLightmap(realtimeColorTex);

        #ifdef DIRLIGHTMAP_COMBINED
            half4 realtimeDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, dynamicLightmapUV);
            lightmap += DecodeDirectionalLightmap(realtimeColor, realtimeDirTex, vertex.N);
        #else
            lightmap += realtimeColor;
        #endif
    #endif
    env += lightmap;

    // Main Light
    #if !(defined(LIGHTMAP_ON) && defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN))
        SCLightData light = (SCLightData)0;
        light.color = _LightColor0.rgb * attenuation;
        #if defined(UNITY_PASS_FORWARDBASE)
            light.direction = _WorldSpaceLightPos0.xyz;
        #elif defined(UNITY_PASS_FORWARDADD)
            light.directional = normalize(UnityWorldSpaceLightDir(p.posWorld.xyz));
        #endif
        SCCalculateLight(lightSum, sd, cd, vertex, light);
    #endif

    env += vertexLighting;

    SCCalculateEnvironmentLight(lightSum, env, sd, cd, vertex, SHAr, SHAg, SHAb, SHBr, SHBg, SHBb, SHC);
}

void SCCalculateAllLights(inout SCLightData lightSum, inout half3 env, inout SCShadingData sd, inout SCCustomData cd, SCVertexData vertex, v2f i)
{
    SCCalculateAllLights(lightSum, env, sd, cd, vertex, i, SCVertexLighting(vertex.position));
}
