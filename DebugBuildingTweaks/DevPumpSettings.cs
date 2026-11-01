using KSerialization;
using UnityEngine;

namespace DebugBuildingTweaks;

/// <summary>
/// Per-pump settings (output rate and temperature) added to the vanilla dev pumps,
/// plus the replacement fill logic that honours them.
/// </summary>
public sealed class DevPumpSettings : KMonoBehaviour
{
    public const float LiquidMaxGramsPerSecond = 10000f;
    public const float GasMaxGramsPerSecond = 1000f;
    public const float MinTemperature = 1f;
    public const float MaxTemperature = 5000f;
    public const float FallbackTemperature = 293.15f;

    private const float MinCapacityKg = 20f;

    [MyCmpReq] private Storage storage;
    [MyCmpReq] private DevPump pump;

    [Serialize] private float gramsPerSecond = LiquidMaxGramsPerSecond;
    [Serialize] private bool useCustomTemperature;
    [Serialize] private float customTemperature = FallbackTemperature;

    public float MaxGramsPerSecond =>
        pump.elementState == Filterable.ElementState.Gas ? GasMaxGramsPerSecond : LiquidMaxGramsPerSecond;

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

    /// <summary>Kelvin. Falls back to the selected element's default temperature until one is set.</summary>
    public float Temperature => useCustomTemperature ? customTemperature : DefaultTemperature;

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

    public void SetTemperature(float kelvin)
    {
        customTemperature = Mathf.Clamp(kelvin, MinTemperature, MaxTemperature);
        useCustomTemperature = true;
        // Stored mass keeps its old temperature, so discard it to apply the new one immediately.
        storage.ConsumeAllIgnoringDisease();
    }

    /// <summary>
    /// Replacement for <see cref="DevPump.Sim1000ms"/>.
    /// Returns false when the vanilla logic must run instead (the selection is an item, not an element).
    /// </summary>
    public bool TryFillStorage()
    {
        Tag tag = pump.SelectedTag;
        if (!tag.IsValid)
        {
            return true;
        }

        Element element = ElementLoader.GetElement(tag);
        if (element == null)
        {
            return false;
        }

        float missing = Kilograms - storage.GetAmountAvailable(tag);
        if (missing > 0f)
        {
            storage.AddElement(element.id, missing, Temperature, byte.MaxValue, 0, false, false);
        }

        return true;
    }

    private void UpdateCapacity()
    {
        storage.capacityKg = Mathf.Max(MinCapacityKg, Kilograms * 2f);
    }
}
