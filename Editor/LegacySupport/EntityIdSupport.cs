// 古いUnityでEntityIdが使えないのでその対応
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.shadercore
{
    #if !UNITY_6000_3_OR_NEWER
    internal static class ProjectWindowUtil
    {
        public static void CreateAssetWithTextContent(string filename, string content) => UnityEditor.ProjectWindowUtil.CreateAssetWithContent(filename, content);
        public static void ShowCreatedAsset(Object o) => UnityEditor.ProjectWindowUtil.ShowCreatedAsset(o);
    }

    internal static class EditorUtility
    {
        public static Object EntityIdToObject(EntityId entityId) => UnityEditor.EditorUtility.InstanceIDToObject(entityId.i);
        public static EntityId GetEntityId(this Object obj) => EntityId.FromULong((ulong)obj.GetInstanceID());
        public static void CompressTexture(Texture2D texture, TextureFormat format, TextureCompressionQuality quality) => UnityEditor.EditorUtility.CompressTexture(texture, format, quality);
        public static string SaveFilePanel(string title, string directory, string defaultName, string extension) => UnityEditor.EditorUtility.SaveFilePanel(title, directory, defaultName, extension);
        public static void DisplayCustomMenu(Rect position, GUIContent[] options, int selected, UnityEditor.EditorUtility.SelectMenuItemFunction callback, object userData) => UnityEditor.EditorUtility.DisplayCustomMenu(position, options, selected, callback, userData);
    }
    #endif

    #if !UNITY_6000_2_OR_NEWER
    internal struct EntityId
    {
        public int i;
        public static EntityId FromULong(ulong value) => new(){i = (int)value};
    }
    #endif
}
