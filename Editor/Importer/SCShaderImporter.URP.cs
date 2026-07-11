using System.Collections.Generic;
using System.Text;

namespace jp.lilxyzw.shadercore
{
    internal partial class SCShaderImporter
    {
        private static bool ReplacePropertiesURP(string line, string indent, StringBuilder sb, IEnumerable<SCModule> modules)
        {
            if (!line.Contains("__SC_URP_properties__")) return false;

            URP_Base(indent, sb, modules);
            sb.AppendLine();

            URP_DOTS(indent, sb, modules);
            sb.AppendLine();

            URP_Textures(indent, sb, modules);
            sb.AppendLine();

            return true;
        }

        private static void URP_Base(string indent, StringBuilder sb, IEnumerable<SCModule> modules)
        {
            sb.Append(indent).AppendLine("CBUFFER_START(UnityPerMaterial)");
            foreach (var module in modules)
            {
                if(module.properties == null) continue;
                foreach(var prop in module.properties)
                {
                    if (SkipProperty(prop)) continue;
                    var hlsl = prop.type switch
                    {
                        "float" => $"{prop.type} {prop.name};",
                        "float4" => $"{prop.type} {prop.name};",
                        "uint" => $"{prop.type} {prop.name};",
                        "uint4" => $"{prop.type} {prop.name};",
                        "int" => $"{prop.type} {prop.name};",
                        "int4" => $"{prop.type} {prop.name};",
                        "color" => $"float4 {prop.name};",
                        "ScaleOffset" => $"float4 {prop.name}_ST;",
                        _ => null,
                    };
                    if (string.IsNullOrEmpty(hlsl)) continue;
                    sb.Append(indent).AppendLine(hlsl);
                }
                sb.AppendLine();
            }
            sb.Append(indent).AppendLine("CBUFFER_END");
        }

        private static void URP_Textures(string indent, StringBuilder sb, IEnumerable<SCModule> modules)
        {
            foreach (var module in modules)
            {
                if(module.properties == null) continue;
                foreach(var prop in module.properties)
                {
                    if (SkipProperty(prop)) continue;
                    var hlsl = prop.type switch
                    {
                        "Texture2D" => $"{prop.type} {prop.name};",
                        "Texture2DArray" => $"{prop.type} {prop.name};",
                        "Texture3D" => $"{prop.type} {prop.name};",
                        "TextureCube" => $"{prop.type} {prop.name};",
                        "TextureCubeArray" => $"{prop.type} {prop.name};",
                        "SamplerState" => $"{prop.type} {prop.name};",
                        _ => null,
                    };
                    if (string.IsNullOrEmpty(hlsl)) continue;
                    sb.Append(indent).AppendLine(hlsl);
                }
                sb.AppendLine();
            }
        }

