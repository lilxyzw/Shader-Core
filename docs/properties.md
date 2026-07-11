# プロパティの記述

シェーダー本体の場合は`*.scshader`、モジュールの場合は`*.scmodule`と同一階層に`properties.hlsl`ファイルを作成することでプロパティを定義できます。

## 基本的な記述

```
//SC_型(変数名, 初期値, [アトリビュート][アトリビュート]..., 表示名, 説明)

SC_Texture2D(_BaseTexture, "white", [SCMainTexture], "Texture", "")
SC_SamplerState(sampler_BaseTexture)
SC_Texture2D(_SharedMask, "white", [SCMask], "__SharedMask", "")
SC_Texture2DArray(_SharedGradients, "white", [SCGradients], "__SharedGradients", "")
SC_float(_NormalScale, 1, [SCCache][SCRange(-10,10)], "", "")
SC_Texture2D(_NormalMap, "bump", [], "__NormalMap", "")
SC_uint(_NormalMapWithRoughness, 0, [SCToggle], "__NormalMapWithRoughness", "")
SC_float(_Roughness, 0.5, [SCRange(0.002,1)], "__Roughness", "")
SC_float(_Cutoff, 0.5, [SCRange(-0.001,1.001)], "__Cutoff", "")

// Boxで囲んでマテリアルエディタを見やすくできます
SC_Box
SC_float(_OutlineWidth, 0.1, [], "Outline Width", "")
SC_color(_OutlineColor, (0.6,0.45,0.55,1), [], "Outline Color", "")
SC_BoxEnd

// Foldoutも使用できます
SC_Foldout(Example Foldout)
SC_float(_Hidden, 0, [], "Hidden", "")
SC_FoldoutEnd
```

## プロパティの型

使える型: `Texture2D`, `Texture2DArray`, `Texture3D`, `TextureCube`, `TextureCubeArray`, `float`, `float4`, `uint`, `uint4`, `int`, `int4`, `color`

特殊なプロパティ: `SC_ScaleOffset(テクスチャ変数名)` テクスチャのスケーリングプロパティ。Unityだと`float4 テクスチャ変数名_ST`が生成されます。
`SC_SamplerState(変数名)` SamplerStateプロパティ。

## 変数名

hlslで使われる変数名です。他モジュールと変数名が重複してエラーにならないように、読み込み時にモジュールの固有IDが自動で変数名に付与されます。

## 初期値

スカラー型の場合は単に数値を入れます。ベクター型の場合は`(0,0,0,0)`のように書きます。テクスチャの場合はShaderLabのように`"white"`や`"bump"`のように記述します。

## 定義済みアトリビュート

`[]`で囲んで記述します。Unity組み込みのアトリビュートは対応していたりしなかったりするので基本的に以下を使用してください。

- `[SCModule(com.example.modulename)]` モジュールの定義、モジュールの言語ファイルの読み込みに使用
- `[SCMainTexture]` シェーダー本体専用のメインテクスチャの定義アトリビュート
- `[SCHide]` エディタ上で非表示にする
- `[SCToggle]` float・intプロパティをトグルで設定できるようにする
- `[SCEnum(A,0,B,1,C,2,D,3)]``[SCEnum(enum型名)]` float・intプロパティをポップアップで設定できるようにする
- `[SCRange(min,max)]` floatプロパティをスライダーで表示
- `[SCRangeInt(min,max)]` float・intプロパティをスライダーで表示
- `[SCMinMax(min,max)]` MinMaxスライダーでfloat4プロパティを表示
- `[SCVector(n)]``[SCVector2]``[SCVector3]``[SCVector4]` float4プロパティを任意のベクターに制限して表示
- `[SCHDR]` colorプロパティでHDRカラーを設定可能にする
- `[SCCache]` プロパティを下のプロパティとまとめて1行で表示できるようにする
- `[SCMask]` 組み込みのマスクエディタを使えるようにする
- `[SCMasks]` 組み込みのマスクエディタを使えるようにする（Texture2DArray）
- `[SCGradients]` 組み込みのグラデーションエディタを使えるようにする
- `[SCGradientSelect]``[SCGradientSelect(propertyname)]` グラデーションを選択するプロパティで選択中のグラデーションを表示
- `[SCMaskChannel]` テクスチャチャンネルを選択するポップアップとテクスチャのプレビューを表示
- `[SCConstValue(max)]``[SCKeyword(max,stage,stage,...)]` 可能であればプロパティを定数化。Unityの場合はシェーダーキーワードを利用。シェーダーステージを指定することでバリアントの増加を抑えられ、シェーダーステージには`vertex``pixel``hull``domain``geomerty`が指定可能

## カスタムアトリビュート

`AttributeActions.AddDrawer(string key, Action<SCMaterialEditor, MaterialProperty, string, VisualElement> action)`や`AttributeActions.AddDecorator(string key, Action<SCMaterialEditor, MaterialProperty, string> action)`で任意のアトリビュートとUIを追加できます。注意点として、Unityではアトリビュートに`-`の記号を使用できないため、インポート時に`-`を`_`に置き換えます。もしアトリビュートの引数として負の値を使う場合、エディタで`_`から`-`に戻すようにしてください。以下はカスタムアトリビュートの実装例です。

```c#
[InitializeOnLoadMethod]
private static void Init()
{
    AttributeActions.AddDecorator("ExampleDecorator", ExampleDecorator);
    AttributeActions.AddDrawer("ExampleDrawer", ExampleDrawer);
}

private static void ExampleDecorator(SCMaterialEditor editor, MaterialProperty prop, string args)
{
    editor.root.Add(new Label(args));
}

private static void ExampleDrawer(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
{
    var argsSeparated = args.Split(',');
    container.Add(new ExampleField(prop, float.Parse(argsSeparated[0].Replace('_','-')), float.Parse(argsSeparated[1].Replace('_','-'))));
}
```
