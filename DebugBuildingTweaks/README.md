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

Checked against the U60 public test build; `minimumSupportedBuild` in `mod.yaml` / `mod_info.yaml` will be bumped once U60 is released.
