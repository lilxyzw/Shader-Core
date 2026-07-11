using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Rendering;

namespace jp.lilxyzw.shadercore
{
    internal static class PropertyClipboard
    {
        private static readonly FieldInfo FI_m_GUID = typeof(ObjectIdentifier).GetField("m_GUID", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo FI_m_LocalIdentifierInFile = typeof(ObjectIdentifier).GetField("m_LocalIdentifierInFile", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo FI_m_FileType = typeof(ObjectIdentifier).GetField("m_FileType", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void ToClipboard(params MaterialProperty[] properties)
        {
            GUIUtility.systemCopyBuffer = SCPropertyClipboard.ToText(
                properties.Select(p =>
                {
                    var property = new SCPropertyClipboard();
                    property.name = p.name;
                    switch (p.propertyType)
                    {
                        case ShaderPropertyType.Float:
                        case ShaderPropertyType.Range:
                            property.type = "float";
                            property.x = p.floatValue;
                            break;
                        case ShaderPropertyType.Vector:
                        case ShaderPropertyType.Color:
                            property.type = "float4";
                            property.x = p.vectorValue.x;
                            property.y = p.vectorValue.y;
                            property.z = p.vectorValue.z;
                            property.w = p.vectorValue.w;
                            break;
                        case ShaderPropertyType.Int:
                            property.type = "int";
                            property.ix = p.intValue;
                            break;
                        case ShaderPropertyType.Texture:
                            property.type = "reference";
                            property.reference = ObjectToText(p.textureValue);
                            property.x = p.textureScaleAndOffset.x;
                            property.y = p.textureScaleAndOffset.y;
                            property.z = p.textureScaleAndOffset.z;
                            property.w = p.textureScaleAndOffset.w;
                            break;
                    }
                    return property;
            }));
        }

        public static void FromClipboard(params MaterialProperty[] properties)
        {
            var scprops = SCPropertyClipboard.FromText(GUIUtility.systemCopyBuffer);
            foreach (var prop in properties)
            {
                var scprop = scprops.FirstOrDefault(p => p.name == prop.name);
                if (scprop.name != prop.name) continue;
                switch (prop.propertyType)
                {
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        if (scprop.type == "float") prop.floatValue = scprop.x;
                        break;
                    case ShaderPropertyType.Vector:
                    case ShaderPropertyType.Color:
                        if (scprop.type == "float4") prop.vectorValue = new(scprop.x,scprop.y,scprop.z,scprop.w);
                        break;
                    case ShaderPropertyType.Int:
                        if (scprop.type == "int") prop.intValue = scprop.ix;
                        break;
                    case ShaderPropertyType.Texture:
                        if (scprop.type == "reference" && scprop.reference.StartsWith("Unity,"))
                        {
                            prop.textureValue = TextToObject(scprop.reference) as Texture;
                            prop.textureScaleAndOffset = new(scprop.x,scprop.y,scprop.z,scprop.w);
                        }
                        break;
                }
            }
        }

        public static void Revert(params MaterialProperty[] properties)
        {
            var targets = properties[0].targets;
            string displayName;
            if (properties.Length != 1) displayName = $"{properties.Length} properties";
            else displayName = properties[0].displayName;
            string text = targets.Length == 0 ? targets[0].name : (targets.Length + " Materials");
            Undo.RecordObjects(targets, $"Revert {displayName} of {text}");
            foreach (var prop in properties)
                foreach (Material material in targets)
                    material.RevertPropertyOverride(prop.name);
        }

        public static void ApplyToParent(params MaterialProperty[] properties)
        {
            foreach (var prop in properties)
                foreach (Material material in properties[0].targets)
                {
                    var parent = material;
                    while (parent = parent.parent)
                        if (!AssetDatabase.IsForeignAsset(parent)) material.ApplyPropertyOverride(parent, prop.name);
                }
        }

        private static string ObjectToText(Object obj)
        {
            if (!obj) return "null";
            var instanceID = obj.GetEntityId();
            if (ObjectIdentifier.TryGetObjectIdentifier(obj, out var objectId))
            {
                return $"Unity,{objectId.guid},{objectId.localIdentifierInFile},{(int)objectId.fileType},{instanceID}";
            }
            else
            {
                return $"Unity,{instanceID}";
            }
        }

        private static Object TextToObject(string reference)
        {
            if (reference == "null") return null;

            var values = reference.Split(',');
            if (values[0] != "Unity") return null;

            if (values.Length == 2)
            {
                return EditorUtility.EntityIdToObject(EntityId.FromULong(ulong.Parse(values[1])));
            }
            else if (values.Length == 5)
            {
                var id = new ObjectIdentifier();
                var refId = __makeref(id);
                FI_m_GUID.SetValueDirect(refId, new GUID(values[1]));
                FI_m_LocalIdentifierInFile.SetValueDirect(refId, long.Parse(values[2]));
                FI_m_FileType.SetValueDirect(refId, int.Parse(values[3]));
                if (ObjectIdentifier.ToObject(id) is Object obj) return obj;
                return EditorUtility.EntityIdToObject(EntityId.FromULong(ulong.Parse(values[4])));
            }
            return null;
        }
    }
}
