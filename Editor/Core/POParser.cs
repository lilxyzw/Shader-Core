using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace jp.lilxyzw.shadercore
{
    internal static class POParser
    {
        private const string REG_ID = @"^\s*msgid\s*""(.*)""\s*$";
        private const string REG_STR = @"^\s*msgstr\s*""(.*)""\s*$";
        public static Dictionary<string,string> Load(string path)
        {
            var regID = new Regex(REG_ID);
            var regStr = new Regex(REG_STR);
            var dic = new Dictionary<string,string>();
            using var sr = new StreamReader(path);
            string key = "";
            string line;
            while((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('"') || line.StartsWith('#')) continue;
                var id = regID.Match(line);
                if (id.Success)
                {
                    key = id.Groups[1].Value;
                    continue;
                }
                var str = regStr.Match(line);
                if (str.Success)
                {
                    if (dic.TryAdd(key, str.Groups[1].Value)) continue;
                    throw new Exception($"Key duplication. {path}\r\n{key}");
                }
                throw new Exception($"PO file error. {path}\r\n{line}");
            }
            return dic;
        }
    }
}
