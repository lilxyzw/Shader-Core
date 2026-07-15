using System;
using System.Collections.Generic;
using System.Linq;
using jp.lilxyzw.shadercore;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using L10n = jp.lilxyzw.shadercore.L10n;
using Object = UnityEngine.Object;

#if !UNITY_6000_1_OR_NEWER
using MaterialProperty = jp.lilxyzw.shadercore.MaterialProperty;
#endif

public partial class SCMaterialEditor : MaterialEditor
{
    private VisualElement root;
    [NonSerialized] public VisualElement tempParent;
    public Shader shader;
    public string shaderPath;

    [SerializeField] private List<string> openedTextures = new();
    private readonly List<MaterialProperty> propertyCache = new();

    public override VisualElement CreateInspectorGUI()
    {
        UpdateInspectorGUI();
        return root;
    }

    public void UpdateInspectorGUI()
    {
        shader = (target as Material).shader;
        shaderPath = AssetDatabase.GetAssetPath(shader);
        if (root == null)
        {
            root = new();
            root.Bind(serializedObject);
            root.FixFont();
            root.style.marginLeft = 8;
            root.style.paddingLeft = 0;
            TargetHolder.targets.Add(root);
        }
        root.Clear();

        // Language
        bool isInitialized = false;
        var codes = L10n.GetLanguages().ToList();
        var names = L10n.GetLanguageNames().ToList();
        string CodeToName(string code)
        {
            var i = codes.IndexOf(code);
            if (i < 0 || i > (names.Count-1)) return code;
            return names[i];
        }
        var langField = new PopupField<string>("Language", codes, -1, CodeToName, CodeToName){bindingPath = "language"};
        langField.Bind(new(Settings.instance));
        langField.RegisterValueChangedCallback(e => {
            if (!isInitialized)
            {
                isInitialized = true;
                return;
            }
            L10n.Clear();
            SCLocalizeEvent.Invoke();
        });
        root.Add(langField);

        // モジュール編集
        var moduleButton = new Button(() => {
            var window = CreateInstance<EditorWindow>();
            var root = window.rootVisualElement;
            root.style.paddingBottom = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingLeft = 8;
            root.Add(new Label(shader.name));
            var box = new SCBox();
            box.style.flexGrow = 1;
            root.Add(box);
            box.Add(ModuleSetter.ModuleEditorField(shaderPath));
            window.ShowAuxWindow();
        }){text = L10n.L("__SelectModules")};
        moduleButton.RegisterCallback<SCLocalizeEvent>(e => moduleButton.text = L10n.L("__SelectModules"));
        root.Add(moduleButton);

        // Search
        var searchContainer = new VisualElement();
        root.Add(searchContainer);
        searchContainer.style.flexDirection = FlexDirection.Row;
        searchContainer.style.alignItems = Align.Center;
        searchContainer.style.marginBottom = 8;

        var search = new ToolbarSearchField();
        searchContainer.Add(search);
        search.style.flexGrow = 1;
        search.style.width = 0;

        var caseToggle = new ToolbarToggle();
        caseToggle.style.width = 24;
        caseToggle.style.height = 16;
        caseToggle.Add(new Image{image = EditorGUIUtility.IconContent("Font Icon").image});
        searchContainer.Add(caseToggle);

        void UpdateVisiblity()
        {
            var comparison = caseToggle.value ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            foreach (var e in root.Query<SCPropertyContainer>().Build()) e.SetVisible(search.value, comparison);

            void SetVisible(VisualElement element)
            {
                var containers = element.Query<SCPropertyContainer>().Build();
                element.style.display = containers.Any() && containers.All(e => e.style.display == DisplayStyle.None) ? DisplayStyle.None : DisplayStyle.Flex;
                foreach(var child in element.Children()) SetVisible(child);
            }
            foreach(var child in root.Children()) SetVisible(child);
        }

        search.RegisterValueChangedCallback(_ => UpdateVisiblity());
        caseToggle.RegisterValueChangedCallback(_ => UpdateVisiblity());

        #if !UNITY_6000_1_OR_NEWER
        static MaterialProperty[] GetMaterialProperties(Object[] targets)
        {
            return MaterialEditor.GetMaterialProperties(targets).Select(p => (MaterialProperty)p).ToArray();
        }

        static MaterialProperty GetMaterialProperty(Object[] targets, string name)
        {
            return MaterialEditor.GetMaterialProperty(targets, name);
        }
        #endif

        // Build Properties
        MaterialProperty[] properties = null;
        MaterialProperty[] targetProperties = null;
        if (targets.All(t => t is Material m && m && m.shader == shader))
        {
            properties = GetMaterialProperties(targets);
        }
        else
        {
            MaterialProperty[] duplication = null;
            foreach(var target in targets)
            {
                if (target is not Material m || !m || !m.shader) continue;
                var props = GetMaterialProperties(new Object[]{target});
                if (duplication == null)
                {
                    properties = props;
                    duplication = props;
                    shader = m.shader;
                    continue;
                }
                duplication = duplication.Where(d => props.Any(p => p.name == d.name && p.propertyType == d.propertyType)).ToArray();
            }
            targetProperties = duplication.Select(d => GetMaterialProperty(targets, d.name)).ToArray();
        }

        // Build Properties
        tempParent = new();
        var propertiesParent = tempParent;
        root.Add(propertiesParent);
        var i = -1;
        foreach (var prop in properties)
        {
            i++;
            var attributes = shader.GetPropertyAttributes(i);

            // Decorator
            foreach (var attr in attributes)
            {
                AttributeActions.TryInvokeDecorator(attr, this, prop);
            }

            if (attributes.Any(a => a == "SCHide" || a == "SCInHeader") || prop.propertyFlags.HasFlag(ShaderPropertyFlags.HideInInspector))
            {
                continue;
            }

            var prop2 = targetProperties?.FirstOrDefault(t => t.name == prop.name && t.propertyType == prop.propertyType);
            if (targetProperties == null) prop2 = prop;

            if (prop2 == null)
            {
                if (!attributes.Any(a => a == "SCCache")) propertyCache.Clear();
                continue;
            }

            if (attributes.Any(a => a == "SCCache"))
            {
                propertyCache.Add(prop2);
            }
            else
            {
                var container = GetPropertyContainer(prop2, propertyCache);
                ShaderProperty(container, prop2, attributes);
                foreach (var p in propertyCache) ShaderProperty(container, p);
                propertyCache.Clear();
            }
        }
        propertyCache.Clear();

        // Remove Unused UI
        if (targetProperties != null)
        {
            var children = propertiesParent.Children().ToArray();
            foreach(var child in children)
            {
                if (child.Q<SCPropertyContainer>() == null) propertiesParent.Remove(child);
            }
        }

        GetPropertyContainer(MaterialSerializedProperty.CustomRenderQueue).Add(new SCRenderQueueField(targets));
        GetPropertyContainer(MaterialSerializedProperty.EnableInstancingVariants).Add(new SCEnableInstancingField(targets));
        GetPropertyContainer(MaterialSerializedProperty.DoubleSidedGI).Add(new SCDoubleSidedGIField(targets));

        L10n.Load(shaderPath);
    }

