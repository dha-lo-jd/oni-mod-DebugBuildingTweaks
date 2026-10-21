# Debug Building Tweaks

This mod improves the usability of debug buildings in Oxygen Not Included.  
Right now it focuses on **Dev Pumps (Liquid/Gas)** and adds output-rate and temperature controls.

## Steam Description (Copy/Paste)

A small tweak mod that makes debug pumps (Dev Pumps) easier to use.  
You can directly configure output rate and output temperature from the side screen.

- Liquid Dev Pump output cap: **10000 g/s**
- Gas Dev Pump output cap: **1000 g/s**
- Temperature setting support (follows in-game temperature units)
- Improved element filter search (matches both display names and internal IDs)

Designed mainly for debugging and test scenarios.  
This is not intended as a normal gameplay balance mod.

## Features

### 1. Dev Pump Output Control

- Adjust output rate from the side screen using a slider or numeric input
- Output caps:
  - Liquid: **10000 g/s**
  - Gas: **1000 g/s**

### 2. Dev Pump Temperature Control

- Adjust output temperature from the side screen
- Temperature display follows the game unit setting (e.g. °C / K)

### 3. Enhanced Filter Search

- Dev Pump element filtering now matches internal IDs (e.g. `OxygenGas`) in addition to display names

## Compatibility

- ONI build: **744825 or later**
- Required DLC: none
- Mod version: **1.0.0**

## Usage

1. Place a Dev Pump (Liquid or Gas) from debug buildings
2. Select the building and open its side screen
3. Adjust `Output` and `Temperature`

## Notes

- Primarily intended for debugging workflows
- May conflict with other mods that modify UI/side-screen behavior

## Technical Notes

- Built with Harmony + PLib
- Main patch targets:
  - `DevPumpGasConfig.DoPostConfigureComplete`
  - `DevPumpLiquidConfig.DoPostConfigureComplete`
  - `DevPump.Sim1000ms`
  - `DetailsScreen.OnPrefabInit`
