#if !UNITY_6000_1_OR_NEWER
// 古いUnityでpropertyTypeとpropertyFlagsが使えないのでその対応
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    public class MaterialProperty
    {
        private readonly UnityEditor.MaterialProperty property;
        public MaterialProperty(UnityEditor.MaterialProperty property) => this.property = property;
        public ShaderPropertyType propertyType => (ShaderPropertyType)property.type;
        public ShaderPropertyFlags propertyFlags => (ShaderPropertyFlags)property.flags;

        public Object[] targets => property.targets;
        public string name => property.name;
        public string displayName => property.displayName;
        public TextureDimension textureDimension => property.textureDimension;
        public Vector2 rangeLimits => property.rangeLimits;
        public bool hasMixedValue => property.hasMixedValue;

        public Color colorValue
        {
            get { return property.colorValue; }
            set { property.colorValue = value; }
        }

        public Vector4 vectorValue
        {
            get { return property.vectorValue; }
            set { property.vectorValue = value; }
        }

        public float floatValue
        {
            get { return property.floatValue; }
            set { property.floatValue = value; }
        }

        public int intValue
        {
            get { return property.intValue; }
            set { property.intValue = value; }
        }

        public Texture textureValue
        {
            get { return property.textureValue; }
            set { property.textureValue = value; }
        }

        public Vector4 textureScaleAndOffset
        {
            get { return property.textureScaleAndOffset; }
            set { property.textureScaleAndOffset = value; }
        }

        public static implicit operator UnityEditor.MaterialProperty(MaterialProperty property)
        {
            return property.property;
        }

        public static implicit operator MaterialProperty(UnityEditor.MaterialProperty property)
        {
            return new(property);
        }
    }
}

public partial class SCMaterialEditor : MaterialEditor
{
    public void ShaderProperty(VisualElement container, UnityEditor.MaterialProperty prop, string[] attributes = null) => ShaderProperty(container, (jp.lilxyzw.shadercore.MaterialProperty)prop, attributes);
}
#endif
