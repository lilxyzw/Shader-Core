using UnityEngine;

namespace jp.lilxyzw.shadercore
{
    internal static class TextureUtils
    {
        // テクスチャから任意のチャンネル・インデックスを取得
        private static readonly int id_MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int id_MainTexArray = Shader.PropertyToID("_MainTexArray");
        private static readonly int id_Channel = Shader.PropertyToID("_Channel");
        private static readonly int id_Index = Shader.PropertyToID("_Index");

        public static Texture2D Get2DChannel(Texture2D tex, int width, int height, int channel)
        {
            if (!tex) return Texture2D.blackTexture;
            var material = new Material(Shader.Find("Hidden/ShaderCore/PreviewTexture"));
            material.SetTexture(id_MainTex, tex);
            material.SetFloat(id_Channel, channel);
            material.SetFloat(id_Index, -1);
            return MaterialToTexture(material, width, height);
        }

        public static Texture2D Get2DArraySlice(Texture2DArray tex, int width, int height, int slice, int channel = -1)
        {
            if (!tex || slice < 0) return Texture2D.blackTexture;
            var material = new Material(Shader.Find("Hidden/ShaderCore/PreviewTexture"));
            material.SetTexture(id_MainTexArray, tex);
            material.SetFloat(id_Channel, channel);
            material.SetFloat(id_Index, Mathf.Clamp(slice, 0, tex.depth-1));
            return MaterialToTexture(material, width, height);
        }

        private static Texture2D MaterialToTexture(Material material, int width, int height)
        {
            var currentRT = RenderTexture.active;
            var renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            RenderTexture.active = renderTexture;
            Graphics.Blit(null, renderTexture, material);
            var outTex = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            outTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            outTex.Apply();
            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);
            Object.DestroyImmediate(material);
            return outTex;
        }
    }
}
