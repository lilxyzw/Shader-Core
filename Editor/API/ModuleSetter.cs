using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    public static class ModuleSetter
    {
        public static VisualElement ModuleEditorField(string path)
        {
            ProjectSettings.GetShaderModules(path, out var modulenames, out var multiModules);
            var modules = AssetUtils.GetFiles("*.scmodule").Select(path => SCModule.FromFile(path)).Where(m => m != null).ToArray();

            var so = new SerializedObject(ScriptableObject.CreateInstance<TempModulesObject>());
            using var toggles = so.FindProperty("toggles");
            toggles.arraySize = modules.Count(m => m.properties_multi == null || m.properties_multi.Count == 0);
            using var _multiModules = so.FindProperty("multiModules");
            _multiModules.arraySize = modules.Count(m => m.properties_multi == null || m.properties_multi.Count == 0);

            var root = new VisualElement();
            root.Bind(so);

            var applyButton = new Button(){text = "Apply"};
            applyButton.clickable = new(() =>
            {
                modulenames.Clear();
                multiModules.Clear();
                foreach (var module in modules)
                {
                    if (module.properties_multi == null || module.properties_multi.Count == 0)
                    {
                        var t = root.Q<Toggle>(module.uniqueID);
                        bool enable = t != null && t.value;
                        if (enable) modulenames.Add(module.uniqueID);
                    }
                    else
                    {
                        var i = root.Q<IntegerField>(module.uniqueID);
                        var value = i.value;
                        if (value > 0) multiModules.Add(new(){name = module.uniqueID, count = value});
                    }
                }
                applyButton.SetEnabled(false);
                ProjectSettings.instance.Save();
                AssetDatabase.ImportAsset(path);
            });
            applyButton.SetEnabled(false);

            int index_t = 0;
            int index_m = 0;
            foreach (var module in modules)
            {
                if (module.properties_multi == null || module.properties_multi.Count == 0)
                {
                    using var t = toggles.GetArrayElementAtIndex(index_t);
                    t.boolValue = modulenames.Contains(module.uniqueID);
                    var toggle = new Toggle{name = module.uniqueID, text = $"{module.name} ({module.uniqueID})", bindingPath = $"toggles.Array.data[{index_t}]"};
                    root.Add(toggle);
                    bool initialized = false;
                    toggle.RegisterValueChangedCallback(e => {
                        if (initialized) applyButton.SetEnabled(true);
                        initialized = true;
                    });
                    index_t++;
                }
                else
                {
                    using var m = _multiModules.GetArrayElementAtIndex(index_m);
                    var first = multiModules.FirstOrDefault(m => m.name == module.uniqueID);
                    if (first == null) m.intValue = 0;
                    else m.intValue = first.count;

                    var count = new IntegerField{name = module.uniqueID, label = $"{module.name} ({module.uniqueID})", bindingPath = $"multiModules.Array.data[{index_m}]"};
                    root.Add(count);
                    bool initialized = false;
                    count.RegisterValueChangedCallback(e => {
                        if (initialized) applyButton.SetEnabled(true);
                        initialized = true;
                    });
                    index_m++;
                }
            }

            root.Add(applyButton);
            return root;
        }

        private class TempModulesObject : ScriptableObject
        {
            public bool[] toggles;
            public int[] multiModules;
        }
    }
}
