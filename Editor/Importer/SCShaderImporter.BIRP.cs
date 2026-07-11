using System.Collections.Generic;
using System.Text;

namespace jp.lilxyzw.shadercore
{
    internal partial class SCShaderImporter
    {
        private static bool ReplacePropertiesBIRP(string line, string indent, StringBuilder sb, IEnumerable<SCModule> modules)
        {
            if (!line.Contains("__SC_BIRP_properties__")) return false;
            foreach (var module in modules)
            {
                if(module.properties == null) continue;
                foreach(var prop in module.properties)
                {
                    var hlsl = ToHLSL_BIRP(prop);
                    if (string.IsNullOrEmpty(hlsl)) continue;
                    sb.Append(indent).AppendLine(hlsl);
                }
                sb.AppendLine();
            }
            return true;
        }

        private static string ToHLSL_BIRP(SCProperty prop)
        {
            if (SkipProperty(prop)) return null;
            return prop.type switch
            {
                "Texture2D" => $"{prop.type} {prop.name};",
                "Texture2DArray" => $"{prop.type} {prop.name};",
                "Texture3D" => $"{prop.type} {prop.name};",
                "TextureCube" => $"{prop.type} {prop.name};",
                "TextureCubeArray" => $"{prop.type} {prop.name};",
                "float" => $"{prop.type} {prop.name};",
                "float4" => $"{prop.type} {prop.name};",
                "uint" => $"{prop.type} {prop.name};",
                "uint4" => $"{prop.type} {prop.name};",
                "int" => $"{prop.type} {prop.name};",
                "int4" => $"{prop.type} {prop.name};",
                "color" => $"float4 {prop.name};",
                "SamplerState" => $"{prop.type} {prop.name};",
                "ScaleOffset" => $"float4 {prop.name}_ST;",
                _ => null,
            };
        }
    }
}
