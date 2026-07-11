using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal interface IMaterialOtherPropertyElement
    {
        public Object[] Targets { get; set; }
        public MaterialSerializedProperty Property { get; }
        public string LocalizationKey { get; }
    
        private static readonly MethodInfo MI_GetPropertyState_Serialized = typeof(Material).GetMethod("GetPropertyState_Serialized", BindingFlags.Instance | BindingFlags.NonPublic);

        public void InitializeVisualElement(VisualElement element, Object[] targets, Label label)
        {
            Targets = targets;
            element.style.flexGrow = 1;
            SetupVisualElement(element);
            element.RegisterCallback<SCUpdateEvent>(_ => SetupVisualElement(element));
            label.text = L10n.L(LocalizationKey);
            label.RegisterCallback<SCLocalizeEvent>(_ => label.text = L10n.L(LocalizationKey));
        }

        private void SetupVisualElement(VisualElement element)
        {
            bool isLockedInChildren = false;
            bool isLockedByAncestor = false;
            bool isOverriden = true;
            foreach (Material material in Targets)
            {
                GetPropertyState(material, (int)Property, out var flag, out var flag2, out var flag3);
                isOverriden &= flag;
                isLockedInChildren |= flag2;
                isLockedByAncestor |= flag3;
            }

            element.SetEnabled(!isLockedByAncestor);
            element.style.unityFontStyleAndWeight = isOverriden ? FontStyle.Bold : FontStyle.Normal;
        }

        public static void GetPropertyState(Material material, int property, out bool isOverriden, out bool isLockedInChildren, out bool isLockedByAncestor)
        {
            isOverriden = false;
            isLockedInChildren = false;
            isLockedByAncestor = false;
            var args = new object[]{property, isOverriden, isLockedInChildren, isLockedByAncestor};
            MI_GetPropertyState_Serialized.Invoke(material, args);
            isOverriden = (bool)args[1];
            isLockedInChildren = (bool)args[2];
            isLockedByAncestor = (bool)args[3];
        }
    }

    public enum MaterialSerializedProperty
    {
        None = 0,
        LightmapFlags = 2,
        EnableInstancingVariants = 4,
        DoubleSidedGI = 8,
        CustomRenderQueue = 0x10
    }
}
