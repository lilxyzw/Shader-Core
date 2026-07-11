using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCVector4Field : Vector4Field, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }

        public SCVector4Field(MaterialProperty property) : base()
        {
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
            SCStyles.ApplyVectorStyle(this);
        }

        public override void SetValueWithoutNotify(Vector4 newValue)
        {
            if (Property != null) Property.vectorValue = newValue;
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue)
            {
                rawValue = Property.vectorValue;
                int index = 0;
                foreach (var f in this.Query<FloatField>().Build())
                    f.SetValueWithoutNotify(rawValue[index++]);
            }
        }
    }
}
