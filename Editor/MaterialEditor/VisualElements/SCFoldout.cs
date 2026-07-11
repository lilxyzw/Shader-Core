using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCFoldout : Foldout
    {
        private static readonly Color backcolPale = EditorGUIUtility.isProSkin ? new Color(1,1,1,0.075f) : new Color(0,0,0,0.075f);
        private static readonly Color backcol = EditorGUIUtility.isProSkin ? new Color(1,1,1,0.15f) : new Color(0,0,0,0.15f);
        private static readonly Color bordercol = EditorGUIUtility.isProSkin ? new(0,0,0,0.5f) : new(0,0,0,0.15f);
        private readonly VisualElement m_header;
        private static readonly FieldInfo FI_m_Clickable = typeof(BaseBoolField).GetField("m_Clickable", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly string moduleID;

        public SCFoldout(string key, string label)
        {
            moduleID = L10n.currentID;
            text = L10n.L(label);

            RegisterCallback<SCLocalizeEvent>(e =>
            {
                L10n.Load(moduleID);
                if (!string.IsNullOrWhiteSpace(text)) text = L10n.L(label);
            });

            m_header = new();
            m_header.style.marginBottom = 0;
            m_header.style.marginLeft = 0;
            m_header.style.marginRight = 0;
            m_header.style.marginTop = 1;
            m_header.style.borderBottomWidth = 1;
            m_header.style.borderLeftWidth = 0;
            m_header.style.borderRightWidth = 0;
            m_header.style.borderTopWidth = 0;
            m_header.style.paddingBottom = 0;
            m_header.style.paddingLeft = 0;
            m_header.style.paddingRight = 0;
            m_header.style.paddingTop = 0;

            m_header.style.backgroundColor = backcol;
            m_header.style.borderBottomLeftRadius = 4;
            m_header.style.borderBottomRightRadius = 4;
            m_header.style.borderTopLeftRadius = 4;
            m_header.style.borderTopRightRadius = 4;
            m_header.style.borderBottomColor = bordercol;

            var toggle = this.Q<Toggle>();
            toggle.RemoveManipulator(FI_m_Clickable.GetValue(toggle) as Clickable);
            toggle.style.unityFontStyleAndWeight = FontStyle.Bold;
            toggle.focusable = false;
            toggle.style.marginBottom = 2;
            toggle.style.marginLeft = 2;
            toggle.style.marginRight = 2;
            toggle.style.marginTop = 2;
            toggle.style.borderBottomWidth = 0;
            toggle.style.borderLeftWidth = 0;
            toggle.style.borderRightWidth = 0;
            toggle.style.borderTopWidth = 0;
            toggle.style.paddingBottom = 0;
            toggle.style.paddingLeft = 0;
            toggle.style.paddingRight = 0;
            toggle.style.paddingTop = 0;
            m_header.Add(toggle);

            hierarchy.Add(m_header);
            hierarchy.Add(contentContainer);

            contentContainer.style.marginBottom = 0;
            contentContainer.style.marginLeft = 16;
            contentContainer.style.marginRight = 0;
            contentContainer.style.marginTop = 0;
            contentContainer.style.borderBottomWidth = 0;
            contentContainer.style.borderLeftWidth = 0;
            contentContainer.style.borderRightWidth = 0;
            contentContainer.style.borderTopWidth = 0;
            contentContainer.style.paddingBottom = 0;
            contentContainer.style.paddingLeft = 0;
            contentContainer.style.paddingRight = 0;
            contentContainer.style.paddingTop = 0;

            toggle.RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.button == 0) m_header.style.backgroundColor = backcolPale;
            });

            toggle.RegisterCallback<PointerLeaveEvent>(e =>
            {
                m_header.style.backgroundColor = backcol;
            });

            value = FoldoutSaver.IsOpened(key);

            toggle.RegisterCallback<PointerUpEvent>(e =>
            {
                m_header.style.backgroundColor = backcol;
                if (e.button == 0)
                {
                    value = !value;
                    if (value) FoldoutSaver.Open(key);
                    else FoldoutSaver.Close(key);
                }
                else if (e.button == 1)
                {
                    EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition, Vector2.one), new GUIContent[] { L10n.G("Copy"), L10n.G("Paste"), L10n.G("Reset") }, -1, (_, _, selected) =>
                    {
                        var props = contentContainer.Query<SCPropertyContainer>().Build().SelectMany(c => c.properties).ToArray();

                        void InvokeActions()
                        {
                            if (props[0].targets[0] is not Material m || m.shader is not Shader shader) return;
                            foreach (var prop in props)
                            {
                                var attributes = shader.GetPropertyAttributes(shader.FindPropertyIndex(prop.name));
                                foreach (var attr in attributes)
                                {
                                    AttributeActions.TryInvokeValueChangeAction(attr, prop);
                                }
                            }
                        }

                        // コピー処理
                        if (selected == 0)
                        {
                            PropertyClipboard.ToClipboard(props);
                            InvokeActions();
                        }

                        // ペースト処理
                        if (selected == 1)
                        {
                            PropertyClipboard.FromClipboard(props);
                            InvokeActions();
                        }

                        // リセット処理
                        if (selected == 2)
                        {
                            MaterialPropertyUtils.Reset(props);
                            InvokeActions();
                        }

                        foreach (var field in contentContainer.Query<VisualElement>().Build().Select(e => e as IMaterialPropertyElement).Where(e => e != null).ToArray())
                        {
                            field.SetupVisualElement((VisualElement)field);
                            field.UpdateUI();
                        }
                    }, null);
                }
            });
        }
    }
}
