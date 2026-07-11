using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.shadercore
{
    internal class SCPropertyContainer : VisualElement
    {
        public readonly MaterialSerializedProperty[] types;
        public readonly MaterialProperty[] properties;
        public VisualElement container;
        public override VisualElement contentContainer => container;

        public SCPropertyContainer(VisualElement root, Object[] targets, params object[] objs) : base()
        {
            types = objs.Where(p => p is MaterialSerializedProperty).Select(p => (MaterialSerializedProperty)p).ToArray();
            properties = objs.Select(p => p as MaterialProperty).Where(p => p != null).Union(objs.Where(o => o is List<MaterialProperty>).SelectMany(o => o as List<MaterialProperty>)).ToArray();

            style.flexDirection = FlexDirection.Row;
            hierarchy.Add(new SCLockProperty(root, targets, objs));

            container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.flexDirection = FlexDirection.Row;
            hierarchy.Add(container);
        }

        public void SetVisible(string search, StringComparison comparison)
        {
            style.display = string.IsNullOrEmpty(search) ||
                properties.Any(p => p.name.Contains(search, comparison)) || this.Query<Label>().Build().Any(l => l.text.Contains(search, comparison)) ||
                types.Any(t => t switch
                {
                    MaterialSerializedProperty.CustomRenderQueue => SCRenderQueueField.PATH.Contains(search, comparison),
                    MaterialSerializedProperty.EnableInstancingVariants => SCEnableInstancingField.PATH.Contains(search, comparison),
                    MaterialSerializedProperty.DoubleSidedGI => SCDoubleSidedGIField.PATH.Contains(search, comparison),
                    _ => false
                }) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
