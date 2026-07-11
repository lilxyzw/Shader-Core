using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace jp.lilxyzw.shadercore
{
    internal static class MaterialPropertyUtils
    {
        private static readonly Type T_Clipboard = typeof(Editor).Assembly.GetType("UnityEditor.Clipboard");
        private static readonly PropertyInfo PI_colorValue = T_Clipboard.GetProperty("colorValue", BindingFlags.Static | BindingFlags.Public);
        private static readonly PropertyInfo PI_vector4Value = T_Clipboard.GetProperty("vector4Value", BindingFlags.Static | BindingFlags.Public);
        private static readonly PropertyInfo PI_floatValue = T_Clipboard.GetProperty("floatValue", BindingFlags.Static | BindingFlags.Public);
        private static readonly PropertyInfo PI_integerValue = T_Clipboard.GetProperty("integerValue", BindingFlags.Static | BindingFlags.Public);
        private static readonly PropertyInfo PI_guidValue = T_Clipboard.GetProperty("guidValue", BindingFlags.Static | BindingFlags.Public);
        private static readonly PropertyInfo PI_hasColor = T_Clipboard.GetProperty("hasColor", BindingFlags.Static | BindingFlags.Public);
        private static readonly PropertyInfo PI_hasVector4 = T_Clipboard.GetProperty("hasVector4", BindingFlags.Static | BindingFlags.Public);
        private static readonly PropertyInfo PI_hasFloat = T_Clipboard.GetProperty("hasFloat", BindingFlags.Static | BindingFlags.Public);
        private static readonly PropertyInfo PI_hasInteger = T_Clipboard.GetProperty("hasInteger", BindingFlags.Static | BindingFlags.Public);
        private static readonly PropertyInfo PI_hasGuid = T_Clipboard.GetProperty("hasGuid", BindingFlags.Static | BindingFlags.Public);

        public static void Copy(MaterialProperty prop)
        {
            if (prop.hasMixedValue) return;
            switch (prop.propertyType)
            {
                case ShaderPropertyType.Color: PI_colorValue.SetValue(null, prop.colorValue); break;
                case ShaderPropertyType.Vector: PI_vector4Value.SetValue(null, prop.vectorValue); break;
                case ShaderPropertyType.Float: PI_floatValue.SetValue(null, prop.floatValue); break;
                case ShaderPropertyType.Range: PI_floatValue.SetValue(null, prop.floatValue); break;
                case ShaderPropertyType.Int: PI_integerValue.SetValue(null, prop.intValue); break;
                case ShaderPropertyType.Texture: PI_guidValue.SetValue(null, AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(prop.textureValue))); break;
            }
        }

        public static void Paste(MaterialProperty prop)
        {
            switch (prop.propertyType)
            {
                case ShaderPropertyType.Color: if ((bool)PI_hasColor.GetValue(null)) prop.colorValue = (Color)PI_colorValue.GetValue(null); break;
                case ShaderPropertyType.Vector: if ((bool)PI_hasVector4.GetValue(null)) prop.vectorValue = (Vector4)PI_vector4Value.GetValue(null); break;
                case ShaderPropertyType.Float: if ((bool)PI_hasFloat.GetValue(null)) prop.floatValue = (float)PI_floatValue.GetValue(null); break;
                case ShaderPropertyType.Range: if ((bool)PI_hasFloat.GetValue(null)) prop.floatValue = (float)PI_floatValue.GetValue(null); break;
                case ShaderPropertyType.Int: if ((bool)PI_hasInteger.GetValue(null)) prop.intValue = (int)PI_integerValue.GetValue(null); break;
                case ShaderPropertyType.Texture: if ((bool)PI_hasGuid.GetValue(null)) prop.textureValue = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath((GUID)PI_guidValue.GetValue(null))) as Texture; break;
            }
        }

        public static void Reset(params MaterialProperty[] properties)
        {
            var shader = ((Material)properties[0].targets[0]).shader;
            foreach (var prop in properties)
            {
                var i = shader.FindPropertyIndex(prop.name);
                var shaderImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(shader)) as ShaderImporter;
                switch (prop.propertyType)
                {
                    case ShaderPropertyType.Color: prop.colorValue = shader.GetPropertyDefaultVectorValue(i); break;
                    case ShaderPropertyType.Vector: prop.vectorValue = shader.GetPropertyDefaultVectorValue(i); break;
                    case ShaderPropertyType.Float: prop.floatValue = shader.GetPropertyDefaultFloatValue(i); break;
                    case ShaderPropertyType.Range: prop.floatValue = shader.GetPropertyDefaultFloatValue(i); break;
                    case ShaderPropertyType.Int: prop.intValue = shader.GetPropertyDefaultIntValue(i); break;
                    case ShaderPropertyType.Texture:
                        prop.textureValue = prop.textureValue = shaderImporter ? shaderImporter.GetDefaultTexture(prop.name) : null;
                        prop.textureScaleAndOffset = new(1,1,0,0);
                        break;
                }
            }
        }

        private static readonly MethodInfo MI_GetPropertyState = typeof(Material).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(m => m.Name == "GetPropertyState" && m.GetParameters()[0].ParameterType == typeof(int));

        public static void GetPropertyState(Material material, int nameID, out bool isOverriden, out bool isLockedInChildren, out bool isLockedByAncestor)
        {
            isOverriden = false;
            isLockedInChildren = false;
            isLockedByAncestor = false;
            var args = new object[]{nameID, isOverriden, isLockedInChildren, isLockedByAncestor};
            MI_GetPropertyState.Invoke(material, args);
            isOverriden = (bool)args[1];
            isLockedInChildren = (bool)args[2];
            isLockedByAncestor = (bool)args[3];
        }
    }
}
