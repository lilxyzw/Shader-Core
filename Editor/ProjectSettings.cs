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

        public static void GetShaderModules(string path, out List<string> modulenames, out List<MultiImportModuleSetting> multiModules)
        {
            var shadername = GetShaderName(path);
            if (instance.shaderSettings.FirstOrDefault(s => s.shadername == shadername) is ShaderSettings s)
            {
                modulenames = s.modules;
                multiModules = s.multiModules;
                return;
            }

            var settings = new ShaderSettings{shadername = shadername};
            var modules = AssetUtils.GetFiles("*.scmodule", Utils.GetDirectory(path)).Select(p => SCModule.FromFile(p));
            modulenames = settings.modules = modules.Where(m => m != null && (m.properties_multi == null || m.properties_multi.Count == 0)).Select(m => m.uniqueID).ToList();
            multiModules = settings.multiModules = modules.Where(m => m != null && m.properties_multi != null && m.properties_multi.Count > 0).Select(m => new MultiImportModuleSetting{name = m.uniqueID, count = 1}).ToList();

            instance.shaderSettings.Add(settings);
            instance.Save();
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
            public List<MultiImportModuleSetting> multiModules = new();
        }

        [Serializable]
        public class MultiImportModuleSetting
        {
            public string name;
            public int count;
        }
    }
}
