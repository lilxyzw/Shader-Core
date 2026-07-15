using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCEditableTextureButton<TexType, ImporterType> : Button
        where TexType : Texture
        where ImporterType : ScriptedImporter
    {
        private readonly SCTextureField field;
        private readonly string extension;
        public SCEditableTextureButton(SCTextureField field, string extension) : base()
        {
            this.field = field;
            this.extension = extension;
            field.UpdateUICallback -= UpdateUI;
            field.UpdateUICallback += UpdateUI;
            RegisterCallback<SCUpdateEvent>(_ => UpdateUI());
            RegisterCallback<SCLocalizeEvent>(_ => UpdateUI());
            clickable = new Clickable(CreateAndEditGUI);
            UpdateUI();
        }

        private void UpdateUI()
        {
            text = field.Property.textureValue ? L10n.L("__EditTexture") : L10n.L("__CreateTexture");
            SetEnabled(!field.Property.hasMixedValue && (!field.Property.textureValue || AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(field.Property.textureValue)) is ImporterType));
        }

        private void CreateAndEditGUI()
        {
            if (!field.Property.textureValue)
            {
                var path = $"{Path.GetDirectoryName(AssetDatabase.GetAssetPath(field.Property.targets[0]))}/{field.Property.targets[0].name}{field.Property.name}.{extension}";
                File.WriteAllBytes(path, new byte[]{});
                AssetDatabase.ImportAsset(path);
                field.Property.textureValue = AssetDatabase.LoadAssetAtPath<TexType>(path);

                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(field.Property.textureValue)) as ImporterType;
                var window = ScriptableObject.CreateInstance<EditorWindow>();
                window.rootVisualElement.Add(Editor.CreateEditor(importer).CreateInspectorGUI());
                window.ShowUtility();

                var e = SCUpdateEvent.GetPooled();
                e.target = field;
                field.SendEvent(e);
            }
            else if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(field.Property.textureValue)) is ImporterType importer)
            {
                var window = ScriptableObject.CreateInstance<EditorWindow>();
                window.rootVisualElement.Add(Editor.CreateEditor(importer).CreateInspectorGUI());
                window.ShowUtility();
            }
        }
    }
}
