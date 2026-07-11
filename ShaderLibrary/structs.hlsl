// 頂点シェーダー・ピクセルシェーダー両方で使える構造体
struct SCVertexData
{
    // 頂点シェーダーではUV0-7のxyzw成分が使える
    // ピクセルシェーダーではUV0-3のxy成分が使える
    // ピクセルシェーダーに全部転送するようにすると出力が多すぎてジオメトリシェーダーが書けない
    float4 uv[8];

    // 基本的に変数はすべてワールド空間です
    float3 position;
    half3 N; // normal
    half3 T; // tangent
    half3 B; // binormal
    half3x3 TBN;
    half crossDirection; // tangent.w

    half3 V; // view
    half3 Head; // view (VRでは左右の目の中間)
    float cameraDepth; // length(camera.position - position)
    float headDepth; // length(head.position - position)

    half4 color; // Vertex Shader Only
    bool isFront; // Pixel Shader Only
    float4 positionRaw; // Pixel Shader Only, SV_Position
    float2 uvDepth; // Pixel Shader Only, used for sample depth texture
    float2 uvColor; // Pixel Shader Only, used for sample color texture

    float shadowOffset; // Shadow Offset
};

struct SCPositionAndDirection
{
    float3 position;
    half3 up;
    half3 down;
    half3 left;
    half3 right;
    half3 forward;
    half3 back;
};

// phaseがlightのモジュールでのみ使える構造体
struct SCLightData
{
    half3 direction;
    half3 color; // ライト色 * 減衰 * 影
};

// ピクセルシェーダーのみで使える構造体
struct SCShadingData
{
    half4 albedoAlpha; // 材質の色と不透明度、メインテクスチャやディテール、デカールなどのみを適用
    half4 col; // 最終的に出力されるシェーディングの結果
    half4 mask; // 各モジュールで使える共有マスクテクスチャ
    float2 uv; // メインテクスチャのサンプリングに使われるものと同じuv
    half3 T; // ノーマルマップ適用後のnormal
    half3 B; // ノーマルマップ適用後のtangent
    half3 N; // ノーマルマップ適用後のbinormal
    half3 N_detail; // ディテールノーマルマップ適用後のnormal
    half3 L; // トゥーンシェーディング用のライトベクトル
    half3 lightColor; // ライト色
    half shadow; // 影
    half2 roughness; // ラフネス
    half3 add; // 加算
    half3 postadd; // ライト乗算後の加算（エミッション）
    bool normalMapWithRoughness; // ノーマルマップのRBチャンネルにラフネスを含めるかどうか
    Texture2D maskTexture; // マスクテクスチャ
    Texture2DArray gradientsTexture; // グラデーションテクスチャ
};
