using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.UI;
using UnityEngine;

namespace DebugBuildingTweaks;

[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
public static class DetailsScreen_OnPrefabInit_Patch
{
    public static void Postfix() => PUIUtils.AddSideScreenContent<DevPumpSideScreen>();
}

// Attaches DevPumpSettings to both the liquid and the gas dev pump.
[HarmonyPatch]
public static class DevPumpConfig_Patch
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(DevPumpLiquidConfig), nameof(DevPumpLiquidConfig.DoPostConfigureComplete));
        yield return AccessTools.Method(typeof(DevPumpGasConfig), nameof(DevPumpGasConfig.DoPostConfigureComplete));
    }

    public static void Postfix(GameObject go) => go.AddOrGet<DevPumpSettings>();
}

[HarmonyPatch(typeof(DevPump), nameof(DevPump.Sim1000ms))]
public static class DevPump_Sim1000ms_Patch
{
    // Returning false skips the vanilla implementation.
    public static bool Prefix(DevPump __instance)
    {
        DevPumpSettings settings = __instance.GetComponent<DevPumpSettings>();
        return settings == null || !settings.TryFillStorage();
    }
}

// Makes the dev pump element filter match the internal element ID (e.g. "OxygenGas") as well as the display name.
// TagContainsSearchWord is static and has no idea which screen is calling it, so the search handler flags the context.
public static class DevPumpSearchPatches
{
    private static bool searchingDevPump;

    [HarmonyPatch(typeof(SingleItemSelectionSideScreenBase), "OnSearchBarValueChanged")]
    public static class OnSearchBarValueChanged_Patch
    {
        public static void Prefix(SingleItemSelectionSideScreenBase __instance)
        {
            searchingDevPump = __instance is FilterSideScreen &&
                               Traverse.Create(__instance).Field("targetFilterable").GetValue<Filterable>() is DevPump;
        }

        // A finalizer, unlike a postfix, also runs when the original throws, so the flag can't leak.
        public static void Finalizer() => searchingDevPump = false;
    }

    [HarmonyPatch(typeof(SingleItemSelectionSideScreenBase), "TagContainsSearchWord")]
    public static class TagContainsSearchWord_Patch
    {
        public static void Postfix(Tag tag, string search, ref bool __result)
        {
            if (__result || !searchingDevPump || string.IsNullOrEmpty(search))
            {
                return;
            }

            __result = tag.ToString().ToUpperInvariant().Contains(search.ToUpperInvariant());
        }
    }
}
