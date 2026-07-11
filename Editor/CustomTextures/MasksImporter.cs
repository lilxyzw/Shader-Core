using System;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace jp.lilxyzw.shadercore.CustomTextures
{
    [ScriptedImporter(1, "scmasks")]
    internal class MasksImporter : ScriptedImporter
    {
        public TextureFormat format = TextureFormat.BC7;
        public int width = 1024;
        public int height = 1024;
        public MaskParam[] masks = {new()};

        [MenuItem("Assets/Create/ShaderCore/Masks")]
        private static void Create() => ProjectWindowUtil.CreateAssetWithTextContent("New Masks.scmasks", "");

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var texture = new Texture2DArray(width, height, masks.Length, format, true);

            for (int index = 0; index < masks.Length; index++)
            {
                var layer = MaskGenerator.Generate(masks[index].R, masks[index].G, masks[index].B, masks[index].A, format, width, height);
                Graphics.CopyTexture(layer, 0, texture, index);
            }

            texture.Apply(false, false);
            ctx.AddObjectToAsset("Texture", texture);
            ctx.SetMainObject(texture);
        }

        [Serializable]
        internal class MaskParam
        {
            public ChannelParam R = new();
            public ChannelParam G = new();
            public ChannelParam B = new();
            public ChannelParam A = new();
        }
    }
}
