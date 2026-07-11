using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore.CustomTextures
{
    [CustomEditor(typeof(MaskImporter))]
    internal class MaskImporterEditor : AssetImporterEditor
    {
        private VisualElement root;
        private PropertyField format;
        private PopupField<int> width;
        private PopupField<int> height;
        private PropertyField R;
        private PropertyField G;
        private PropertyField B;
        private PropertyField A;
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
            root.Add(R = new() { bindingPath = "R" });
            root.Add(G = new() { bindingPath = "G" });
            root.Add(B = new() { bindingPath = "B" });
            root.Add(A = new() { bindingPath = "A" });
            root.Add(new IMGUIContainer(ApplyRevertGUI));
            return root;
        }
    }
}
