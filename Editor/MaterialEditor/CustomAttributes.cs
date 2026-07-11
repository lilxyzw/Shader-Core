using System;
using System.Globalization;
using jp.lilxyzw.shadercore.CustomTextures;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal static class CustomAttributes
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            AttributeActions.AddDecorator("SCModule", SCModule);
            AttributeActions.AddDecorator("SCCache", (_,_,_) => {});
            AttributeActions.AddDecorator("SCFoldout", SCFoldout);
            AttributeActions.AddDecorator("SCFoldoutEnd", SCFoldoutEnd);
            AttributeActions.AddDecorator("SCBox", SCBox);
            AttributeActions.AddDecorator("SCBoxEnd", SCBoxEnd);
            AttributeActions.AddDecorator("SCInternalUpdateGUI", (_,_,_) => {});
            AttributeActions.AddDecorator("SCInHeader", (_,_,_) => {});

            AttributeActions.AddDrawer("SCToggle", SCToggle);
            AttributeActions.AddDrawer("SCEnum", SCEnum);
            AttributeActions.AddDrawer("SCRange", SCRange);
            AttributeActions.AddDrawer("SCRangeInt", SCRangeInt);
            AttributeActions.AddDrawer("SCMinMax", SCMinMax);
            AttributeActions.AddDrawer("SCVector", SCVector);
            AttributeActions.AddDrawer("SCVector2", (a,b,c,d) => SCVector(a,b,"2",d));
            AttributeActions.AddDrawer("SCVector3", (a,b,c,d) => SCVector(a,b,"3",d));
            AttributeActions.AddDrawer("SCVector4", (a,b,c,d) => SCVector(a,b,"4",d));
            AttributeActions.AddDrawer("SCHDR", SCHDR);
            AttributeActions.AddDrawer("SCMask", SCMask);
            AttributeActions.AddDrawer("SCMasks", SCMasks);
            AttributeActions.AddDrawer("SCGradients", SCGradients);
            AttributeActions.AddDrawer("SCGradientSelect", SCGradientSelect);
            AttributeActions.AddDrawer("SCMaskChannel", SCMaskChannel);

            AttributeActions.AddValueChangeAction("SCConstValue", SCConstValue);
        }

        private static void SCModule(SCMaterialEditor editor, MaterialProperty prop, string args)
        {
            L10n.Load(string.IsNullOrEmpty(args) ? editor.shaderPath : args);
        }

        private static void SCFoldout(SCMaterialEditor editor, MaterialProperty prop, string args)
        {
            var foldout = new SCFoldout(prop.name+args, args);
            if (prop.targets[0] is Material m && m.shader is Shader shader)
            {
                var attributes = shader.GetPropertyAttributes(shader.FindPropertyIndex(prop.name));
                foreach (var attr in attributes)
                {
                    if (attr != "SCInHeader") continue;
                    var container = new VisualElement();
                    container.style.flexGrow = 0;
                    container.style.marginBottom = 0;
                    container.style.marginLeft = -6;
                    container.style.marginRight = 4;
                    container.style.marginTop = 1;
                    editor.ShaderProperty(container, prop, attributes);
                    var label = foldout.Q(null, Toggle.textUssClassName);
                    var parent = label.parent;
                    label.style.flexGrow = 1;
                    parent.Add(container);
                    parent.Add(label);
                    break;
                }
            }
            editor.tempParent.Add(foldout);
            editor.tempParent = foldout;
        }

        private static void SCFoldoutEnd(SCMaterialEditor editor, MaterialProperty prop, string args)
        {
            editor.tempParent = editor.tempParent.parent;
        }

        private static void SCBox(SCMaterialEditor editor, MaterialProperty prop, string args)
        {
            var box = new SCBox();
            editor.tempParent.Add(box);
            editor.tempParent = box;
        }

        private static void SCBoxEnd(SCMaterialEditor editor, MaterialProperty prop, string args)
        {
            editor.tempParent = editor.tempParent.parent;
        }

        private static void SCToggle(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            container.Add(new SCToggle(prop));
        }

        private static void SCEnum(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            container.Add(new SCPopupField(prop, args));
        }

        private static void SCRange(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            var argsSeparated = args.Split(',');
            switch (prop.propertyType)
            {
                case ShaderPropertyType.Float: container.Add(new SCSlider(prop, float.Parse(argsSeparated[0].Replace('_','-')), float.Parse(argsSeparated[1].Replace('_','-')))); return;
                case ShaderPropertyType.Int: container.Add(new SCSliderInt(prop, int.Parse(argsSeparated[0].Replace('_','-')), int.Parse(argsSeparated[1].Replace('_','-')))); return;
                default: return;
            }
        }

        private static void SCRangeInt(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            var argsSeparated = args.Split(',');
            container.Add(new SCSliderInt(prop, int.Parse(argsSeparated[0].Replace('_','-')), int.Parse(argsSeparated[1].Replace('_','-'))));
        }

        private static void SCMinMax(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            var argsSeparated = args.Split(',');
            container.Add(new SCMinMax(prop, float.Parse(argsSeparated[0].Replace('_','-')), float.Parse(argsSeparated[1].Replace('_','-'))));
        }

        private static void SCVector(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            if (args == "2") container.Add(new SCVector2Field(prop));
            if (args == "3") container.Add(new SCVector3Field(prop));
            else container.Add(new SCVector4Field(prop));
        }

        private static void SCHDR(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            container.Add(new SCColorField(prop){hdr = true});
        }

        private static void SCMask(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            editor.ShaderProperty(container, prop, new string[]{});
            var field = container.Q<SCTextureField>();
            if (field != null) container.Add(new SCEditableTextureButton<Texture2D, MaskImporter>(field, "scmask"));
        }

        private static void SCMasks(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            editor.ShaderProperty(container, prop, new string[]{});
            var field = container.Q<SCTextureField>();
            if (field != null) container.Add(new SCEditableTextureButton<Texture2DArray, MasksImporter>(field, "scmasks"));
        }

        private static void SCGradients(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            editor.ShaderProperty(container, prop, new string[]{});
            var field = container.Q<SCTextureField>();
            if (field != null) container.Add(new SCEditableTextureButton<Texture2DArray, GradientsImporter>(field, "scgradients"));
        }

        private static void SCGradientSelect(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            container.Add(new SCGradientSelect(prop, string.IsNullOrEmpty(args) ? "_SharedGradients" : args));
        }

        private static void SCMaskChannel(SCMaterialEditor editor, MaterialProperty prop, string args, VisualElement container)
        {
            container.Add(new SCMaskChannel(prop, string.IsNullOrEmpty(args) ? "_SharedMask" : args));
        }

        private static void SCConstValue(MaterialProperty prop, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                throw new Exception($"Invalid max value for SCConstValue. {prop.name}");
            }

            var val = args.Contains(',') ? args.Split(',')[0] : args;
            if (!int.TryParse(val, out var max))
            {
                throw new Exception($"Invalid max value for SCConstValue. {prop.name}");
            }

            var key = prop.name.ToUpper(CultureInfo.InvariantCulture);
            foreach (Material mat in prop.targets)
            {
                for (int i = 0; i <= max; i++)
                {
                    if (prop.propertyType == ShaderPropertyType.Float && prop.floatValue == i ||
                        prop.propertyType == ShaderPropertyType.Range && prop.floatValue == i ||
                        prop.propertyType == ShaderPropertyType.Int && prop.intValue == i) mat.EnableKeyword($"{key}_{i}");
                    else mat.DisableKeyword($"{key}_{i}");
                }
            }
        }
    }
}
