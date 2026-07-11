using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.shadercore
{
    internal class SCLockProperty : Image
    {
        private static readonly Texture TEXTURE_LOCKCHILDREN = EditorGUIUtility.IconContent("HierarchyLock").image;
        private const string TOOLTIP_LOCKCHILDREN = "Locked properties cannot be overriden by a child.";
        private static readonly Texture TEXTURE_LOCKANCESTOR = EditorGUIUtility.IconContent("IN LockButton on").image;
        private const string TOOLTIP_LOCKANCESTOR = "This property is set and locked by an ancestor.";
        private static MethodInfo MI_SetPropertyLock_Serialized = typeof(Material).GetMethod("SetPropertyLock_Serialized", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly Object[] targets;
        private readonly MaterialSerializedProperty[] types;
        private readonly MaterialProperty[] properties;
        private bool isLockedInChildren;
        private bool isLockedByAncestor;
        private bool isOverriden;

        public SCLockProperty(VisualElement parent, Object[] targets, params object[] objs)
        {
            this.targets = targets;
            types = objs.Where(p => p is MaterialSerializedProperty).Select(p => (MaterialSerializedProperty)p).ToArray();
            properties = objs.Select(p => p as MaterialProperty).Where(p => p != null).Union(objs.Where(o => o is List<MaterialProperty>).SelectMany(o => o as List<MaterialProperty>)).ToArray();

            UpdateUI();
            Undo.undoRedoPerformed += UpdateUI;
            RegisterCallback<DetachFromPanelEvent>(_ => Undo.undoRedoPerformed -= UpdateUI);

            style.width = 16;
            style.marginLeft = -8;
            var p = parent;
            Offset(p);
            while((p = p.parent) != null) Offset(p);
            style.marginRight = - style.width.value.value - style.marginLeft.value.value;

            if (isLockedByAncestor)
            {
                image = TEXTURE_LOCKANCESTOR;
                tooltip = L10n.L(TOOLTIP_LOCKANCESTOR);
                this.AddManipulator(new Clickable(e =>
                {
                    if (e.eventTypeId == EventBase<MouseUpEvent>.TypeId() && ((IMouseEvent)e).button == 0 || (e.eventTypeId == EventBase<PointerUpEvent>.TypeId() || e.eventTypeId == EventBase<ClickEvent>.TypeId()) && ((IPointerEvent)e).button == 0)
                    {
                        GotoLockOrigin();
                    }
                }));
            }
            else
            {
                image = TEXTURE_LOCKCHILDREN;
                tooltip = L10n.L(TOOLTIP_LOCKCHILDREN);
                RegisterCallback<MouseEnterEvent>(e =>
                {
                    if (!isLockedInChildren) style.opacity = 0.5f;
                });
                RegisterCallback<MouseLeaveEvent>(e =>
                {
                    if (!isLockedInChildren) style.opacity = 0.0f;
                });
                this.AddManipulator(new Clickable(e =>
                {
                    if (e.eventTypeId == EventBase<MouseUpEvent>.TypeId() && ((IMouseEvent)e).button == 0 || (e.eventTypeId == EventBase<PointerUpEvent>.TypeId() || e.eventTypeId == EventBase<ClickEvent>.TypeId()) && ((IPointerEvent)e).button == 0)
                    {
                        Lock(isLockedInChildren = !isLockedInChildren);
                        style.opacity = isLockedInChildren ? 1.0f : 0.5f;
                    }
                }));
            }
        }

        private void UpdateUI()
        {
            isLockedInChildren = false;
            isLockedByAncestor = false;
            isOverriden = true;
            if(properties.Length > 0)
            {
                int nameID = Shader.PropertyToID(properties[0].name);
                foreach (Material material in targets)
                {
                    MaterialPropertyUtils.GetPropertyState(material, nameID, out var flag, out var flag2, out var flag3);
                    isLockedInChildren |= flag2;
                    isLockedByAncestor |= flag3;
                    isOverriden &= flag;
                }
            }
            if(types.Length > 0)
            {
                foreach (Material material in targets)
                {
                    MaterialPropertyUtils.GetPropertyState(material, (int)types[0], out var flag, out var flag2, out var flag3);
                    isLockedInChildren |= flag2;
                    isLockedByAncestor |= flag3;
                    isOverriden &= flag;
                }
            }
            if (!isLockedByAncestor) style.opacity = isLockedInChildren ? 1.0f : 0.0f;
        }

        private void Offset(VisualElement p)
        {
            if (p is SCFoldout) style.marginLeft = style.marginLeft.value.value - 16f;
            else style.marginLeft = style.marginLeft.value.value - p.style.marginLeft.value.value - p.style.borderLeftWidth.value - p.style.paddingLeft.value.value;
        }

        private void Lock(bool lockValue)
        {
            string lockName = lockValue ? "locking" : "unlocking";
            string displayName;
            int num = properties.Length + types.Length;
            if (num != 1) displayName = $"{num} properties";
            else if (properties.Length != 0) displayName = properties[0].displayName;
            else displayName = types[0].ToString();
            string materialName = (targets.Length == 1) ? targets[0].name : (targets.Length + " Materials");

            Undo.RecordObjects(targets, $"{lockName} {displayName} of {materialName}");
            foreach (Material material in targets)
            {
                foreach (var property in properties)
                    material.SetPropertyLock(property.name, lockValue);
                foreach (var type in types)
                    MI_SetPropertyLock_Serialized.Invoke(material, new object[]{(int)type, lockValue});
            }
            style.opacity = lockValue ? 1.0f : 0.0f;
        }

        private void GotoLockOrigin()
        {
            var material = targets[0] as Material;
            while (material = material.parent)
            {
                foreach (var property in properties)
                {
                    MaterialPropertyUtils.GetPropertyState(material, Shader.PropertyToID(property.name), out _, out var flag, out _);
                    if (flag)
                    {
                        EditorGUIUtility.PingObject(material);
                        Selection.SetActiveObjectWithContext(material, null);
                        return;
                    }
                }

                foreach (var type in types)
                {
                    IMaterialOtherPropertyElement.GetPropertyState(material, (int)type, out _, out var flag, out _);
                    if (flag)
                    {
                        EditorGUIUtility.PingObject(material);
                        Selection.SetActiveObjectWithContext(material, null);
                        return;
                    }
                }
            }
        }
    }
}
