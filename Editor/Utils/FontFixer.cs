using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal static class FontFixer
    {
        // UIElementsで日本語フォントが壊れているため生成して上書き
        private static bool isInitialized = false;
        private static FontAsset m_FontAsset = null;
        private static FontAsset FontAsset => m_FontAsset ? m_FontAsset : m_FontAsset = InitializeFontAsset();
        private static FontDefinition fontDefinition = FontDefinition.FromSDFFont(FontAsset);

        private static FontAsset InitializeFontAsset()
        {
            if (isInitialized) return m_FontAsset;
            isInitialized = true;

            AddFont(FontAsset.CreateFontAsset(EditorStyles.standardFont));

            var allFonts = Font.GetOSInstalledFontNames();
            foreach (var fontName in EditorStyles.standardFont.fontNames)
                if (allFonts.Contains(fontName)) 
                    AddFont(FontAsset.CreateFontAsset(fontName, ""));

            return m_FontAsset;
        }

        private static void AddFont(FontAsset fontAsset)
        {
            if (m_FontAsset)
            {
                m_FontAsset.fallbackFontAssetTable.Add(fontAsset);
                return;
            }

            m_FontAsset = fontAsset;
            m_FontAsset.fallbackFontAssetTable = new List<FontAsset>();
        }

        public static void FixFont(this VisualElement element)
        {
            if (FontAsset) element.style.unityFontDefinition = fontDefinition;
        }
    }
}
