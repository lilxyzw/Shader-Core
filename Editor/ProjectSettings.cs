using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.shadercore
{
    [FilePath("ProjectSettings/jp.lilxyzw.shadercore.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class ProjectSettings : ScriptableSingleton<ProjectSettings>
    {
        [SerializeField] private List<ShaderSettings> shaderSettings = new();
        internal void Save() => Save(true);

        public static List<string> GetShaderModules(string path)
        {
            var shadername = GetShaderName(path);
            if (instance.shaderSettings.FirstOrDefault(s => s.shadername == shadername) is ShaderSettings s) return s.modules;
            var settings = new ShaderSettings()
            {
                shadername = shadername,
                modules = AssetUtils.GetFiles("*.scmodule", Utils.GetDirectory(path)).Select(p => SCModule.FromFile(p)).Where(m => m != null).Select(m => m.uniqueID).ToList()
            };
            instance.shaderSettings.Add(settings);
            instance.Save();
            return settings.modules;
        }

        private static readonly Regex REG_SHADERNAME = new(@"^\s*Shader\s+""([\w\s\/]+)""");
        private static string GetShaderName(string path)
        {
            var match = REG_SHADERNAME.Match(File.ReadAllText(path));
            if (!match.Success) throw new Exception($"Shader name not defined. \"{path}\"");
            return match.Groups[1].Value;
        }

        [Serializable]
        private class ShaderSettings
        {
            public string shadername;
            public List<string> modules = new();
        }
    }
}
