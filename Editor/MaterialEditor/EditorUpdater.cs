using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.shadercore
{
    // MaterialEditorがVisualElementに対応していないのでインスペクターの更新処理を入れる
    internal static class EditorUpdater
    {
        public readonly static Type T_PropertyEditor = typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor");
        private readonly static FieldInfo FI_m_InspectorMode = T_PropertyEditor.GetField("m_InspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly static MethodInfo MI_SetMode = T_PropertyEditor.GetMethod("SetMode", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void UpdateAllPropertyEditors()
        {
            foreach (var e in Resources.FindObjectsOfTypeAll(T_PropertyEditor))
            {
                if ((InspectorMode)FI_m_InspectorMode.GetValue(e) != InspectorMode.Normal) continue;
                MI_SetMode.Invoke(e, new object[]{InspectorMode.Debug});
                MI_SetMode.Invoke(e, new object[]{InspectorMode.Normal});
            }
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Editor.finishedDefaultHeaderGUI += (e) =>
            {
                if (e is not MaterialEditor me || e.target is not Material m || m.shader is not Shader s) return;
                var editor = ShaderUtil.GetCurrentCustomEditor(s);
                if (editor != "SCMaterialEditor" && e is SCMaterialEditor || editor == "SCMaterialEditor" && e is not SCMaterialEditor) UpdateAllPropertyEditors();
            };

            EditorApplication.update += () =>
            {
                if (!EditorUpdaterOnImport.needToUpdate) return;
                EditorUpdaterOnImport.needToUpdate = false;
                foreach (var e in Resources.FindObjectsOfTypeAll<SCMaterialEditor>())
                    e.UpdateInspectorGUI();
            };

            Selection.selectionChanged += () =>
            {
                // Only the editors whose material is actually selected need a refresh.
                // Iterating every live SCMaterialEditor (including ones that are no longer
                // shown) made each selection change cost several seconds once a few dozens
                // of material inspectors had been opened during the session.
                var selected = Selection.objects;
                if (selected == null || selected.Length == 0) return;
                foreach (var e in Resources.FindObjectsOfTypeAll<SCMaterialEditor>())
                {
                    if (e == null || e.target == null) continue;
                    if (Array.IndexOf(selected, e.target) < 0) continue;
                    e.UpdateInspectorGUI();
                }
            };
        }
    }

    internal class EditorUpdaterOnImport : AssetPostprocessor
    {
        public static bool needToUpdate = false;
        private static void OnPostprocessAllAssets(string[] i, string[] d, string[] m, string[] mf, bool dr)
        {
            needToUpdate = true;
        }
    }
}
