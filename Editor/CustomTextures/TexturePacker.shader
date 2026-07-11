Shader "Hidden/ShaderCore/TexturePacker"
{
    Properties
    {
        _TextureR ("Texture", 2D) = "white" {}
        _TextureG ("Texture", 2D) = "white" {}
        _TextureB ("Texture", 2D) = "white" {}
        _TextureA ("Texture", 2D) = "white" {}
        _BlendR ("Color", Vector) = (0,0,0,0)
        _BlendG ("Color", Vector) = (0,0,0,0)
        _BlendB ("Color", Vector) = (0,0,0,0)
        _BlendA ("Color", Vector) = (0,0,0,0)
        _IgnoreTexture ("Color", Vector) = (1,1,1,1)
        _Invert ("Color", Vector) = (0,0,0,0)
        _DefaultR ("Float", Float) = 1
        _DefaultG ("Float", Float) = 1
        _DefaultB ("Float", Float) = 1
        _DefaultA ("Float", Float) = 1
        _TextureSize ("Vector", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            Texture2D _TextureR;
            Texture2D _TextureG;
            Texture2D _TextureB;
            Texture2D _TextureA;
            float4 _TextureR_ST;
            float4 _TextureG_ST;
            float4 _TextureB_ST;
            float4 _TextureA_ST;
            float4 _BlendR;
            float4 _BlendG;
            float4 _BlendB;
            float4 _BlendA;
            float4 _IgnoreTexture;
            float4 _Invert;
            float4 _TextureSize;
            float _DefaultR;
            float _DefaultG;
            float _DefaultB;
            float _DefaultA;
            SamplerState sampler_linear_clamp;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col;
                col.r = dot(_TextureR.Sample(sampler_linear_clamp, i.uv * _TextureR_ST.xy + _TextureR_ST.zw), _BlendR);
                col.g = dot(_TextureG.Sample(sampler_linear_clamp, i.uv * _TextureG_ST.xy + _TextureG_ST.zw), _BlendG);
                col.b = dot(_TextureB.Sample(sampler_linear_clamp, i.uv * _TextureB_ST.xy + _TextureB_ST.zw), _BlendB);
                col.a = dot(_TextureA.Sample(sampler_linear_clamp, i.uv * _TextureA_ST.xy + _TextureA_ST.zw), _BlendA);

                col = lerp(col, 1-col, _Invert);
                col = lerp(col, float4(_DefaultR,_DefaultG,_DefaultB,_DefaultA), _IgnoreTexture);
                //if(!IsGammaSpace()) col.rgb = LinearToGammaSpace(col.rgb);
                return col;
            }
            ENDCG
        }
    }
}
