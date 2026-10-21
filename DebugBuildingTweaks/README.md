# Debug Building Tweaks

## Build

`Local.Build.props` points to the Oxygen Not Included `Managed` directory and the local mod install directory. It is machine-specific and ignored by Git. To configure another machine, copy `Local.Build.props.example` to `Local.Build.props` and update both paths.

```powershell
dotnet build .\DebugBuildingTweaks.sln
```
