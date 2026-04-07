<!-- markdownlint-disable MD060 -->

# FMF HexLabel Mod

Standalone MelonLoader mod for **Data Center** (Waseku) that overlays the hex color code of each `CableSpinner` and `Rack` directly in-world — so you can identify cable and rack colors at a glance without opening any menu.

Rewritten from the former root `HexLabelMod` for the FrikaModdingFramework workflow.

***

## Requirements

| Dependency | Notes |
|---|---|
| [MelonLoader](https://melonwiki.xyz/) | With generated IL2CPP assemblies |

This mod runs standalone and does **not** require FMF runtime APIs.

***

## Installation

1. Drop `FMF.HexLabelMod.dll` into your `Mods/` folder.
2. Launch the game — the config file is created automatically on first run at:

```text
UserData/hexposition.cfg
```

***

## Cable color viewer (F2)

Press **F2** to open an on-screen panel that lists **unique cable colors** as **hex** with a small **color swatch**:

- **Scene**: colors from all `CableSpinner` instances (matches what is loaded from your save in-game).
- **Save data**: reflection on `Save` / `member_values` / `cableColor` when the game exposes them.
- **Save files**: scans `Application.persistentDataPath` for JSON/text-like files containing `cableColor` or `member_values`.

Use **Refresh** to rescan; **Close** or **F2** again to hide.

**Colorblind mode** (toggle in the panel): enlarges the **Held** line and shows **RJ45 / SFP / QSFP** (when detectable via reflection on the player / held item) together with the **hex** color for the cable you are holding.

***

## Configuration

Edit `UserData/hexposition.cfg` to adjust label positioning and font sizes. The file is auto-generated with defaults on first launch and regenerated if any keys are missing.

```ini
# Hex Label Position Config
# File: UserData/hexposition.cfg
# Edit values, then restart game.

# Spinner (UI text near cable spool)
spinner_offset_x=0
spinner_offset_y=-6
spinner_font_min=1.8
spinner_font_max=6.2
spinner_font_scale=0.24

# Rack (world-space text at rack back-right-bottom)
rack_offset_right=-0.03
rack_offset_back=0.06
rack_offset_down=-0.02
rack_font_size=42
rack_character_size=0.05
rack_scale=1
```

### Config Keys

| Key | Type | Default | Description |
|---|---|---|---|
| `spinner_offset_x` | float | `0` | Horizontal offset relative to the source label |
| `spinner_offset_y` | float | `-6` | Vertical offset relative to the source label |
| `spinner_font_min` | float | `1.8` | Minimum auto-size font size (TMPro) |
| `spinner_font_max` | float | `6.2` | Maximum auto-size font size (TMPro) |
| `spinner_font_scale` | float | `0.24` | Scale factor applied to the source label's font size |
| `rack_offset_right` | float | `-0.03` | World-space offset along rack's right axis |
| `rack_offset_back` | float | `0.06` | World-space offset along rack's back axis |
| `rack_offset_down` | float | `-0.02` | World-space offset along rack's down axis |
| `rack_font_size` | int | `42` | Font size for the world-space `TextMesh` label |
| `rack_character_size` | float | `0.05` | Character size for the world-space `TextMesh` label |
| `rack_scale` | float | `1` | Uniform world-space scale of the rack label object |

***

## Live Reload *(restricted)*

Pressing **Ctrl+F1** toggles live config reload (6-second interval), allowing you to tune label positions without restarting the game. This feature is restricted to a specific Steam account and will silently do nothing for all other users.

***

## Build

```powershell
dotnet build .\mods\FMF.Mod.HexLabelMod\FMF.HexLabelMod.csproj
```

Output lands in the standard MelonLoader `Mods/` folder as configured in the `.csproj`.

***

## How It Works

1. **Startup** — The mod defers full initialization until MelonLoader's `Latest.log` confirms the Steam runtime is ready (SteamID or Steam marker detected).
2. **Spinner labels** — Every `CableSpinner` gets a cloned `TextMeshProUGUI` label injected into its UI hierarchy, displaying the resolved hex code in white. Color is read from `rgbColor`, then from the `_BaseColor`/`_Color` material property as fallback.
3. **Rack labels** — Every `Rack` gets a world-space `TextMesh` label positioned at its back-right-bottom corner, facing away from the rack front.
4. **Scan loop** — Active spinners and racks are re-checked every 1.5 seconds to catch newly spawned objects.
5. **Harmony patch** — `CableSpinner.Start` is patched to inject the label immediately on spawn, before the first scan cycle runs.

***

## Notes

- Original gameplay behavior is unaffected.
- This mod runs from `mods/FMF.Mod.HexLabelMod` and no longer from the repository root.
- The config file is fully rewritten if any expected keys are missing (e.g. after an update adds new keys).
- FMF assembly presence is no longer required as a startup gate.
