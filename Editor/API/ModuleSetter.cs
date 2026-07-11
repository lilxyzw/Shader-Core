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
            var shaderModules = ProjectSettings.GetShaderModules(path);
            var modules = AssetUtils.GetFiles("*.scmodule").Select(path => SCModule.FromFile(path)).Where(m => m != null).ToArray();

            var so = new SerializedObject(ScriptableObject.CreateInstance<TempModulesObject>());
            using var toggles = so.FindProperty("toggles");
            toggles.arraySize = modules.Length;

            var root = new VisualElement();
            root.Bind(so);

            var applyButton = new Button(){text = "Apply"};
            applyButton.clickable = new(() =>
            {
                var toggles = root.Query<Toggle>().Build().ToArray();
                for (int i = 0; i < modules.Length; i++)
                {
                    var module = modules[i];
                    var t = root.Q<Toggle>(module.uniqueID);
                    bool enable = t != null && t.value;
                    if (enable)
                    {
                        if (!shaderModules.Contains(module.uniqueID)) shaderModules.Add(module.uniqueID);
                    }
                    else
                    {
                        if (shaderModules.Contains(module.uniqueID)) shaderModules.Remove(module.uniqueID);
                    }
                }
                applyButton.SetEnabled(false);
                ProjectSettings.instance.Save();
                AssetDatabase.ImportAsset(path);
            });
            applyButton.SetEnabled(false);

            for (int i = 0; i < modules.Length; i++)
            {
                var module = modules[i];
                using var t = toggles.GetArrayElementAtIndex(i);
                t.boolValue = shaderModules.Contains(module.uniqueID);
                var toggle = new Toggle{name = module.uniqueID, text = $"{module.name} ({module.uniqueID})", bindingPath = $"toggles.Array.data[{i}]"};
                root.Add(toggle);
                bool initialized = false;
                toggle.RegisterValueChangedCallback(e => {
                    if (initialized) applyButton.SetEnabled(true);
                    initialized = true;
                });
            }

            root.Add(applyButton);
            return root;
        }

        private class TempModulesObject : ScriptableObject
        {
            public bool[] toggles;
        }
    }
}
