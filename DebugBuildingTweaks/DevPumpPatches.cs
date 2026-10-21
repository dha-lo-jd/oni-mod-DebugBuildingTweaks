using HarmonyLib;
using PeterHan.PLib.UI;
using UnityEngine;

namespace DebugBuildingTweaks;

[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
public static class DetailsScreen_OnPrefabInit_Patch
{
    public static void Postfix()
    {
        PUIUtils.AddSideScreenContent<DevPumpSideScreen>();
    }
}

[HarmonyPatch(typeof(DevPumpGasConfig), nameof(DevPumpGasConfig.DoPostConfigureComplete))]
public static class DevPumpGasConfig_Patch
{
    public static void Postfix(GameObject go)
    {
        DevPumpSettings settings = go.AddOrGet<DevPumpSettings>();
        settings.MaxGramsPerSecond = DevPumpSettings.GasMaxGramsPerSecond;
    }
}

[HarmonyPatch(typeof(DevPumpLiquidConfig), nameof(DevPumpLiquidConfig.DoPostConfigureComplete))]
public static class DevPumpLiquidConfig_Patch
{
    public static void Postfix(GameObject go) => go.AddOrGet<DevPumpSettings>();
}

[HarmonyPatch(typeof(DevPump), nameof(DevPump.Sim1000ms))]
public static class DevPump_Sim1000ms_Patch
{
    public static bool Prefix(DevPump __instance)
    {
        DevPumpSettings settings = __instance.GetComponent<DevPumpSettings>();
        if (settings == null)
        {
            return true;
        }

        Tag tag = __instance.SelectedTag;
        if (!tag.IsValid)
        {
            return false;
        }

        Element element = ElementLoader.GetElement(tag);
        if (element == null)
        {
            return true;
        }

        Storage storage = __instance.GetComponent<Storage>();
        float amount = settings.Kilograms - storage.GetAmountAvailable(tag);
        if (amount > 0f)
        {
            storage.AddElement(element.id, amount, settings.Temperature, byte.MaxValue, 0, false, false);
        }

        return false;
    }
}

// Makes the dev pump element filter match the internal element ID (e.g. "OxygenGas") as well as the display name.
[HarmonyPatch(typeof(SingleItemSelectionSideScreenBase), "OnSearchBarValueChanged")]
public static class SearchBarValueChanged_Patch
{
    public static bool DevPumpSearch;

    public static void Prefix(SingleItemSelectionSideScreenBase __instance)
    {
        DevPumpSearch = __instance is FilterSideScreen &&
                        Traverse.Create(__instance).Field("targetFilterable").GetValue<Filterable>() is DevPump;
    }

    public static void Postfix() => DevPumpSearch = false;
}

[HarmonyPatch(typeof(SingleItemSelectionSideScreenBase), "TagContainsSearchWord")]
public static class TagContainsSearchWord_Patch
{
    public static void Postfix(Tag tag, string search, ref bool __result)
    {
        if (__result || !SearchBarValueChanged_Patch.DevPumpSearch || string.IsNullOrEmpty(search))
        {
            return;
        }

        __result = tag.ToString().ToUpperInvariant().Contains(search.ToUpperInvariant());
    }
}
