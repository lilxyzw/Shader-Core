using UnityEditor;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCFloatField : FloatField, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }

        public SCFloatField(MaterialProperty property) : base()
        {
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            if (Property != null) Property.floatValue = newValue;
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue)
            {
                text = Property.floatValue.ToString(formatString);
                rawValue = Property.floatValue;
            }
        }
    }
}
