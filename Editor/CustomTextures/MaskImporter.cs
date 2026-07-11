using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace jp.lilxyzw.shadercore.CustomTextures
{
    [ScriptedImporter(1, "scmask")]
    internal class MaskImporter : ScriptedImporter
    {
        public TextureFormat format = TextureFormat.BC7;
        public int width = 1024;
        public int height = 1024;
        public ChannelParam R = new();
        public ChannelParam G = new();
        public ChannelParam B = new();
        public ChannelParam A = new();

        [MenuItem("Assets/Create/ShaderCore/Mask")]
        private static void Create() => ProjectWindowUtil.CreateAssetWithTextContent("New Mask.scmask", "");

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var texture = MaskGenerator.Generate(R, G, B, A, format, width, height);
            ctx.AddObjectToAsset("Texture", texture);
            ctx.SetMainObject(texture);
        }
    }
}
