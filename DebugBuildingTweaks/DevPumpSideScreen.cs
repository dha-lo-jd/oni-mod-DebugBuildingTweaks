using System.Globalization;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace DebugBuildingTweaks;

public sealed class DevPumpSideScreen : SideScreenContent
{
    private DevPumpSettings target;
    private GameObject content;
    private bool updating;

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        ContentContainer = gameObject;
    }

    public override bool IsValidForTarget(GameObject target)
    {
        return target != null && target.GetComponent<DevPumpSettings>() != null;
    }

    public override void SetTarget(GameObject targetObject)
    {
        target = targetObject != null ? targetObject.GetComponent<DevPumpSettings>() : null;
        Rebuild();
    }

    public override void ClearTarget()
    {
        target = null;
        Rebuild();
    }

    // DetailsScreen lists side screens in descending order of this value. The vanilla element filter returns 0 and the
    // debug tool panel (AutoPlumberSideScreen, shown in instant-build mode) returns -150, so -100 sits between them.
    public override int GetSideScreenSortOrder() => -100;

    public override string GetTitle()
    {
        return STRINGS.DEV_PUMP.TITLE;
    }

    private void Rebuild()
    {
        if (content != null)
        {
            Destroy(content);
            content = null;
        }

        if (target == null)
        {
            return;
        }

        PPanel root = new PPanel("DevPumpSettings")
        {
            Direction = PanelDirection.Vertical,
            Alignment = TextAnchor.UpperLeft,
            Spacing = 0,
            Margin = new RectOffset(0, 0, 0, 0),
            FlexSize = Vector2.right
        };

        float tempMin = GameUtil.GetConvertedTemperature(DevPumpSettings.MinTemperature);
        float tempMax = GameUtil.GetConvertedTemperature(DevPumpSettings.MaxTemperature);

        AddSetting(root, "Rate", STRINGS.DEV_PUMP.RATE, "g/s", 0f, target.MaxGramsPerSecond,
            () => target.GramsPerSecond, v => target.GramsPerSecond = v);
        AddSetting(root, "Temp", STRINGS.DEV_PUMP.TEMPERATURE, GameUtil.GetTemperatureUnitSuffix().Trim(),
            tempMin, tempMax,
            () => GameUtil.GetConvertedTemperature(target.Temperature),
            v => target.SetTemperature(GameUtil.GetTemperatureConvertedToKelvin(v)));

        root.AddOnRealize(go => content = go);
        root.AddTo(ContentContainer != null ? ContentContainer : gameObject);
    }

    private void AddSetting(PPanel root, string id, string label, string unit, float min, float max,
        System.Func<float> getter, System.Action<float> setter)
    {
        GameObject slider = null;
        GameObject field = null;

        root.AddChild(new PLabel(id + "Label")
        {
            Text = label,
            TextStyle = PUITuning.Fonts.TextLightStyle,
            TextAlignment = TextAnchor.MiddleLeft,
            Margin = new RectOffset(8, 8, 3, 3),
            BackColor = new Color32(168, 80, 122, 255),
            FlexSize = Vector2.right
        });

        PPanel inputRow = new PPanel(id + "InputRow")
        {
            Direction = PanelDirection.Horizontal,
            Alignment = TextAnchor.MiddleCenter,
            Spacing = 6,
            Margin = new RectOffset(0, 0, 6, 2),
            FlexSize = Vector2.right
        };

        PPanel row = new PPanel(id + "Row")
        {
            Direction = PanelDirection.Horizontal,
            Alignment = TextAnchor.MiddleCenter,
            Margin = new RectOffset(12, 12, 2, 2),
            FlexSize = Vector2.right
        };

        PSliderSingle sliderDef = new PSliderSingle(id + "Slider")
        {
            MinValue = min,
            MaxValue = max,
            InitialValue = Mathf.Clamp(getter(), min, max),
            PreferredLength = 230f,
            TrackSize = 8f,
            HandleSize = 16f,
            Direction = UnityEngine.UI.Slider.Direction.LeftToRight,
            IntegersOnly = true,
            FlexSize = Vector2.right,
            OnValueChanged = (_, value) =>
            {
                if (updating || target == null)
                {
                    return;
                }

                updating = true;
                setter(value);
                SetFieldText(field, getter());
                updating = false;
            }
        };
        sliderDef.SetKleiPinkStyle();
        sliderDef.AddOnRealize(go => slider = go);
        row.AddChild(sliderDef);

        PTextField fieldDef = new PTextField(id + "Field")
        {
            Text = Format(getter()),
            Type = PTextField.FieldType.Float,
            TextStyle = PUITuning.Fonts.TextDarkStyle,
            TextAlignment = TextAlignmentOptions.Midline,
            MinWidth = 100,
            OnTextChanged = (_, text) =>
            {
                if (updating || target == null || !TryParse(text, out float value))
                {
                    return;
                }

                updating = true;
                setter(value);
                PSliderSingle.SetCurrentValue(slider, Mathf.Clamp(getter(), min, max));
                updating = false;
            }
        };
        fieldDef.AddOnRealize(go => field = go);
        inputRow.AddChild(fieldDef);

        inputRow.AddChild(new PLabel(id + "Unit")
        {
            Text = unit,
            TextStyle = PUITuning.Fonts.TextDarkStyle,
            TextAlignment = TextAnchor.MiddleLeft
        });

        root.AddChild(inputRow);
        root.AddChild(row);

        PPanel range = new PPanel(id + "Range")
        {
            Direction = PanelDirection.Horizontal,
            Alignment = TextAnchor.MiddleLeft,
            Margin = new RectOffset(12, 12, 0, 6),
            FlexSize = Vector2.right
        };
        range.AddChild(new PLabel(id + "Min")
        {
            Text = Format(min) + " " + unit,
            TextStyle = PUITuning.Fonts.TextDarkStyle,
            TextAlignment = TextAnchor.MiddleLeft,
            FlexSize = Vector2.right
        });
        range.AddChild(new PLabel(id + "Max")
        {
            Text = Format(max) + " " + unit,
            TextStyle = PUITuning.Fonts.TextDarkStyle,
            TextAlignment = TextAnchor.MiddleRight
        });
        root.AddChild(range);
    }

    private static void SetFieldText(GameObject field, float value)
    {
        TMP_InputField input = field != null ? field.GetComponentInChildren<TMP_InputField>() : null;
        if (input != null)
        {
            input.text = Format(value);
        }
    }

    private static string Format(float value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static bool TryParse(string text, out float value)
    {
        return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
               float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }
}
