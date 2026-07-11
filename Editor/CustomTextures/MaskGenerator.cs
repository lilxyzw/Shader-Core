using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.shadercore.CustomTextures
{
    internal static class MaskGenerator
    {
        public static Texture2D Generate(ChannelParam R, ChannelParam G, ChannelParam B, ChannelParam A, TextureFormat format, int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, true);

            var material = new Material(Shader.Find("Hidden/ShaderCore/TexturePacker"));
            SetChannel(material, "R", R);
            SetChannel(material, "G", G);
            SetChannel(material, "B", B);
            SetChannel(material, "A", A);
            material.SetVector("_IgnoreTexture", new Vector4(
                R.tex ? 0 : 1,
                G.tex ? 0 : 1,
                B.tex ? 0 : 1,
                A.tex ? 0 : 1
            ));
            material.SetVector("_Invert", new Vector4(
                R.mode == ChannelMode.OneMinusR || R.mode == ChannelMode.OneMinusG || R.mode == ChannelMode.OneMinusB || R.mode == ChannelMode.OneMinusA?1:0,
                G.mode == ChannelMode.OneMinusR || G.mode == ChannelMode.OneMinusG || G.mode == ChannelMode.OneMinusB || G.mode == ChannelMode.OneMinusA?1:0,
                B.mode == ChannelMode.OneMinusR || B.mode == ChannelMode.OneMinusG || B.mode == ChannelMode.OneMinusB || B.mode == ChannelMode.OneMinusA?1:0,
                A.mode == ChannelMode.OneMinusR || A.mode == ChannelMode.OneMinusG || A.mode == ChannelMode.OneMinusB || A.mode == ChannelMode.OneMinusA?1:0
            ));
            material.SetVector("_TextureSize", new Vector4(width, height, 0, 0));

            var currentRT = RenderTexture.active;
            var renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            RenderTexture.active = renderTexture;
            Graphics.Blit(null, renderTexture, material);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);

            EditorUtility.CompressTexture(texture, format, TextureCompressionQuality.Best);
            return texture;
        }

        private static void SetChannel(Material material, string channel, ChannelParam param)
        {
            material.SetTexture("_Texture"+channel, GetUncompressedTexture(param.tex));
            material.SetVector("_Blend"+channel, ModeToVector(param));
            material.SetFloat("_Default"+channel, param.fallbackValue);
        }

        private static Texture2D GetUncompressedTexture(Texture2D tex)
        {
            if (!tex) return tex;
            var path = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(path)) return tex;

            var raw = new Texture2D(4,4, TextureFormat.RGBA32, false, !tex.isDataSRGB);
            if (raw.LoadImage(File.ReadAllBytes(path))) return raw;

            if (raw) Object.DestroyImmediate(raw);
            return tex;
        }

        private static Vector4 ModeToVector(ChannelParam param)
        {
            return param.mode switch
            {
                ChannelMode.R => new(1, 0, 0, 0),
                ChannelMode.G => new(0, 1, 0, 0),
                ChannelMode.B => new(0, 0, 1, 0),
                ChannelMode.A => new(0, 0, 0, 1),
                ChannelMode.OneMinusR => new(1, 0, 0, 0),
                ChannelMode.OneMinusG => new(0, 1, 0, 0),
                ChannelMode.OneMinusB => new(0, 0, 1, 0),
                ChannelMode.OneMinusA => new(0, 0, 0, 1),
                ChannelMode.Gray => new(0.333333f, 0.333333f, 0.333333f, 0f),
                ChannelMode.Luminance => new(0.2126729f, 0.7151522f, 0.0721750f, 0f),
                _ => param.blend,
            };
        }
    }

    [Serializable]
    internal class ChannelParam
    {
        public Texture2D tex;
        public ChannelMode mode = ChannelMode.R;
        public Vector4 blend;
        public float fallbackValue = 1;
    }

    internal enum ChannelMode
    {
        R,
        G,
        B,
        A,
        OneMinusR,
        OneMinusG,
        OneMinusB,
        OneMinusA,
        Gray,
        Luminance,
        Custom
    }
}
