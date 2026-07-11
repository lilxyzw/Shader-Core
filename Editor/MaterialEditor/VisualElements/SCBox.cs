using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCBox : Box
    {
        private static readonly Color backcol = EditorGUIUtility.isProSkin ? new(1,1,1,0.04f) : new(0,0,0,0.04f);
        private static readonly Color bordercol = EditorGUIUtility.isProSkin ? new(0,0,0,0.25f) : new(0,0,0,0.25f);
        private const int radius = 4;

        public SCBox()
        {
            style.backgroundColor = backcol;
            style.borderBottomColor = bordercol;
            style.borderLeftColor = bordercol;
            style.borderRightColor = bordercol;
            style.borderTopColor =  bordercol;
            style.flexDirection = FlexDirection.Column;
            style.marginBottom = 0;
            style.marginLeft = 0;
            style.marginRight = 0;
            style.marginTop = 0;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.borderTopWidth = 1;
            style.borderBottomLeftRadius = radius;
            style.borderBottomRightRadius = radius;
            style.borderTopLeftRadius = radius;
            style.borderTopRightRadius = radius;
            style.paddingBottom = 1;
            style.paddingLeft = 1;
            style.paddingRight = 1 + 2;
            style.paddingTop = 1;
        }
    }
}
