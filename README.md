# gregMod.HexLabelMod (FMF HexLabel Mod)

MelonLoader mod for **Data Center**: shows hex color codes for `CableSpinner` and `Rack` in-world (no menu). Standalone, **without** FMF runtime APIs.

| | |
|:---|:---|
| **In workspace** | Path `gregFramework/gregMod.HexLabelMod/`. Overview: [gregFramework README](../README.md). |
| **Remote** | [`mleem97/gregModHexLabelMod`](https://github.com/mleem97/gregModHexLabelMod) |

## Prerequisites

| Dependency | Note |
|------------|------|
| [MelonLoader](https://melonwiki.xyz/) | With generated IL2CPP assemblies |

## Installation

1. Place `FMF.HexLabelMod.dll` in `Mods/`.
2. Launch the game — configuration is created on first run under `UserData/hexposition.cfg`.

## Cable color viewer (F2)

**F2** opens a panel with unique cable colors (hex + swatch): scene, save data, save files. **Refresh** / **Close** or press **F2** again. **Colorblind mode** enlarges the “hero” row among other tweaks.

## Configuration

File: `UserData/hexposition.cfg` (created on first start; missing keys are added).

```ini
# Hex Label Position Config
spinner_offset_x=0
spinner_offset_y=-6
spinner_font_min=1.8
spinner_font_max=6.2
spinner_font_scale=0.24
rack_offset_right=-0.03
rack_offset_back=0.06
rack_offset_down=-0.02
rack_font_size=42
rack_character_size=0.05
rack_scale=1
```

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `spinner_offset_x` | float | `0` | Horizontal offset relative to the source label |
| `spinner_offset_y` | float | `-6` | Vertical offset relative to the source label |
| `spinner_font_min` | float | `1.8` | Min auto-size (TMPro) |
| `spinner_font_max` | float | `6.2` | Max auto-size (TMPro) |
| `spinner_font_scale` | float | `0.24` | Scale relative to source font size |
| `rack_offset_right` | float | `-0.03` | World space along rack “right” |
| `rack_offset_back` | float | `0.06` | World space along rack “back” |
| `rack_offset_down` | float | `-0.02` | World space along rack “down” |
| `rack_font_size` | int | `42` | `TextMesh` font size |
| `rack_character_size` | float | `0.05` | Character size (`TextMesh`) |
| `rack_scale` | float | `1` | Uniform world scale of the rack label |

**Live reload:** Ctrl+F1 toggles periodic reload (limited; see source).

## Build

```powershell
Set-Location $PSScriptRoot
dotnet build .\FMF.HexLabelMod.csproj
```

## Behavior (short)

Harmony patches on `CableSpinner`, world-space labels on `Rack`, scan loop; details in source.

## Notes

- Gameplay unchanged.
- Live reload (Ctrl+F1) may be limited (see source).
