# シェーダー本体の書き方

シェーダー本体ではPBRやトゥーン、Unlitなど大まかなライティングの方向性を定義します。また、ベースカラーやベースノーマルマップなどモジュール化する必要のないプロパティも実装します。

## ファイルの作成

ShaderLabで書いて拡張子を`*.scshader`にします。

## プロパティ

scshaderと同一階層に`properties.hlsl`を配置してプロパティを記述します。記述方法については[プロパティの記述](https://github.com/lilxyzw/Shader-Core/blob/main/docs/properties.md)を参照してください。

## ShaderLabのプロパティブロックの書き換え

`properties.hlsl`に記述したプロパティはシェーダー本体のShaderLabから消し、`__SC_SHADERLAB_properties__`と書いてください。するとそこに`properties.hlsl`の中身が展開されます。末尾がアトリビュートになることによるシェーダーエラー回避のため、末尾に何らかのプロパティを書くか、`[SCHide]_Dummy("",Float) = 0`のようなダミープロパティを残してください。

## HLSLの書き換え

ShaderLab同様にHLSLのプロパティ記述部分も消し、`__SC_BIRP_properties__`と記述してください。そこにシェーダー本体と各モジュールのプロパティが展開されます。

## 変数の宣言

各モジュールで使える変数を宣言する必要があります。

```hlsl
v2f vert(appdata v)
{
    ...

    // camera head headBone vertexが必須
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

    ...
}

half4 frag(v2f i, bool isFront : SV_IsFrontFace) : SV_Target
{
    ...

    // camera head headBone vertexが必須
    SCPositionAndDirection camera = SCGetCameraData();
    SCPositionAndDirection head = SCGetHeadData();
    SCPositionAndDirection headBone = SCGetHeadBoneData();
    SCVertexData vertex = FromPixelInput(i, camera, head, headBone, unity_WorldTransformParams.w, isFront);

    ...

    // cdが必須
    SCCustomData cd = (SCCustomData)cd;

    ...
    // sdが必須
    SCShadingData sd;
    sd.L = 0;
    sd.lightColor = 0;
    sd.shadow = 1;
    sd.add = 0;
    sd.postadd = 0;
    sd.uv = vertex.uv[0].xy;
    sd.albedoAlpha = SCSample(_BaseTexture, sampler_BaseTexture, sd.uv);
    sd.mask = SCSample(_SharedMask, sampler_BaseTexture, sd.uv);
    sd.roughness = _Roughness;
    sd.normalMapWithRoughness = _NormalMapWithRoughness;
    sd.N = SCUnpackNormalAndRoughness(SCSample(_NormalMap, sampler_BaseTexture, sd.uv), _NormalScale, sd.roughness, sd.normalMapWithRoughness);
    sd.N_detail = sd.N;
    sd.maskTexture = _SharedMask;
    sd.gradientsTexture = _SharedGradients;

    __SC_PHASE_base__

    sd.albedoAlpha = saturate(sd.albedoAlpha);
    sd.col = sd.albedoAlpha;

    sd.N = normalize(mul(sd.N, vertex.TBN));
    sd.N_detail = normalize(mul(sd.N_detail, vertex.TBN));
    sd.T = normalize(vertex.T - sd.N_detail * dot(sd.N_detail, vertex.T));
    sd.B = normalize(cross(sd.N_detail, sd.T) * vertex.crossDirection * unity_WorldTransformParams.w);

    ...

    // lightSum envが必須
    SCLightData lightSum = (SCLightData)0;
    half3 env = 0;
}
```

## モジュールを挿入

モジュールを挿入したいところに`__SC_PHASE_フェーズ名__`と記述します。フェーズ名のリストは[フェーズ](https://github.com/lilxyzw/Shader-Core/blob/main/docs/phase.md)にあります。リストにあるフェーズを一通り書くことでシェーダー本体の実装は完了です。
