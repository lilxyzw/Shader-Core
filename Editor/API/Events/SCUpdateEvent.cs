using UnityEditor;

namespace jp.lilxyzw.shadercore
{
    public class SCUpdateEvent : SCEventBase<SCUpdateEvent>
    {
        [InitializeOnLoadMethod]
        private static void SetCallback()
        {
            Undo.undoRedoPerformed -= Invoke;
            Undo.undoRedoPerformed += Invoke;
        }
    }
}
