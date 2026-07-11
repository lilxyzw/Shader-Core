using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore.CustomTextures
{
    [CustomEditor(typeof(GradientsImporter))]
    internal class GradientsImporterEditor : AssetImporterEditor
    {
        private VisualElement root;
        private PopupField<int> size;
        private PropertyField gradients;
        public List<int> widths = new(){
            4,
            8,
            16,
            32,
            64,
            128,
            256
        };

        protected override bool needsApplyRevert => true;
        public override bool showImportedObject => false;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            root.Bind(serializedObject);
            root.Add(size = new() { bindingPath = "size", choices = widths, label = "Width" });
            root.Add(gradients = new() { bindingPath = "gradients" });
            root.Add(new IMGUIContainer(ApplyRevertGUI));
            return root;
        }
    }
}
