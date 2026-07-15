# モジュールの書き方

モジュールではシェーダー本体で決定されたライティングに基づいて、RampシェーディングやリムライトやラメやSSAOなどを実行します。また、プラットフォーム固有の特殊なライト（VRCLVやLTCGI）などを追加でシェーダー本体に渡すこともできます。ただし、シェーダー本体で決定されたライティングの方向性を変えるようなモジュールは想定されておらず、そのようなモジュール同士は競合するのでサポート対象外です。

## ファイルの作成

JSONで書いて拡張子を`*.scmodule`にします。以下が記述例です。

### 最小パターン

```json
{
    "name": "Example Module",
    "uniqueID": "com.example.modulename"
}
```

### 詳細定義パターン

```json
{
    "name": "Example Module",
    "uniqueID": "com.example.modulename",
    "phases": [
        {
            "name": "Example Speular",
            "phase": "light",
            "path": "specular.hlsl"
        },
        {
            "name": "Example MatCaps",
            "phase": "reflection",
            "path": "matcaps.hlsl",
            "befores": [
                "com.example.othermodule"
            ],
            "afters": [
                "com.example.othermoduleN"
            ]
        }
    ]
}
```

### JSONプロパティ

ファイル名が`phase_フェーズ名.hlsl`である場合は`phases`および`phases.path`記述を省略できます。

- `name` 表示名です。
- `uniqueID` 固有IDです。他と重複しないようにしてください。
- `phases` フェーズの定義です。
  - `name` フェーズ名です
  - `phase` 挿入先のフェーズです
  - `path` ファイルパスです。
  - `befores` 他のモジュールの固有IDを記述しそのモジュールより先に実行されるように定義します。
  - `afters` 他のモジュールの固有IDを記述しそのモジュールより後に実行されるように定義します。

## include

scmoduleと同一階層に`includes.hlsl`を配置して`#include "Packages/com.example..."`を記述することで外部ファイルをincludeできます。

## プロパティ

scmoduleと同一階層に`properties.hlsl`を配置してプロパティを記述します。記述方法についてはプロパティの記述を参照してください。

## シェーダーコード

`*.hlsl`ファイルを作成しモジュールのコードを記述します。他のモジュールとローカル変数名が重複しエラーにならないように`{}`で囲うことをオススメします。ネスト外にローカル変数を記述することで他のモジュール・フェーズと共有することもできますが競合の原因になる場合があるため慎重に行ってください。以下はコードの例です。

```
{
    half rim = saturate(1-dot(sd.N,vertex.V));
    rim = pow(rim, _RimPower);
    sd.add += rim * sd.mask[_RimLightMaskChannel] * _RimLightColor.rgb;
}
```

> [!WARNING]
> `sd.albedoAlpha = SCSampleClamp(_Tex_, uv);`のように他モジュールと共有する変数を完全に置き換えることは推奨されません。他モジュールとの互換性がなくなるのと、シェーダーコンパイラーが最適化してSamplerStateを正しく読み込めずエラーになる可能性があります。

## 多重導入可能なモジュール

以下のように多重に導入できるモジュールを作成することもできます。
https://x.com/lil_xyzw/status/2077089990467842168

### プロパティの配置

`properties_multi.hlsl`を配置し、以下のようにプロパティを記述します。モジュールを読み込む際、`__N__`の部分がインデックスの数字に置き換えられます。`properties_multi.hlsl`は導入した個数分だけ繰り返し読み込まれます。

```
SC_Foldout(Decal __N__)
SC_Texture2D(_Decal__N__Texture, "black", [], "__Texture", "")
SC_uint(_Decal__N__UV, 0, [SCEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)], "__UV", "")
SC_FoldoutEnd
```

また、`properties.hlsl`に`__N__`が含まれる場合、ユーザーが導入した個数分の数値に置き換えられます。`properties.hlsl`は導入した個数に関わらず1回しか読み込まれません。

```
SC_uint(_DecalCount, 0, [SCEnum(__N__)][SCConstValue(__N__,pixel)], "Decal Count", "")
```

### hlslの書き換え

フェーズごとのhlslも導入した個数分だけ繰り返し読み込まれます。hlslを読み込む際、`__N__`の部分がインデックスの数字に置き換えられます。

```
{
    half4 decal = SCSampleClamp(_Decal__N__Texture, vertex.uv[_Decal__N__UV;].xy);
    sd.albedoAlpha.rgb = lerp(sd.albedoAlpha.rgb, decal.rgb, decal.a);
}
```
