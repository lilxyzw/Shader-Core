using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    public abstract class SCEventBase<T> : EventBase<T> where T : EventBase<T>, new()
    {
        public static void Invoke()
        {
            // Inspectors that were closed (or editors that leaked) never remove their root
            // from this static set, so prune detached element trees before walking them.
            TargetHolder.targets.RemoveWhere(t => t == null || t.panel == null);
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
