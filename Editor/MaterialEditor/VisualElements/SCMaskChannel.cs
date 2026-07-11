using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCMaskChannel : PopupField<int>, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }
        private readonly List<int> values = new(){0,1,2,3};
        private readonly List<string> names = new(){"R","G","B","A"};

        private readonly Image image;
        private readonly string texturePropertyName;

        public SCMaskChannel(MaterialProperty property, string texturePropertyName) : base()
        {
            string GetLabel(int v)
            {
                if (v >= 0 && v < names.Count) return names[v];
                return "";
            }
            choices = values;
            formatListItemCallback = GetLabel;
            formatSelectedValueCallback = GetLabel;

            this.texturePropertyName = texturePropertyName;
            image = new Image{scaleMode = ScaleMode.StretchToFill};
            image.style.width = 16;
            image.style.height = 16;
            image.style.marginBottom = 1;
            image.style.marginLeft = 2;
            image.style.marginRight = 0;
            image.style.marginTop = 1;
            Add(image);

            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
            SCStyles.ApplyPopupStyle(this);
            style.flexGrow = 0;
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            if (Property != null && new System.Diagnostics.StackFrame(3, false).GetMethod().ToString() == "Void ChangeValueFromMenu(Int32)") Property.intValue = newValue;
            UpdateImage();
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue)
            {
                rawValue = Property.intValue;
                if (formatSelectedValueCallback != null) textElement.text = formatSelectedValueCallback(rawValue);
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
                    image.image = TextureUtils.Get2DChannel(prop.textureValue as Texture2D, 128, 128, Property.intValue);
                }
            }
        }
    }
}
