using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCMinMax : BaseField<Vector4>, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }
        private readonly SCMinMaxTextField minField;
        private readonly SCMinMaxSlider slider;
        private readonly SCMinMaxTextField maxField;

        public SCMinMax(MaterialProperty property, float min, float max) : base("", new(){name = "visual-input"})
        {
            var visualInput = this.Q("visual-input");
            visualInput.style.flexDirection = FlexDirection.Row;
            visualInput.Add(minField = new SCMinMaxTextField(this, property, true));
            visualInput.Add(slider = new SCMinMaxSlider(this, property, min, max));
            visualInput.Add(maxField = new SCMinMaxTextField(this, property, false));
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
        }

        public void UpdateUI()
        {
            minField?.UpdateUI();
            slider?.UpdateUI();
            maxField?.UpdateUI();
        }

        protected override void UpdateMixedValueContent()
        {
            minField.showMixedValue = Property.hasMixedValue;
            slider.showMixedValue = Property.hasMixedValue;
            minField.showMixedValue = Property.hasMixedValue;
        }

        private class SCMinMaxSlider : MinMaxSlider, IMaterialPropertyElement
        {
            public MaterialProperty Property { get => minMax?.Property; set => minMax.Property = value; }
            public string ModuleID { get => minMax?.ModuleID; set {} }
            public string LocalizedLabel { get => ""; set {} }
            private static readonly MethodInfo MI_UpdateDragElementPosition = typeof(MinMaxSlider).GetMethod("UpdateDragElementPosition", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[]{}, null);
            private readonly SCMinMax minMax;

            public SCMinMaxSlider(SCMinMax minMax, MaterialProperty property, float min, float max) : base(min, max, min, max)
            {
                this.minMax = minMax;
                ((IMaterialPropertyElement)this).InitializeVisualElement(this, null, property);
            }

            public override void SetValueWithoutNotify(Vector2 newValue)
            {
                if (Property != null)
                {
                    var vec = Property.vectorValue;
                    Property.vectorValue = new(newValue.x, newValue.y, vec.z, vec.w);
                }
                minMax?.UpdateUI();
                base.SetValueWithoutNotify(newValue);
            }

            public void UpdateUI()
            {
                if (!Property.hasMixedValue) rawValue = Property.vectorValue;
                MI_UpdateDragElementPosition.Invoke(this, null);
                ((IMaterialPropertyElement)this).SetupVisualElement(this);
            }
        }

        private class SCMinMaxTextField : FloatField, IMaterialPropertyElement
        {
            public MaterialProperty Property { get => minMax?.Property; set => minMax.Property = value; }
            public string ModuleID { get => minMax?.ModuleID; set {} }
            public string LocalizedLabel { get => ""; set {} }
            private readonly bool isMin;
            private readonly SCMinMax minMax;

            public SCMinMaxTextField(SCMinMax minMax, MaterialProperty property, bool isMin) : base()
            {
                this.minMax = minMax;
                this.isMin = isMin;
                ((IMaterialPropertyElement)this).InitializeVisualElement(this, null, property);
                style.marginLeft = 0;
                style.marginRight = 0;
                style.paddingLeft = isMin ? 0 : 2;
                style.paddingRight = isMin ? 2 : 0;
                style.width = 52;
                style.minWidth = 52;
                style.maxWidth = 52;
            }

            public override void SetValueWithoutNotify(float newValue)
            {
                if (Property != null)
                {
                    var vec = Property.vectorValue;
                    if (isMin) Property.vectorValue = new(newValue, vec.y, vec.z, vec.w);
                    else Property.vectorValue = new(vec.x, newValue, vec.z, vec.w);
                }
                minMax?.UpdateUI();
                base.SetValueWithoutNotify(newValue);
            }

            public void UpdateUI()
            {
                if (isMin && !Property.hasMixedValue)
                {
                    text = Property.vectorValue.x.ToString(formatString);
                    rawValue = Property.vectorValue.x;
                }
                else if (!isMin && !Property.hasMixedValue)
                {
                    text = Property.vectorValue.y.ToString(formatString);
                    rawValue = Property.vectorValue.y;
                }
                ((IMaterialPropertyElement)this).SetupVisualElement(this);
            }
        }
    }
}