        private static void URP_DOTS(string indent, StringBuilder sb, IEnumerable<SCModule> modules)
        {
            sb.Append(indent).AppendLine("#ifdef UNITY_DOTS_INSTANCING_ENABLED");
            sb.AppendLine();

            sb.Append(indent).AppendLine("UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)");
            foreach (var module in modules)
            {
                if(module.properties == null) continue;
                foreach(var prop in module.properties)
                {
                    if (SkipProperty(prop)) continue;
                    var hlsl = prop.type switch
                    {
                        "float" => $"UNITY_DOTS_INSTANCED_PROP({prop.type},{prop.name})",
                        "float4" => $"UNITY_DOTS_INSTANCED_PROP({prop.type},{prop.name})",
                        "uint" => $"UNITY_DOTS_INSTANCED_PROP({prop.type},{prop.name})",
                        "uint4" => $"UNITY_DOTS_INSTANCED_PROP({prop.type},{prop.name})",
                        "int" => $"UNITY_DOTS_INSTANCED_PROP({prop.type},{prop.name})",
                        "int4" => $"UNITY_DOTS_INSTANCED_PROP({prop.type},{prop.name})",
                        "color" => $"UNITY_DOTS_INSTANCED_PROP(float4,{prop.name})",
                        //"ScaleOffset" => $"UNITY_DOTS_INSTANCED_PROP(float4,{prop.name}_ST)",
                        _ => null,
                    };
                    if (string.IsNullOrEmpty(hlsl)) continue;
                    sb.Append(indent).AppendLine(hlsl);
                }
                sb.AppendLine();
            }
            sb.Append(indent).AppendLine("UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)");
            sb.AppendLine();

            foreach (var module in modules)
            {
                if(module.properties == null) continue;
                foreach(var prop in module.properties)
                {
                    if (SkipProperty(prop)) continue;
                    var hlsl = prop.type switch
                    {
                        "float" => $"static {prop.type} unity_DOTS_Sampled{prop.name};",
                        "float4" => $"static {prop.type} unity_DOTS_Sampled{prop.name};",
                        "uint" => $"static {prop.type} unity_DOTS_Sampled{prop.name};",
                        "uint4" => $"static {prop.type} unity_DOTS_Sampled{prop.name};",
                        "int" => $"static {prop.type} unity_DOTS_Sampled{prop.name};",
                        "int4" => $"static {prop.type} unity_DOTS_Sampled{prop.name};",
                        "color" => $"static float4 unity_DOTS_Sampled{prop.name};",
                        //"ScaleOffset" => $"static float4 unity_DOTS_Sampled{prop.name}_ST;",
                        _ => null,
                    };
                    if (string.IsNullOrEmpty(hlsl)) continue;
                    sb.Append(indent).AppendLine(hlsl);
                }
                sb.AppendLine();
            }
            sb.AppendLine();

            sb.Append(indent).AppendLine("void SetupDOTSLitMaterialPropertyCaches()");
            sb.Append(indent).AppendLine("{");
            foreach (var module in modules)
            {
                if(module.properties == null) continue;
                foreach(var prop in module.properties)
                {
                    if (SkipProperty(prop)) continue;
                    var hlsl = prop.type switch
                    {
                        "float" => $"unity_DOTS_Sampled{prop.name} = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT({prop.type}, {prop.name});",
                        "float4" => $"unity_DOTS_Sampled{prop.name} = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT({prop.type}, {prop.name});",
                        "uint" => $"unity_DOTS_Sampled{prop.name} = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT({prop.type}, {prop.name});",
                        "uint4" => $"unity_DOTS_Sampled{prop.name} = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT({prop.type}, {prop.name});",
                        "int" => $"unity_DOTS_Sampled{prop.name} = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT({prop.type}, {prop.name});",
                        "int4" => $"unity_DOTS_Sampled{prop.name} = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT({prop.type}, {prop.name});",
                        "color" => $"unity_DOTS_Sampled{prop.name} = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, {prop.name});",
                        //"ScaleOffset" => $"unity_DOTS_Sampled{prop.name}_ST = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, {prop.name}_ST);",
                        _ => null,
                    };
                    if (string.IsNullOrEmpty(hlsl)) continue;
                    sb.Append(indent).AppendLine(hlsl);
                }
                sb.AppendLine();
            }
            sb.Append(indent).AppendLine("}");
            sb.AppendLine();

            sb.Append(indent).AppendLine("#undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES");
            sb.Append(indent).AppendLine("#define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSLitMaterialPropertyCaches()");
            sb.AppendLine();

            foreach (var module in modules)
            {
                if(module.properties == null) continue;
                foreach(var prop in module.properties)
                {
                    if (SkipProperty(prop)) continue;
                    var hlsl = prop.type switch
                    {
                        "float" => $"#define {prop.name} unity_DOTS_Sampled{prop.name}",
                        "float4" => $"#define {prop.name} unity_DOTS_Sampled{prop.name}",
                        "uint" => $"#define {prop.name} unity_DOTS_Sampled{prop.name}",
                        "uint4" => $"#define {prop.name} unity_DOTS_Sampled{prop.name}",
                        "int" => $"#define {prop.name} unity_DOTS_Sampled{prop.name}",
                        "int4" => $"#define {prop.name} unity_DOTS_Sampled{prop.name}",
                        "color" => $"#define {prop.name} unity_DOTS_Sampled{prop.name}",
                        //"ScaleOffset" => $"#define {prop.name}_ST unity_DOTS_Sampled{prop.name}_ST",
                        _ => null,
                    };
                    if (string.IsNullOrEmpty(hlsl)) continue;
                    sb.Append(indent).AppendLine(hlsl);
                }
                sb.AppendLine();
            }
            sb.Append(indent).AppendLine("#endif");
        }
    }
}
