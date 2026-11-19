# Debug Building Tweaks

## Build

`Local.Build.props` points to the Oxygen Not Included `Managed` directory and the local mod install directory. It is machine-specific and ignored by Git. To configure another machine, copy `Local.Build.props.example` to `Local.Build.props` and update both paths.

```powershell
dotnet build .\DebugBuildingTweaks.sln
```

## Structure

- `DebugBuildingTweaksMod.cs` – mod entry point (initialises PLib).
- `DevPumpSettings.cs` – per-pump output rate / temperature and the replacement fill logic. The rate limit is derived from the pump's element state.
- `DevPumpPatches.cs` – Harmony patches: attach the settings, redirect `DevPump.Sim1000ms`, and extend the element search.
- `DevPumpSideScreen.cs` – PLib side screen for editing the settings.

## Game compatibility

Updated for U60 (the Dev Pump patches were refactored to match the new game code). Verified to build against the released U60; `minimumSupportedBuild` is left unchanged.

## Changelog

### 1.0.1
- U60 compatibility: refactored the Dev Pump patches; the settings panel is now placed below the vanilla filter panel.
- Side screen text moved to `STRINGS.cs` with Japanese translation (`translations/ja.po`).
