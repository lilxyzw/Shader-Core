using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace jp.lilxyzw.shadercore
{
    public class SCProperty
    {
        public string type;
        public string name;
        public string originalName;
        public string defaultvalue;
        public List<string> attributes;
        public string displayname;
        public string description;

        private const string REG_START = @"^\s*SC_";
        private const string REG_END = @"\s*$";
        private const string REG_PAR_START = @"\s*\(\s*";
        private const string REG_PAR_END = @"\s*\)";
        private const string REG_SEPARATOR = @"\s*,\s*";

        private const string REG_variable = @"\w+";
        private const string REG_num = @"[\d\.\-]+";
        private const string REG_vector = @"\([\d\.\-,]*\)";
        private const string REG_string = @"""[^""]*""";

        private const string REG_type = "(" + REG_variable + ")";
        private const string REG_name = "(" + REG_variable + ")";
        private const string REG_defaultvalue = "(" + REG_num + "|" + REG_vector + "|" + REG_string + ")";
        private const string REG_attributes = @"(\[[^\[\]]*\]\s*)*";
        private const string REG_displayname = "(" + REG_string + ")";
        private const string REG_description = "(" + REG_string + ")";

        private const string REG_Property = REG_START + REG_type + REG_PAR_START + REG_name + REG_SEPARATOR + REG_defaultvalue + REG_SEPARATOR + REG_attributes + REG_SEPARATOR + REG_displayname + REG_SEPARATOR + REG_description + REG_PAR_END + REG_END;
        private const string REG_SamplerState = REG_START + @"SamplerState\(\s*(" + REG_variable + @")\s*\)" + REG_END;
        private const string REG_ScaleOffset = REG_START + @"ScaleOffset\(\s*(" + REG_variable + @")\s*\)" + REG_END;
        private const string REG_Box = REG_START + @"Box" + REG_END;
        private const string REG_BoxEnd = REG_START + @"BoxEnd" + REG_END;
        private const string REG_Foldout = REG_START + @"Foldout\(([^(())]*)\)" + REG_END;
        private const string REG_FoldoutEnd = REG_START + @"FoldoutEnd" + REG_END;

        public static List<SCProperty> FromFile(string path, string uniqueID)
        {
            if (!string.IsNullOrEmpty(uniqueID)) uniqueID = "_" + uniqueID.Replace('.', '_');
            using var sr = new StreamReader(path);
            var props = new List<SCProperty>();
            string line;
            while ((line = sr.ReadLine()) != null)
                if (Parse(line, uniqueID) is SCProperty property) props.Add(property);
            return props;
        }

        private static SCProperty Parse(string line, string uniqueID)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            var regex = new Regex(REG_Property).Match(line);
            if (regex.Success)
            {
                return new SCProperty
                {
                    type = regex.Groups[1].Value,
                    name = uniqueID + regex.Groups[2].Value,
                    originalName = regex.Groups[2].Value,
                    defaultvalue = regex.Groups[3].Value,
                    attributes = regex.Groups[4].Captures.Select(c => c.Value).Where(v => v != "[]").ToList(),
                    displayname = regex.Groups[5].Value,
                    description = regex.Groups[6].Value
                };
            }

            if (new Regex(REG_Box).Match(line).Success)
                return new SCProperty{type = "Box"};

            if (new Regex(REG_BoxEnd).Match(line).Success)
                return new SCProperty{type = "BoxEnd"};

            var foldout = new Regex(REG_Foldout).Match(line);
            if (foldout.Success)
                return new SCProperty{type = "Foldout", name = foldout.Groups[1].Value};

            if (new Regex(REG_FoldoutEnd).Match(line).Success)
                return new SCProperty{type = "FoldoutEnd"};

            var samplerState = new Regex(REG_SamplerState).Match(line);
            if (samplerState.Success)
                return new SCProperty{type = "SamplerState", name = uniqueID + samplerState.Groups[1].Value, originalName = samplerState.Groups[1].Value};

            var scaleOffset = new Regex(REG_ScaleOffset).Match(line);
            if (scaleOffset.Success)
                return new SCProperty{type = "ScaleOffset", name = uniqueID + scaleOffset.Groups[1].Value, originalName = scaleOffset.Groups[1].Value};

            throw new Exception($"Property error. {line}");
        }
    }
}
