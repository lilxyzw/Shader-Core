using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCRenderQueueField : VisualElement, IMaterialOtherPropertyElement
    {
        public Object[] Targets { get; set; }
        public MaterialSerializedProperty Property => MaterialSerializedProperty.CustomRenderQueue;
        public string LocalizationKey => "__RenderQueue";
        public static string PATH => "m_CustomRenderQueue";

        private static readonly List<int> definedQueues = new(){-1, 2000, 2450, 3000};
        private static readonly string[] definedQueueNames = {"From Shader", "Geometry", "AlphaTest", "Transparent"};
        private static readonly PropertyInfo PI_rawRenderQueue = typeof(Material).GetProperty("rawRenderQueue", BindingFlags.Instance | BindingFlags.NonPublic);
        private int rawRenderQueue => (int)PI_rawRenderQueue.GetValue(Targets[0]);

        public SCRenderQueueField(Object[] targets)
        {
            visible = SupportedRenderingFeatures.active.editableMaterialRenderQueue;
            style.flexDirection = FlexDirection.Row;

            var queueNames = definedQueueNames.Select(l => L10n.L(l)).ToList();
            string GetLabel(int q)
            {
                var index = definedQueues.IndexOf(q);
                if (index >= 0) return queueNames[index];

                var closest = 2000;
                for (int i = 2; i < definedQueues.Count; i++)
                    if (Mathf.Abs(definedQueues[i] - q) < Mathf.Abs(closest - q)) closest = definedQueues[i];
                var diff = q-closest;
                var label = queueNames[definedQueues.IndexOf(closest)];
                return diff >= 0 ? $"{label}+{diff}" : $"{label}{diff}";
            }

            var material = targets[0] as Material;
            var popup = new PopupField<int>(PATH, definedQueues, -1, GetLabel, GetLabel);
            SCStyles.ApplyPopupStyle(popup);
            popup.style.flexGrow = 1;
            Add(popup);
            var integer = new SCRenderQueueIntField(material);
            integer.style.flexGrow = 1;
            Add(integer);

            if (enabledSelf)
            {
                popup.bindingPath = PATH;
                integer.bindingPath = PATH;
            }
            else
            {
                popup.value = rawRenderQueue;
                integer.value = material.renderQueue;
            }

            ((IMaterialOtherPropertyElement)this).InitializeVisualElement(this, targets, popup.labelElement);
        }

        private class SCRenderQueueIntField : IntegerField
        {
            private Material material;

            public SCRenderQueueIntField(Material material) : base()
            {
                this.material = material;
            }

            protected override string ValueToString(int v)
            {
                return base.ValueToString(v == -1 ? material.renderQueue : v);
            }
        }
    }
}
