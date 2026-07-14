using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace jp.lilxyzw.shadercore
{
    internal static class AssetUtils
    {
        // AssetDatabaseを使わないアセットパス取得
        public static IEnumerable<string> GetFiles(string searchPattern)
        {
            var directories = new List<string>(){"Assets/", "Packages/", "Library/PackageCache"};
            if (File.Exists("Packages/manifest.json"))
            {
                var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<PackageManifest>(File.ReadAllText("Packages/manifest.json"));
                var directory = "Packages";
                directories.AddRange(manifest.dependencies.Select(kv => kv.Value).Where(p => p.StartsWith("file:")).Select(p => {
                    try
                    {
                        var path = p[5..];
                        if (Path.IsPathRooted(path)) return path;
                        return "Packages/" + path;
                    }
                    catch
                    {
                        UnityEngine.Debug.LogError($"Invalid package path. {p}");
                        return null;
                    }
                }).Where(p => !string.IsNullOrEmpty(p)));
            }
            return directories.SelectMany(d => Directory.GetFiles(d, searchPattern, SearchOption.AllDirectories)).Select(p => p.Replace('\\', '/'));
        }

        public static IEnumerable<string> GetFiles(string searchPattern, string directory)
        {
            return Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories).Select(p => p.Replace('\\', '/'));
        }

        [Serializable]
        private class PackageManifest
        {
            public Dictionary<string,string> dependencies = new();
        }
    }
}
