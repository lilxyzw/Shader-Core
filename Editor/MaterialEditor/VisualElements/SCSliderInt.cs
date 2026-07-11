using System;
using System.Reflection;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCSliderInt : SliderInt, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }
        private static readonly MethodInfo MI_UpdateDragElementPosition = typeof(BaseSlider<int>).GetMethod("UpdateDragElementPosition", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[]{}, null);
        private static readonly MethodInfo MI_UpdateTextFieldValue = typeof(BaseSlider<int>).GetMethod("UpdateTextFieldValue", BindingFlags.Instance | BindingFlags.NonPublic);

        public SCSliderInt(MaterialProperty property, int min, int max) : base(min, max)
        {
            showInputField = true;
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            if (Property != null)
            {
                if (Property.propertyType == ShaderPropertyType.Int) Property.intValue = newValue;
                else Property.floatValue = newValue;
            }
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue)
            {
                rawValue = Property.propertyType == ShaderPropertyType.Int ? Property.intValue : (int)Property.floatValue;
                MI_UpdateTextFieldValue.Invoke(this, null);
                MI_UpdateDragElementPosition.Invoke(this, null);
            }
            else
            {
                showMixedValue = Property.hasMixedValue;
            }
            this.Q<TextField>().showMixedValue = Property.hasMixedValue;
        }
    }
}
