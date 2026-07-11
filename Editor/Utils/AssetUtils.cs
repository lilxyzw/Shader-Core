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
            return Directory.GetFiles("Assets/", searchPattern, SearchOption.AllDirectories)
                .Union(Directory.GetFiles("Packages/", searchPattern, SearchOption.AllDirectories))
                .Union(Directory.GetFiles("Library/PackageCache", searchPattern, SearchOption.AllDirectories))
                .Select(p => p.Replace('\\', '/'));
        }

        public static IEnumerable<string> GetFiles(string searchPattern, string directory)
        {
            return Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories).Select(p => p.Replace('\\', '/'));
        }
    }
}
