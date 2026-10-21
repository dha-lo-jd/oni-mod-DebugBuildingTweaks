using KSerialization;
using UnityEngine;

namespace DebugBuildingTweaks;

public sealed class DevPumpSettings : KMonoBehaviour
{
    public const float LiquidMaxGramsPerSecond = 10000f;
    public const float GasMaxGramsPerSecond = 1000f;
    public const float MinTemperature = 1f;
    public const float MaxTemperature = 5000f;
    public const float FallbackTemperature = 293.15f;

    [MyCmpReq] private Storage storage;
    [MyCmpReq] private DevPump pump;

    public float MaxGramsPerSecond = LiquidMaxGramsPerSecond;

    [Serialize] private float gramsPerSecond = LiquidMaxGramsPerSecond;
    [Serialize] private bool useCustomTemperature;
    [Serialize] private float customTemperature = FallbackTemperature;

    public float GramsPerSecond
    {
        get => gramsPerSecond;
        set
        {
            gramsPerSecond = Mathf.Clamp(value, 0f, MaxGramsPerSecond);
            UpdateCapacity();
        }
    }

    public float Kilograms => gramsPerSecond / 1000f;

    public float Temperature
    {
        get => useCustomTemperature ? customTemperature : DefaultTemperature;
        set
        {
            customTemperature = Mathf.Clamp(value, MinTemperature, MaxTemperature);
            useCustomTemperature = true;
            // Stored mass keeps its old temperature, so discard it to apply the new one immediately.
            storage.ConsumeAllIgnoringDisease();
        }
    }

    public float DefaultTemperature
    {
        get
        {
            Tag tag = pump.SelectedTag;
            Element element = tag.IsValid ? ElementLoader.GetElement(tag) : null;
            return element != null ? element.defaultValues.temperature : FallbackTemperature;
        }
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        gramsPerSecond = Mathf.Clamp(gramsPerSecond, 0f, MaxGramsPerSecond);
        UpdateCapacity();
    }

    private void UpdateCapacity()
    {
        if (storage != null)
        {
            storage.capacityKg = Mathf.Max(20f, Kilograms * 2f);
        }
    }
}
