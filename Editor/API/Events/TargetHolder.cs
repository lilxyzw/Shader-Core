using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    public abstract class SCEventBase<T> : EventBase<T> where T : EventBase<T>, new()
    {
        public static void Invoke()
        {
            foreach (var target in TargetHolder.targets.SelectMany(t => t.Query().Build()).ToArray())
                SendEvent(target);
        }

        private static void SendEvent(VisualElement target)
        {
            using var evt = GetPooled();
            evt.target = target;
            target.SendEvent(evt);
        }
    }

    public static class TargetHolder
    {
        public static readonly HashSet<VisualElement> targets = new();
    }
}
