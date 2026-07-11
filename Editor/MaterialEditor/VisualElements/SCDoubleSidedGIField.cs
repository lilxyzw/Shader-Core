using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCDoubleSidedGIField : Toggle, IMaterialOtherPropertyElement
    {
        public Object[] Targets { get; set; }
        public MaterialSerializedProperty Property => MaterialSerializedProperty.DoubleSidedGI;
        public string LocalizationKey => "__DoubleSidedGlobalIllumination";
        public static string PATH => "m_DoubleSidedGI";

        public SCDoubleSidedGIField(Object[] targets) : base(PATH)
        {
            ((IMaterialOtherPropertyElement)this).InitializeVisualElement(this, targets, labelElement);
            if (enabledSelf) bindingPath = PATH;
            else value = (targets[0] as Material).enableInstancing;
        }
    }
}
