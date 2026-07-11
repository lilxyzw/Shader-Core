using System.Globalization;
using UnityEditor;

namespace jp.lilxyzw.shadercore
{
    [FilePath("jp.lilxyzw/shadercore.asset", FilePathAttribute.Location.PreferencesFolder)]
    internal class Settings : ScriptableSingleton<Settings>
    {
        public string language = CultureInfo.CurrentCulture.Name;
        internal void Save() => Save(true);
    }
}
