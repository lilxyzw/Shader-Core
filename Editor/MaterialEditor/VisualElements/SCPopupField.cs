using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCPopupField : PopupField<int>, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get; set; }
        private readonly List<string> names = new();
        private List<string> localizedNames;

        public SCPopupField(MaterialProperty property, string value) : base()
        {
            var args = value.Split(',');
            var values = new List<int>();
            try
            {
                if (args.Length == 1)
                {
                    var type = TypeCache.GetTypesDerivedFrom(typeof(Enum)).FirstOrDefault(x => x.Name == value || x.FullName == value);
                    if (type == null)
                    {
                        Debug.LogError($"Enum {value} not found.");
                    }
                    names = Enum.GetNames(type).ToList();
                    values = ((int[])Enum.GetValues(type)).ToList();
                }
                else
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (i % 2 == 0) names.Add(args[i].Trim());
                        else values.Add(int.Parse(args[i].Trim()));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            localizedNames = names.Select(n => L10n.L(n)).ToList();

            string GetLabel(int v)
            {
                var index = values.IndexOf(v);
                if (index >= 0) return localizedNames[index];
                return "";
            }
            choices = values;
            formatListItemCallback = GetLabel;
            formatSelectedValueCallback = GetLabel;

            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
            SCStyles.ApplyPopupStyle(this);
            style.flexGrow = 0;

            RegisterCallback<SCLocalizeEvent>(e =>
            {
                L10n.Load(ModuleID);
                localizedNames = names.Select(n => L10n.L(n)).ToList();
                textElement.text = formatSelectedValueCallback(rawValue);
            });
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            if (Property != null && new System.Diagnostics.StackFrame(3, false).GetMethod().ToString() == "Void ChangeValueFromMenu(Int32)")
            {
                if (Property.propertyType == ShaderPropertyType.Int) Property.intValue = newValue;
                else Property.floatValue = newValue;
            }
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue)
            {
                rawValue = Property.propertyType == ShaderPropertyType.Int ? Property.intValue : (int)Property.floatValue;
                if (formatSelectedValueCallback != null) textElement.text = formatSelectedValueCallback(rawValue);
            }
        }
    }
}