    public override void OnDisable()
    {
        TargetHolder.targets.Remove(root);
        base.OnDisable();
    }

    public void ShaderProperty(VisualElement container, MaterialProperty prop, string[] attributes = null)
    {
        attributes ??= shader.GetPropertyAttributes(shader.FindPropertyIndex(prop.name));
        foreach (var attr in attributes)
            if (AttributeActions.TryInvokeDrawer(attr, this, prop, container)) return;

        if (attributes.Any(a => !AttributeActions.ContainsKey(a)))
        {
            Debug.LogWarning($"{prop.name} has an unknown attribute, so it fallback to IMGUI.");
            var imgui = new IMGUIContainer(() => {ShaderProperty(prop, prop.displayName);});
            imgui.style.marginLeft = 4;
            container.Add(imgui);
            return;
        }

        switch (prop.propertyType)
        {
            case ShaderPropertyType.Color: container.Add(new SCColorField(prop)); break;
            case ShaderPropertyType.Float: container.Add(new SCFloatField(prop)); break;
            case ShaderPropertyType.Int: container.Add(new SCIntField(prop)); break;
            case ShaderPropertyType.Range: container.Add(new SCSlider(prop, prop.rangeLimits.x, prop.rangeLimits.y)); break;
            case ShaderPropertyType.Vector: container.Add(new SCVector4Field(prop)); break;
            case ShaderPropertyType.Texture:
                var textureField = new SCTextureField(prop);
                container.Add(textureField);
                if (!prop.propertyFlags.HasFlag(ShaderPropertyFlags.NoScaleOffset))
                {
                    textureField.labelElement.style.paddingLeft = 16;

                    var foldout = new Foldout();
                    var toggle = foldout.Q<Toggle>();
                    toggle.style.width = 16;
                    toggle.style.marginLeft = 4;
                    toggle.style.height = 20;
                    toggle.style.marginTop = -20;
                    toggle.style.marginBottom = 0;
                    toggle.style.paddingTop = 0;
                    toggle.style.paddingBottom = 4;
                    toggle.style.alignItems = Align.Center;

                    tempParent.Add(toggle);
                    var foldoutContainer = foldout.contentContainer;
                    foldoutContainer.style.marginLeft = 0;
                    tempParent.Add(foldoutContainer);
                    var container2 = GetPropertyContainer(prop);
                    container2.hierarchy.RemoveAt(0);
                    container2.Add(new SCScaleOffset(prop));
                    foldoutContainer.Add(container2);

                    foldout.value = openedTextures.Contains(prop.name);
                    foldout.RegisterValueChangedCallback(e => {
                        if (e.newValue) openedTextures.Add(prop.name);
                        else openedTextures.Remove(prop.name);
                    });
                }
                break;
        }
    }

    private VisualElement GetPropertyContainer(params object[] objs)
    {
        var container = new SCPropertyContainer(tempParent, targets, objs);
        tempParent.Add(container);
        return container;
    }
}
