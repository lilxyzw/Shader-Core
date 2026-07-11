# モジュールの書き方

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
  - `befores` 他のモジュールの固有IDを記述しそのモジュールより後に実行されるように定義します。

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
