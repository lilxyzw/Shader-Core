using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCGradientSelect : IntegerField, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }
        private readonly Image image;
        private readonly string texturePropertyName;

        public SCGradientSelect(MaterialProperty property, string texturePropertyName) : base()
        {
            this.texturePropertyName = texturePropertyName;
            image = new Image{scaleMode = ScaleMode.StretchToFill};
            image.style.flexGrow = 1;
            image.style.height = 16;
            image.style.marginBottom = 1;
            image.style.marginLeft = 2;
            image.style.marginRight = 0;
            image.style.marginTop = 1;
            Add(image);
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            if (Property != null) Property.intValue = newValue;
            UpdateImage();
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue)
            {
                text = Property.intValue.ToString(formatString);
                rawValue = Property.intValue;
            }
            UpdateImage();
        }

        private void UpdateImage()
        {
            if (image != null)
            {
                var prop = MaterialEditor.GetMaterialProperty(Property.targets, texturePropertyName);
                if (prop.hasMixedValue)
                {
                    image.image = null;
                }
                else
                {
                    image.image = TextureUtils.Get2DArraySlice(prop.textureValue as Texture2DArray, 128, 1, Property.intValue);
                }
            }
        }
    }
}
