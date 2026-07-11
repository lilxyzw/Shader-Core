using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore.CustomTextures
{
    [CustomEditor(typeof(MasksImporter))]
    internal class MasksImporterEditor : AssetImporterEditor
    {
        private VisualElement root;
        private PropertyField format;
        private PopupField<int> width;
        private PopupField<int> height;
        private PropertyField masks;
        public List<int> widths = new(){
            32,
            64,
            128,
            256,
            512,
            1024,
            2048,
            4096,
            8192
        };

        protected override bool needsApplyRevert => true;
        public override bool showImportedObject => false;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            root.Bind(serializedObject);
            root.Add(format = new() { bindingPath = "format" });
            root.Add(width = new() { bindingPath = "width", choices = widths, label = "Width" });
            root.Add(height = new() { bindingPath = "height", choices = widths, label = "Height" });
            root.Add(masks = new() { bindingPath = "masks" });
            root.Add(new IMGUIContainer(ApplyRevertGUI));
            return root;
        }
    }
}
