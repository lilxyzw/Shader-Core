using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.PackageManager;

namespace jp.lilxyzw.shadercore
{
    internal static class AssetUtils
    {
        // AssetDatabaseを使わないアセットパス取得
        public static IEnumerable<string> GetFiles(string searchPattern)
        {
            var directories = new List<string>(){"Assets/"};
            foreach (var package in PackageInfo.GetAllRegisteredPackages())
            {
                if (package.source != PackageSource.BuiltIn && package.resolvedPath != null && Directory.Exists(package.resolvedPath))
                {
                    directories.Add(package.resolvedPath);
                }
            }
            return directories.SelectMany(d => Directory.GetFiles(d, searchPattern, SearchOption.AllDirectories)).Select(p => p.Replace('\\', '/'));
        }

        public static IEnumerable<string> GetFiles(string searchPattern, string directory)
        {
            return Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories).Select(p => p.Replace('\\', '/'));
        }
    }
}
