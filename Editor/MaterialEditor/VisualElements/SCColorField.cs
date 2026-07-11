using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCColorField : ColorField, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }

        public SCColorField(MaterialProperty property) : base()
        {
            #if !UNITY_6000_4_OR_NEWER
            this.Q<IMGUIContainer>().style.width = 0;
            #endif
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
        }

        public override void SetValueWithoutNotify(Color newValue)
        {
            if (Property != null) Property.colorValue = newValue;
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue) rawValue = Property.colorValue;
        }
    }
}
