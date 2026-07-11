using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace jp.lilxyzw.shadercore.CustomTextures
{
    [ScriptedImporter(1, "scgradients")]
    internal class GradientsImporter : ScriptedImporter
    {
        public int size = 128;
        public Gradient[] gradients = {new()};

        [MenuItem("Assets/Create/ShaderCore/Gradients")]
        private static void Create() => ProjectWindowUtil.CreateAssetWithTextContent("New Gradients.scgradients", "");

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var texture = new Texture2DArray(size, 1, gradients.Length, TextureFormat.RGBA32, true);
            for (int index = 0; index < gradients.Length; index++)
            {
                var pixels = texture.GetPixelData<Color32>(0, index);
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = gradients[index].Evaluate(i / (float)pixels.Length);
                }
            }
            texture.Apply();
            ctx.AddObjectToAsset("Texture", texture);
            ctx.SetMainObject(texture);
        }
    }
}
