using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace jp.lilxyzw.shadercore
{
    [Serializable]
    public class SCModule : IComparable
    {
        public string name;
        public string uniqueID;
        public bool keepPropertyNames;
        public List<SCPhase> phases = new();
        [NonSerialized] public List<SCProperty> properties;
        [NonSerialized] public string path;

        public static SCModule FromShaderFile(string path)
        {
            var module = new SCModule{name = "Main", path = path};
            var proppath = $"{Utils.RemoveExtension(path)}_properties.hlsl";
            module.path = path;
            if (File.Exists(proppath)) module.properties = SCProperty.FromFile(proppath, module.uniqueID);
            return module;
        }

        public static SCModule FromFile(string path)
        {
            var directory = Utils.GetDirectory(path);
            var module = JsonParser.Deserialize<SCModule>(File.ReadAllText(path));
            var exists = module.phases.Select(p => string.IsNullOrEmpty(p.path) ? $"phase_{p.phase}.hlsl" : p.path).Where(p => !string.IsNullOrEmpty(p));
            module.path = path;

            var proppath = $"{directory}properties.hlsl";
            if (File.Exists(proppath)) module.properties = SCProperty.FromFile(proppath, module.keepPropertyNames ? "" : module.uniqueID);

            module.phases.AddRange(Directory.GetFiles(directory, "phase_*.hlsl").Select(n => new SCPhase(){phase = Regex.Match(n, @"phase_(\w+)\.hlsl").Groups[1].Value}));

            foreach (var phase in module.phases)
            {
                if (string.IsNullOrEmpty(phase.name)) phase.name = module.name;
                if (string.IsNullOrEmpty(phase.path)) phase.path = $"phase_{phase.phase}.hlsl";
                phase.path = directory+phase.path;
                phase.module = module;
            }
            return module;
        }

        public Dictionary<string,string> LoadLocalization(string code)
        {
            var directory = Utils.GetDirectory(path)+"lang/";
            return LoadLocalizationDirect(code, directory);
        }

        public static Dictionary<string,string> LoadLocalizationDirect(string code, string directory)
        {
            if (File.Exists($"{directory}{code}.po")) return POParser.Load($"{directory}{code}.po");
            if (File.Exists($"{directory}en-US.po")) return POParser.Load($"{directory}en-US.po");
            return new();
        }

        public int CompareTo(object obj)
        {
            var obj2 = obj as SCModule;
            if (phases.Count == 0 && obj2.phases.Count == 0) return 0;
            if (phases.Count == 0) return -1;
            if (obj2.phases.Count == 0) return 1;
            return phases[0].CompareTo(obj2.phases[0]);
        }
    }

    [Serializable]
    public class SCPhase : IComparable
    {
        public string name;
        public string phase;
        public string path;
        public string[] befores = {};
        public string[] afters = {};
        [NonSerialized] public SCModule module;

        public void LoadHLSL(StringBuilder sb, string indent, string scaleOffsetPostfix)
        {
            if (!File.Exists(path)) throw new Exception($"File not found. {path}");
            using var sr = new StreamReader(path);
            string line;
            while((line = sr.ReadLine()) != null)
            {
                if (module.properties != null)
                    foreach (var prop in module.properties)
                        if (!string.IsNullOrEmpty(prop.originalName)) line = Regex.Replace(
                            line,
                            @"([^\w])("+prop.originalName+(prop.type=="ScaleOffset"?scaleOffsetPostfix:"")+@")([^\w])",
                            @"$1"+prop.name+(prop.type=="ScaleOffset"?scaleOffsetPostfix:"")+@"$3"
                        );
                sb.Append(indent);
                sb.AppendLine(line);
            }
        }

        public int CompareTo(object obj)
        {
            var obj2 = obj as SCPhase;
            if (string.IsNullOrEmpty(phase)) return -1;
            if (string.IsNullOrEmpty(obj2.phase)) return 1;
            if (phase != obj2.phase) return phase.CompareTo(obj2.phase);

            bool isAfter = afters.Contains(obj2.name) || obj2.befores.Contains(name);
            bool isBefore = befores.Contains(obj2.name) || obj2.afters.Contains(name);
            if (isAfter && isBefore) throw new Exception($"The priority setting is invalid. {name} and {obj2.name}");
            if (isAfter) return 1;
            if (isBefore) return -1;
            return name.CompareTo(obj2.name);
        }
    }
}
