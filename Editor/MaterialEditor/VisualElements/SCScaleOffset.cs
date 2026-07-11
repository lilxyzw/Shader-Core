using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.shadercore
{
    internal class SCScaleOffset : Vector4Field, IMaterialPropertyElement
    {
        public MaterialProperty Property { get; set; }
        public string ModuleID { get; set; }
        public string LocalizedLabel { get => " "; set {} }

        public SCScaleOffset(MaterialProperty property) : base()
        {
            ((IMaterialPropertyElement)this).InitializeVisualElement(this, UpdateUI, property);
            SCStyles.ApplyVectorStyle(this);

            var box = new SCBox();
            this.Q<FloatField>().parent.Add(box);
            box.style.flexGrow = 1;

            var scale = new VisualElement();
            scale.style.flexDirection = FlexDirection.Row;
            scale.style.marginBottom = 1;
            scale.style.marginTop = 1;
            var scaleLabel = new Label(L10n.L("__Tiling"));
            scale.Add(scaleLabel);
            scaleLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            scaleLabel.style.fontSize = 10;
            scaleLabel.style.width = 40;
            scaleLabel.style.color = EditorGUIUtility.isProSkin ? new Color(1,1,1,0.5f) : new Color(0,0,0,0.6f);
            scaleLabel.style.paddingLeft = 4;
            box.Add(scale);

            var offset = new VisualElement();
            offset.style.flexDirection = FlexDirection.Row;
            offset.style.marginBottom = 1;
            offset.style.marginTop = 1;
            var offsetLabel = new Label(L10n.L("__Offset"));
            offset.Add(offsetLabel);
            offsetLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            offsetLabel.style.fontSize = 10;
            offsetLabel.style.width = 40;
            offsetLabel.style.color = EditorGUIUtility.isProSkin ? new Color(1,1,1,0.5f) : new Color(0,0,0,0.6f);
            offsetLabel.style.paddingLeft = 4;
            box.Add(offset);

            box.RegisterCallback<SCLocalizeEvent>(e =>
            {
                L10n.Load();
                scaleLabel.text = L10n.L("__Tiling");
                offsetLabel.text = L10n.L("__Offset");
            });

            int index = 0;
            foreach (var f in this.Query<FloatField>().Build())
            {
                if (index == 0)
                {
                    scale.Add(f);
                }
                if (index == 1)
                {
                    scale.Add(f);
                }
                if (index == 2)
                {
                    offset.Add(f);
                    f.labelElement.text = "X";
                    f.labelElement.style.marginLeft = 0;
                }
                if (index == 3)
                {
                    offset.Add(f);
                    f.labelElement.text = "Y";
                }
                index++;
            }
        }

        public override void SetValueWithoutNotify(Vector4 newValue)
        {
            if (Property != null) Property.textureScaleAndOffset = newValue;
            base.SetValueWithoutNotify(newValue);
        }

        public void UpdateUI()
        {
            if (!Property.hasMixedValue)
            {
                rawValue = Property.textureScaleAndOffset;
                int index = 0;
                foreach (var f in this.Query<FloatField>().Build())
                    f.SetValueWithoutNotify(rawValue[index++]);
            }
        }
    }
}
