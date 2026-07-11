using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    public interface IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string LocalizedLabel { get; set; }
        public string ModuleID { get; set; }

        public void InitializeVisualElement<T>(BaseField<T> element, Action updateUI, MaterialProperty inProperty)
        {
            Property = inProperty;
            ModuleID = L10n.currentID;
            LocalizedLabel = L10n.L(Property.displayName);
            element.label = LocalizedLabel;
            element.style.flexGrow = 1;
            element.style.marginRight = 2;
            updateUI?.Invoke();
            SetupVisualElement(element);

            void UpdateUILocal()
            {
                Property = MaterialEditor.GetMaterialProperty(Property.targets, Property.name);
                updateUI?.Invoke();
                SetupVisualElement(element);
            }

            element.RegisterValueChangedCallback(e => SetupVisualElement(element));
            element.RegisterCallback<SCUpdateEvent>(_ => UpdateUILocal());
            element.RegisterCallback<ChangeEvent<T>>(e =>
            {
                if (Property.targets[0] is not Material m || m.shader is not Shader shader) return;
                var attributes = shader.GetPropertyAttributes(shader.FindPropertyIndex(Property.name));
                foreach (var attr in attributes)
                {
                    AttributeActions.TryInvokeValueChangeAction(attr, Property);
                }
            });
            if (!string.IsNullOrWhiteSpace(element.label)) element.RegisterCallback<SCKeyEvent>(e => { if (e.keyCode == KeyCode.LeftAlt) element.label = e.isDown ? Property.name : LocalizedLabel; });
            element.RegisterCallback<SCLocalizeEvent>(e =>
            {
                L10n.Load(ModuleID);
                if (!string.IsNullOrWhiteSpace(element.label)) element.label = LocalizedLabel = L10n.L(Property.displayName);
            });

            // Right click menu
            element.Q<Label>(className: BaseField<T>.labelUssClassName)?.RegisterCallback<PointerUpEvent>(e => {
                if (e.button != 1) return;
                MaterialPropertyUtils.GetPropertyState(Property.targets[0] as Material, Shader.PropertyToID(Property.name), out var isOverriden, out var isLockedInChildren, out var isLockedByAncestor);
                if (isOverriden)
                {
                    EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition, Vector2.one), new GUIContent[] { L10n.G("Copy"), L10n.G("Paste"), L10n.G("Apply to parent"), L10n.G("Revert"), L10n.G("Copy Name") }, -1, (_, _, selected) =>
                    {
                        if (selected == 0)
                        {
                            MaterialPropertyUtils.Copy(Property);
                        }
                        else if (selected == 1)
                        {
                            MaterialPropertyUtils.Paste(Property);
                            UpdateUILocal();
                        }
                        else if (selected == 2)
                        {
                            PropertyClipboard.ApplyToParent(Property);
                            UpdateUILocal();
                        }
                        else if (selected == 3)
                        {
                            PropertyClipboard.Revert(Property);
                            UpdateUILocal();
                        }
                        else if (selected == 4)
                        {
                            GUIUtility.systemCopyBuffer = Property.name;
                        }
                    }, null);
                }
                else
                {
                    EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition, Vector2.one), new GUIContent[] { L10n.G("Copy"), L10n.G("Paste"), L10n.G("Reset"), L10n.G("Copy Name") }, -1, (_, _, selected) =>
                    {
                        if (selected == 0)
                        {
                            MaterialPropertyUtils.Copy(Property);
                        }
                        else if (selected == 1)
                        {
                            MaterialPropertyUtils.Paste(Property);
                            UpdateUILocal();
                        }
                        else if (selected == 2)
                        {
                            MaterialPropertyUtils.Reset(Property);
                            UpdateUILocal();
                        }
                        else if (selected == 3)
                        {
                            GUIUtility.systemCopyBuffer = Property.name;
                        }
                    }, null);
                }
            });
        }

        public void UpdateUI();

        public void SetupVisualElement<T>(BaseField<T> element)
        {
            SetupVisualElement((VisualElement)element);
            element.showMixedValue = Property.hasMixedValue;
        }

        public void SetupVisualElement(VisualElement element)
        {
            bool isLockedInChildren = false;
            bool isLockedByAncestor = false;
            bool isOverriden = true;
            int nameID = Shader.PropertyToID(Property.name);
            foreach (Material material in Property.targets)
            {
                MaterialPropertyUtils.GetPropertyState(material, nameID, out var flag, out var flag2, out var flag3);
                isOverriden &= flag;
                isLockedInChildren |= flag2;
                isLockedByAncestor |= flag3;
            }

            element.SetEnabled(!isLockedByAncestor);
            element.style.unityFontStyleAndWeight = isOverriden ? FontStyle.Bold : FontStyle.Normal;
        }
    }
}
