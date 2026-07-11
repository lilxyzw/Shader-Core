using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    public static class AttributeActions
    {
        private static readonly Dictionary<string, Action<SCMaterialEditor, MaterialProperty, string, VisualElement>> drawers = new();
        private static readonly Dictionary<string, Action<SCMaterialEditor, MaterialProperty, string>> decorators = new();
        private static readonly Dictionary<string, Action<MaterialProperty, string>> valueChangeActions = new();

        public static void AddDrawer(string key, Action<SCMaterialEditor, MaterialProperty, string, VisualElement> action) => drawers[key] = action;
        public static void AddDecorator(string key, Action<SCMaterialEditor, MaterialProperty, string> action) => decorators[key] = action;
        public static void AddValueChangeAction(string key, Action<MaterialProperty, string> action) => valueChangeActions[key] = action;

        public static bool TryInvokeDrawer(string attribute, SCMaterialEditor editor, MaterialProperty prop, VisualElement container)
        {
            var (key, value) = GetKeyAndValue(attribute);
            if (!drawers.TryGetValue(key, out var action)) return false;
            action.Invoke(editor, prop, value, container);
            return true;
        }

        public static bool TryInvokeDecorator(string attribute, SCMaterialEditor editor, MaterialProperty prop)
        {
            var (key, value) = GetKeyAndValue(attribute);
            if (!decorators.TryGetValue(key, out var action)) return false;
            action.Invoke(editor, prop, value);
            return true;
        }

        public static bool TryInvokeValueChangeAction(string attribute, MaterialProperty prop)
        {
            var (key, value) = GetKeyAndValue(attribute);
            if (!valueChangeActions.TryGetValue(key, out var action)) return false;
            action.Invoke(prop, value);
            return true;
        }

        public static bool ContainsKey(string attribute)
        {
            var (key, _) = GetKeyAndValue(attribute);
            return decorators.ContainsKey(key) || drawers.ContainsKey(key);
        }

        private static (string,string) GetKeyAndValue(string attribute)
        {
            var match = Regex.Match(attribute, @"(\w+)\s*\((.*)\)");
            if (match.Success) return (match.Groups[1].Value, match.Groups[2].Value);
            else return (attribute, null);
        }
    }
}
