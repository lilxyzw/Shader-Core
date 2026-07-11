using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.shadercore
{
    public class SCKeyEvent : SCEventBase<SCKeyEvent>
    {
        private static readonly FieldInfo FI_globalEventHandler = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly HashSet<KeyCode> downKeys = new();

        public KeyCode keyCode;
        public bool isDown;

        [InitializeOnLoadMethod]
        private static void SetCallback()
        {
            var value = (EditorApplication.CallbackFunction)FI_globalEventHandler.GetValue(null);
            value -= DoKey;
            value += DoKey;
            FI_globalEventHandler.SetValue(null, value);
        }

        private static void DoKey()
        {
            if (Event.current == null) return;
            if (Event.current.type == EventType.KeyDown && downKeys.Add(Event.current.keyCode) || Event.current.type == EventType.KeyUp && downKeys.Remove(Event.current.keyCode))
                Invoke();
        }

        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        public SCKeyEvent() => LocalInit();

        private void LocalInit()
        {
            if (Event.current != null)
            {
                keyCode = Event.current.keyCode;
                isDown = Event.current.type == EventType.KeyDown;
            }
            else
            {
                keyCode = KeyCode.None;
                isDown = true;
            }
        }
    }
}
