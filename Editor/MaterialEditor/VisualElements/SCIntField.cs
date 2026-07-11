using UnityEditor;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCIntField : IntegerField, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }

        public SCIntField(MaterialProperty property) : base()
        {
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            if (Property != null) Property.intValue = newValue;
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue)
            {
                text = Property.intValue.ToString(formatString);
                rawValue = Property.intValue;
            }
        }
    }
}
