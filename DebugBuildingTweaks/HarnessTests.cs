#if DEBUG
// In-game tests run by Gravitas.TestHarness:
//   python GravitasTestHarness/Gravitas.TestProbe/probe.py run --project <this repo> --tests '{"tags":["debugbuildingtweaks"]}'
// Needs HarnessDllPath in Local.Build.props (Check / Wait come from the harness assembly).
using System.Collections;
using Gravitas.TestHarness;
using HarmonyLib;
using UnityEngine;

namespace DebugBuildingTweaks;

internal static class HarnessTests
{
    private static GameObject BuildPump(string prefabId)
    {
        var cell = Arrange.FindBuildableCell(prefabId, Arrange.HomeCell());
        Check.True(cell >= 0, "no free place for " + prefabId);
        return Arrange.Build(prefabId, cell);
    }

    private static float StoredTemperature(Storage storage)
    {
        foreach (var item in storage.items)
        {
            var element = item != null ? item.GetComponent<PrimaryElement>() : null;
            if (element != null && element.Mass > 0f) return element.Temperature;
        }

        return -1f;
    }

    [HarnessTest(Name = "debugbuildingtweaks.mod_loaded", Tags = "debugbuildingtweaks")]
    public static void ModLoaded()
    {
        Check.True(DebugBuildingTweaksMod.modPath != null, "DebugBuildingTweaksMod.OnLoad has not run");
    }

    [HarnessTest(Name = "debugbuildingtweaks.settings_attached", Tags = "debugbuildingtweaks")]
    public static void SettingsAttached()
    {
        var liquid = BuildPump("DevPumpLiquid");
        var gas = BuildPump("DevPumpGas");
        try
        {
            var l = liquid.GetComponent<DevPumpSettings>();
            var g = gas.GetComponent<DevPumpSettings>();
            Check.True(l != null, "liquid pump has no DevPumpSettings");
            Check.True(g != null, "gas pump has no DevPumpSettings");
            Check.Equal(DevPumpSettings.LiquidMaxGramsPerSecond, l.MaxGramsPerSecond);
            Check.Equal(DevPumpSettings.GasMaxGramsPerSecond, g.MaxGramsPerSecond);
            g.GramsPerSecond = 99999f;
            Check.Equal(DevPumpSettings.GasMaxGramsPerSecond, g.GramsPerSecond);
        }
        finally
        {
            Arrange.Destroy(liquid);
            Arrange.Destroy(gas);
        }
    }

    // The replaced Sim1000ms fills the storage up to the configured rate at the configured temperature.
    [HarnessTest(Name = "debugbuildingtweaks.fill_rate_and_temperature", Tags = "debugbuildingtweaks")]
    public static void FillRateAndTemperature()
    {
        var go = BuildPump("DevPumpLiquid");
        try
        {
            var pump = go.GetComponent<DevPump>();
            var settings = go.GetComponent<DevPumpSettings>();
            var storage = go.GetComponent<Storage>();
            var water = ElementLoader.FindElementByHash(SimHashes.Water).tag;
            pump.SelectedTag = water;

            settings.GramsPerSecond = 5000f;
            ((ISim1000ms)pump).Sim1000ms(1f);
            Check.Near(5f, storage.GetAmountAvailable(water), 0.01f, "stored amount at 5000 g/s");

            settings.SetTemperature(400f);
            ((ISim1000ms)pump).Sim1000ms(1f);
            Check.Near(5f, storage.GetAmountAvailable(water), 0.01f, "stored amount after temperature change");
            Check.Near(400f, StoredTemperature(storage), 0.5f, "stored temperature");

            settings.GramsPerSecond = 10000f;
            ((ISim1000ms)pump).Sim1000ms(1f);
            Check.Near(10f, storage.GetAmountAvailable(water), 0.01f, "stored amount at 10000 g/s");
        }
        finally
        {
            Arrange.Destroy(go);
        }
    }

    // The element filter search also matches internal IDs such as "CarbonDioxide" (display name "Carbon Dioxide").
    [HarnessTest(Name = "debugbuildingtweaks.search_matches_internal_id", Tags = "debugbuildingtweaks")]
    public static void SearchMatchesInternalId()
    {
        var method = AccessTools.Method(typeof(SingleItemSelectionSideScreenBase), "TagContainsSearchWord");
        Check.True(method != null, "TagContainsSearchWord not found");
        var tag = ElementLoader.FindElementByHash(SimHashes.CarbonDioxide).tag;

        // Outside a dev pump search the postfix must not widen the match.
        Check.False((bool)method.Invoke(null, new object[] { tag, "carbondioxide" }), "matched outside a dev pump search");

        AccessTools.Field(typeof(DevPumpSearchPatches), "searchingDevPump").SetValue(null, true);
        try
        {
            Check.True((bool)method.Invoke(null, new object[] { tag, "carbondioxide" }), "internal id did not match");
        }
        finally
        {
            AccessTools.Field(typeof(DevPumpSearchPatches), "searchingDevPump").SetValue(null, false);
        }
    }

    // Selecting a dev pump opens the settings side screen.
    [HarnessTest(Name = "debugbuildingtweaks.side_screen_shown", Tags = "debugbuildingtweaks")]
    public static IEnumerator SideScreenShown()
    {
        var go = BuildPump("DevPumpLiquid");
        try
        {
            SelectTool.Instance.Select(go.GetComponent<KSelectable>());
            yield return null;
            yield return null;
            yield return null;
            var screen = Object.FindObjectOfType<DevPumpSideScreen>();
            Check.True(screen != null, "DevPumpSideScreen was not created");
            Check.True(screen.gameObject.activeInHierarchy, "DevPumpSideScreen is not shown for the selected pump");
        }
        finally
        {
            SelectTool.Instance.Select(null);
            Arrange.Destroy(go);
        }
    }
}
#endif
