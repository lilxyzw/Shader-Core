using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCVector2Field : Vector2Field, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }

        public SCVector2Field(MaterialProperty property) : base()
        {
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
            SCStyles.ApplyVectorStyle(this);
        }

        public override void SetValueWithoutNotify(Vector2 newValue)
        {
            if (Property != null)
            {
                var vec = Property.vectorValue;
                Property.vectorValue = new(newValue.x, newValue.y, vec.z, vec.w);
            }
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
