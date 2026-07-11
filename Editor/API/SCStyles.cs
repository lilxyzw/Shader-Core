using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    public class SCStyles
    {
        private static readonly Color popup_backcol = EditorGUIUtility.isProSkin ? new(0,0,0,0.4f) : new(1,1,1,0.4f);

        public static void ApplyPopupStyle<T>(PopupField<T> element)
        {
            element.Q(null, "unity-base-popup-field__text").parent.style.backgroundColor = popup_backcol;
        }

        private static readonly Color vector_textcol = EditorGUIUtility.isProSkin ? new(1,1,1,0.3f) : new(0,0,0,0.4f);
        private static readonly Color vector_backcol = EditorGUIUtility.isProSkin ? new(0,0,0,0.2f) : new(1,1,1,0.2f);
        private static readonly Color vector_bordercol = EditorGUIUtility.isProSkin ? new(0,0,0,0.5f) : new(0,0,0,0.15f);

        public static void ApplyVectorStyle<T1, T2, T3>(BaseCompositeField<T1, T2, T3> element) where T2 : TextValueField<T3>, new()
        {
            var floats = element.Query<FloatField>().Build();
            bool firstElement = true;
            foreach (var f in floats)
            {
                f.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                f.labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;
                f.labelElement.style.fontSize = 10;
                f.labelElement.style.color = vector_textcol;
                f.labelElement.style.backgroundColor = vector_backcol;
                f.labelElement.style.borderBottomColor = vector_bordercol;
                f.labelElement.style.borderLeftColor = vector_bordercol;
                f.labelElement.style.borderTopColor =  vector_bordercol;
                f.labelElement.style.minWidth = 18;

                f.labelElement.style.marginBottom = 0;
                f.labelElement.style.marginLeft = firstElement ? 0 : 2;
                f.labelElement.style.marginRight = -2;
                f.labelElement.style.marginTop = 0;
                f.labelElement.style.borderBottomWidth = 1;
                f.labelElement.style.borderLeftWidth = 1;
                f.labelElement.style.borderRightWidth = 0;
                f.labelElement.style.borderTopWidth = 1;
                f.labelElement.style.borderBottomLeftRadius = 4;
                f.labelElement.style.borderBottomRightRadius = 0;
                f.labelElement.style.borderTopLeftRadius = 4;
                f.labelElement.style.borderTopRightRadius = 0;
                f.labelElement.style.paddingBottom = 0;
                f.labelElement.style.paddingLeft = 0;
                f.labelElement.style.paddingRight = 2;
                f.labelElement.style.paddingTop = 0;

                firstElement = false;
            }
        }
    }
}
