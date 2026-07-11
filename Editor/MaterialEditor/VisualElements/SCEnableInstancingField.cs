using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCEnableInstancingField : Toggle, IMaterialOtherPropertyElement
    {
        public Object[] Targets { get; set; }
        public MaterialSerializedProperty Property => MaterialSerializedProperty.EnableInstancingVariants;
        public string LocalizationKey => "__EnableGPUInstancing";
        public static string PATH = "m_EnableInstancingVariants";

        public SCEnableInstancingField(Object[] targets) : base(PATH)
        {
            ((IMaterialOtherPropertyElement)this).InitializeVisualElement(this, targets, labelElement);
            if (enabledSelf) bindingPath = PATH;
            else value = (targets[0] as Material).enableInstancing;
        }
    }
}
