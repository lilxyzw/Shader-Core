using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.shadercore
{
    internal class L10n : AssetPostprocessor
    {
        private static string[] languages;
        private static string[] languageNames;
        private static readonly Regex REG_LANGCODE = new(@"[a-z]+-[A-Z][a-zA-Z\-]+");
        private static readonly Dictionary<string, Dictionary<string, string>> transrations = new();
        private static readonly Dictionary<string, Dictionary<string, GUIContent>> guicontents = new();
        private static Dictionary<string,string> transration;
        private static Dictionary<string,string> coreTransration;
        private static Dictionary<string,GUIContent> guicontent;
        private static Dictionary<string,SCModule> modules;
        private static Dictionary<string,SCModule> Modules => modules ??= AssetUtils.GetFiles("*.scmodule").Select(path => SCModule.FromFile(path)).ToDictionary(m => m.uniqueID, m => m);
        private static string lastLanguage = "";
        private static string m_CurrentID = "";
        public static string currentID => m_CurrentID;

        private static void OnPostprocessAllAssets(string[] i, string[] d, string[] m, string[] mf, bool dr)
        {
            if (i.All(i => !i.EndsWith(".po") && d.All(d => !d.EndsWith(".po")) && m.All(m => !m.EndsWith(".po")))) return;
            Clear();
            SCLocalizeEvent.Invoke();
        }

        public static void Load(string uniqueID = null)
        {
            if (lastLanguage != Settings.instance.language) Clear();
            if (string.IsNullOrEmpty(uniqueID)) uniqueID = "Main";
            if (m_CurrentID == uniqueID) return;
            m_CurrentID = uniqueID;

            if (transrations.TryGetValue(uniqueID, out var tr))
                transration = tr;
            else if (Modules.TryGetValue(uniqueID, out var module))
                transrations[uniqueID] = transration = module.LoadLocalization(Settings.instance.language);
            else
                transrations[uniqueID] = transration = SCModule.FromShaderFile(uniqueID).LoadLocalization(Settings.instance.language);

            if (guicontents.TryGetValue(uniqueID, out var gc))
                guicontent = gc;
            else
                guicontents[uniqueID] = guicontent = new();
        }

        public static void Clear()
        {
            languages = null;
            languageNames = null;
            transrations.Clear();
            guicontents.Clear();
            transration = null;
            coreTransration = null;
            guicontent = null;
            modules = null;
            lastLanguage = Settings.instance.language;
            m_CurrentID = "";
        }

        public static string[] GetLanguages()
        {
            return languages ??= AssetUtils.GetFiles("*.po").Select(p => Path.GetFileNameWithoutExtension(p)).Distinct().Where(c => !string.IsNullOrEmpty(c) && REG_LANGCODE.IsMatch(c)).ToArray();
        }

        public static string[] GetLanguageNames()
        {
            return languageNames ??= languages.Select(l => {
                if(l == "zh-Hans") return "简体中文";
                if(l == "zh-Hant") return "繁體中文";
                return new CultureInfo(l).NativeName;
            }).ToArray();
        }

        public static string L(string key)
        {
            coreTransration ??= SCModule.LoadLocalizationDirect(Settings.instance.language, "Packages/jp.lilxyzw.shadercore/lang/");
            if (key.StartsWith("__") && coreTransration.TryGetValue(key, out var value2)) return value2;
            if (transration != null && transration.TryGetValue(key, out var value)) return value;
            return key;
        }

        public static GUIContent G(string key) => G(key, null, "");
        public static GUIContent G(string key, Texture image, string tooltip)
        {
            if (transration != null && guicontent.TryGetValue(key, out var value)) return value;
            guicontent ??= new();
            return guicontent[key] = new GUIContent(L(key), image, L(tooltip));
        }
    }
}
