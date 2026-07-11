using System;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCSlider : Slider, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }
        private static readonly MethodInfo MI_UpdateDragElementPosition = typeof(BaseSlider<float>).GetMethod("UpdateDragElementPosition", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[]{}, null);
        private static readonly MethodInfo MI_UpdateTextFieldValue = typeof(BaseSlider<float>).GetMethod("UpdateTextFieldValue", BindingFlags.Instance | BindingFlags.NonPublic);

        public SCSlider(MaterialProperty property, float min, float max) : base(min, max)
        {
            showInputField = true;
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            if (Property != null) Property.floatValue = newValue;
            if (this.Q<TextField>() is TextField textField) textField.showMixedValue = false;
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue)
            {
                rawValue = Property.floatValue;
                MI_UpdateTextFieldValue.Invoke(this, null);
                MI_UpdateDragElementPosition.Invoke(this, null);
            }
            this.Q<TextField>().showMixedValue = Property.hasMixedValue;
        }
    }
}
