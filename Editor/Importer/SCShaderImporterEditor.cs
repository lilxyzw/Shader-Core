using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    [CustomEditor(typeof(SCShaderImporter))]
    internal class SCShaderImporterEditor : AssetImporterEditor
    {
        private VisualElement root;
        private Button button;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            root.Bind(serializedObject);
            root.Add(button = new(() => {
                var text = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target)).FirstOrDefault(a => a is TextAsset) as TextAsset;
                var path = EditorUtility.SaveFilePanel("Save shader text", "", target.name, "shader");
                if (!string.IsNullOrEmpty(path)) File.WriteAllText(path, text.text);
            }){text = "Output Text"});
            root.Add(new IMGUIContainer(ApplyRevertGUI));
            return root;
        }
    }
}
