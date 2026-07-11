using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jp.lilxyzw.shadercore
{
    internal partial class SCShaderImporter
    {
        private static bool ReplaceProperties(string line, string indent, StringBuilder sb, IEnumerable<SCModule> modules)
        {
            if (!line.Contains("__SC_SHADERLAB_properties__")) return false;
            foreach (var module in modules)
            {
                if(module.properties == null) continue;
                sb.Append(indent);
                sb.Append($"[SCModule({module.uniqueID})]");
                sb.AppendLine($"[SCFoldout({(module.name == "Main" ? "__Main" : module.name)})]");
                var scaleOffsets = module.properties.Where(p => p.type == "ScaleOffset");
                foreach(var prop in module.properties)
                {
                    var sl = ToShaderLab(prop);
                    if (string.IsNullOrEmpty(sl)) continue;
                    sb.Append(indent).Append("    ");
                    if((prop.type == "Texture2D" || prop.type == "Texture2DArray") && scaleOffsets.All(s => s.name != prop.name)) sb.Append("[NoScaleOffset]");
                    sb.AppendLine(sl);
                    if (sl == "[SCBoxEnd]" || sl == "[SCFoldoutEnd]") sb.AppendLine();
                }
                sb.Append(indent);
                sb.AppendLine("[SCFoldoutEnd]").AppendLine();
            }
            return true;
        }

        public static string ToShaderLab(SCProperty prop)
        {
            switch(prop.type)
            {
                case "Texture2D":
                case "Texture2DArray":
                case "Texture3D":
                case "TextureCube":
                case "TextureCubeArray":
                    return $"{ToShaderLab(prop.attributes)}{prop.name} ({prop.displayname}, {ToShaderLabType(prop)}) = {prop.defaultvalue} {{}}";
                case "float":
                case "float4":
                case "uint":
                case "uint4":
                case "int":
                case "int4":
                case "color":
                    return $"{ToShaderLab(prop.attributes)}{prop.name} ({prop.displayname}, {ToShaderLabType(prop)}) = {prop.defaultvalue}";
                case "Box": return "[SCBox]";
                case "BoxEnd": return "[SCBoxEnd]";
                case "Foldout": return $"[SCFoldout({prop.name})]";
                case "FoldoutEnd": return "[SCFoldoutEnd]";
                default: return null;
            }
        }

        private static string ToShaderLab(List<string> attributes)
        {
            var sb = new StringBuilder();
            foreach (var attr in attributes)
            {
                if (attr == "[SCMainTexture]") sb.Append("[MainTexture]");
                else sb.Append(attr.Replace('-', '_'));
            }
            if(sb.Length > 0) sb.Append(' ');
            return sb.ToString();
        }

        private static string ToShaderLabType(SCProperty prop)
        {
            return prop.type switch
            {
                "Texture2D" => "2D",
                "Texture2DArray" => "2DArray",
                "Texture3D" => "3D",
                "TextureCube" => "Cubemap",
                "TextureCubeArray" => "CubemapArray",
                "float" => "Float",
                "float4" => "Vector",
                "uint" => "Integer",
                "uint4" => "Vector",
                "int" => "Integer",
                "int4" => "Vector",
                "color" => "Color",
                _ => null,
            };
        }
    }
}
