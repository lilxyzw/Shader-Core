using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.shadercore
{
    internal class SCTextureField : ObjectField, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }
        public Action UpdateUICallback { get; set; }
        private static readonly MethodInfo MI_UpdateDisplay = typeof(ObjectField).GetMethod("UpdateDisplay", BindingFlags.Instance | BindingFlags.NonPublic);

        public SCTextureField(MaterialProperty property) : base()
        {
            objectType = property.textureDimension switch
            {
                TextureDimension.Tex2D => typeof(Texture2D),
                TextureDimension.Tex3D => typeof(Texture3D),
                TextureDimension.Cube => typeof(Cubemap),
                TextureDimension.Tex2DArray => typeof(Texture2DArray),
                TextureDimension.CubeArray => typeof(CubemapArray),
                _ => typeof(Texture)
            };

            Children().First(c => c is not Label).style.width = 0;
            style.flexGrow = 1;
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
        }

        public override void SetValueWithoutNotify(Object newValue)
        {
            if (Property != null) Property.textureValue = newValue as Texture;
            UpdateUICallback?.Invoke();
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue)
            {
                rawValue = Property.textureValue;
                MI_UpdateDisplay.Invoke(this, null);
            }
            UpdateUICallback?.Invoke();
        }
    }
}
