Shader "Hidden/ShaderCore/PreviewTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTexArray ("Texture", 2DArray) = "white" {}
        _Channel ("", Int) = -1
        _Index ("", Int) = -1
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            SamplerState sampler_linear_clamp;
            Texture2D _MainTex;
            Texture2DArray _MainTexArray;
            int _Channel;
            int _Index;

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
                if (_Index == -1) col = _MainTex.SampleLevel(sampler_linear_clamp, i.uv, 0);
                else col = _MainTexArray.SampleLevel(sampler_linear_clamp, float3(i.uv, _Index), 0);

                #ifndef UNITY_COLORSPACE_GAMMA
                col.rgb = LinearToGammaSpace(col.rgb);
                #endif

                if (_Channel == 0) col = float4(col.r, 0, 0, 1);
                if (_Channel == 1) col = float4(0, col.g, 0, 1);
                if (_Channel == 2) col = float4(0, 0, col.b, 1);
                if (_Channel == 3) col = float4(col.aaa, 1);

                return col;
            }
            ENDCG
        }
    }
}
