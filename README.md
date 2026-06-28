# gregMod.HexViewer

> Hardware Inspector for **Data Center** — hex color codes, cable type detection, and live world labels.

[![Discord](https://img.shields.io/badge/Discord-Join-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/greg)
[![gregFramework](https://img.shields.io/badge/gregFramework-Website-blue?style=for-the-badge)](https://gregframework.eu)
[![License](https://img.shields.io/badge/License-Apache%202.0-green?style=for-the-badge)](./LICENSE)
[![Version](https://img.shields.io/badge/Version-1.0.0-orange?style=for-the-badge)]()
[![GameVersion](https://img.shields.io/badge/Game%20Version-1.0.50.15-yellow?style=for-the-badge)]()
[![Unity](https://img.shields.io/badge/Unity-6000.5-black?style=for-the-badge&logo=unity&logoColor=white)]()

## Links

- **Website:** [gregframework.eu](https://gregframework.eu)
- **Discord / Support:** [discord.gg/greg](https://discord.gg/greg)
- **Repository:** [github.com/mleem97/gregMod.HexViewer](https://github.com/mleem97/gregMod.HexViewer)

## Overview

**gregMod.HexViewer** extends **Data Center** with a hardware inspection overlay. Point your crosshair at any cable reel, rack, or device to see its hex color code, cable type (RJ45 / SFP / QSFP), and port info — live in-world.

The project is designed as **standalone**: no external framework dependencies, works out of the box with just MelonLoader.

> **Note:** Legacy builds (gregCore-dependent) are no longer available. The project has been migrated to a standalone architecture following the game's Unity engine upgrade.

## Current Features

- **F1** — Toggle HUD overlay (crosshair targeting, hex color + type info)
- **F2** — Open full color list (scene, save data, JSON files)
- **Ctrl+F1** — Live config reload
- World-space hex labels on CableSpinners and Racks
- Cable port detection (RJ45 / SFP / QSFP)
- Held cable / rack detection
- Colorblind mode with enlarged hex display
- Dark JADE-inspired UI design
- Configurable label positioning via `hexposition.cfg`

## Planned Focus Areas

- Improved targeting precision and distance
- More device types (servers, switches, patch panels)
- Hex history and comparison view
- Export color palettes
- Integration with network management features

## Installation

1. Install **MelonLoader** for **Data Center**.
2. Copy the release DLL into the mod folder:

   ```text
   Game/Mods/gregMod.HexViewer.dll
   ```

3. Start the game.
4. Press **F1** to toggle the HUD, **F2** to open the color list.

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **F1** | Toggle HexViewer HUD |
| **F2** | Toggle full color list panel |
| **Ctrl+F1** | Reload config from disk |

## Configuration

File: `UserData/hexposition.cfg` (created on first start).

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
| `spinner_offset_x` | float | `0` | Horizontal offset for cable reel labels |
| `spinner_offset_y` | float | `-6` | Vertical offset for cable reel labels |
| `spinner_font_min` | float | `1.8` | Min auto-size (TMPro) |
| `spinner_font_max` | float | `6.2` | Max auto-size (TMPro) |
| `spinner_font_scale` | float | `0.24` | Scale relative to source font size |
| `rack_offset_right` | float | `-0.03` | World space offset along rack right |
| `rack_offset_back` | float | `0.06` | World space offset along rack back |
| `rack_offset_down` | float | `-0.02` | World space offset along rack down |
| `rack_font_size` | int | `42` | TextMesh font size for rack labels |
| `rack_character_size` | float | `0.05` | Character size (TextMesh) |
| `rack_scale` | float | `1` | Uniform world scale of rack labels |

## Dependencies

Runtime / mod setup requirements:

- **MelonLoader**
- **Il2CppInterop**
- **Harmony**
- Unity / game interop assemblies from the local Data Center installation

## Build from Source

Requirements:

- .NET 6 SDK
- local Data Center / MelonLoader installation
- available interop assemblies in `references/`

Build:

```bash
git clone https://github.com/mleem97/gregMod.HexViewer.git
cd gregMod.HexViewer
git checkout v1.0.0
dotnet build -c Release
```

Release output:

```text
bin/Release/net6.0/gregMod.HexViewer.dll
```

## Project Structure

- **`HexViewerMod.cs`** — MelonLoader entry point, Harmony patches, label management
- **`HexPositionConfig.cs`** — Config loader for label positioning
- **`HexColorUtil.cs`** — Color ↔ hex conversion utilities
- **`GameObjectColorHex.cs`** — Hex extraction from CableSpinner and Rack
- **`GameObjectKindResolver.cs`** — Port type and rack variant detection
- **`CablePortKindUtil.cs`** — RJ45 / SFP / QSFP classification
- **`HeldCableKindResolver.cs`** — Held item hex resolution
- **`CableColorCollector.cs`** — Color collection from scene, save, JSON
- **`HexTargetResolver.cs`** — Crosshair raycast targeting
- **`HexViewerFeature.cs`** — HUD overlay and F2 color list UI

## Community & Support

Questions, feedback, testing, and modding coordination happen on the greg Discord:

- [discord.gg/greg](https://discord.gg/greg)

## Sponsors & Thanks

- **[@tobiasreichel](https://github.com/tobiasreichel)** — main sponsor

## Credits

| Role | Contributor |
|------|-------------|
| **Codebase** | [mleem97](https://github.com/mleem97) ([TeamGreg Modding](https://github.com/teamGregModding)) |
| **Hex Label System** | [mleem97](https://github.com/mleem97), [Joniii11](https://github.com/Joniii11) |
| **Community Testing** | Noootry, TheSlickers, Jarvis, Kirei, TeamWaseku |

## Contributing

Contributions are welcome. Useful starting points:

- report bugs or regressions as issues
- provide reproducible test cases
- discuss roadmap items
- keep pull requests small and easy to review

## License

This project is licensed under the **Apache License 2.0**. See [`LICENSE`](./LICENSE).

---

**gregFramework — powered by the community.**
