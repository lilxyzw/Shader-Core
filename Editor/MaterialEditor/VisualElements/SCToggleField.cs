using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCToggle : Toggle, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }

        public SCToggle(MaterialProperty property) : base()
        {
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            if (Property != null)
            {
                if (Property.propertyType == ShaderPropertyType.Int) Property.intValue = newValue ? 1 : 0;
                else Property.floatValue = newValue ? 1 : 0;
            }
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue)
            {
                rawValue = Property.propertyType == ShaderPropertyType.Int ? Property.intValue != 0 : Property.floatValue != 0;
            }
            UpdateMixedValueContent();
        }
    }
}
