# gregMod.HexViewer

> Hardware inspector for **Data Center** — hex color codes, cable type detection, and live overlay.

[![Discord](https://img.shields.io/badge/Discord-Join-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/greg)
[![gregFramework](https://img.shields.io/badge/gregFramework-Website-blue?style=for-the-badge)](https://gregframework.eu)
[![License](https://img.shields.io/badge/License-Apache%202.0-green?style=for-the-badge)](./LICENSE)
[![Version](https://img.shields.io/badge/Version-1.0.0-orange?style=for-the-badge)]()
[![GameVersion](https://img.shields.io/badge/Game%20Version-1.0.50.15-yellow?style=for-the-badge)]()
[![Unity](https://img.shields.io/badge/Unity-6000.5-black?style=for-the-badge&logo=unity&logoColor=white)]()

## Links

- **Repository:** [github.com/mleem97/gregMod.HexViewer](https://github.com/mleem97/gregMod.HexViewer)
- **Discord / Support:** [discord.gg/greg](https://discord.gg/greg)
- **Website:** [gregframework.eu](https://gregframework.eu)

## Overview

**gregMod.HexViewer** adds a hardware inspection overlay to **Data Center**. Point your crosshair at a cable reel, rack, or hold a cable to see its hex color code and cable type (RJ / SFP / QSFP) — rendered as an opaque overlay in the top-right corner.

The project is **standalone**: no external framework dependencies, works out of the box with just MelonLoader.

## Features

- Context-sensitive HUD overlay (only visible when inspecting a rack, cable, or cable reel)
- Hex color code display with live color swatch
- Cable type detection (RJ / SFP / QSFP) for cable reels and held cables
- Rack variant detection (Normal / Colored)
- Opaque dark UI design matching gregMod.IPAM
- World-space hex labels on CableSpinners and Racks
- Full color list panel (F2) with scene, save data, and JSON sources
- Colorblind mode with enlarged hex display
- Configurable label positioning via `hexposition.cfg`
- Live config reload (Ctrl+F1)

## Installation

1. Install **MelonLoader** (v0.7.2+) for **Data Center**
2. Copy the release DLL into the mod folder:

   ```text
   Game/Mods/gregMod.HexViewer.dll
   ```

3. Start the game
4. Point at a rack, cable reel, or hold a cable to see the overlay

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **F2** | Toggle full color list panel |
| **Ctrl+F1** | Reload config from disk |

## Configuration

File: `UserData/hexposition.cfg` (created on first start).

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

## How It Works

1. On first load, the mod scans the scene for `CableSpinner` and `Rack` objects
2. A Harmony patch attaches hex labels to new CableSpinners on spawn
3. Every 1.5s, all cached spinners and racks are scanned for label updates
4. Every frame, the HUD checks the crosshair raycast and held item:
   - If aimed at a rack, cable reel, or holding a cable → overlay appears (top-right)
   - Otherwise → overlay hidden
5. Hex colors are resolved from `rgbColor`, material `_BaseColor` / `_Color`, or reflection
6. Port types (RJ / SFP / QSFP) are detected from TMPro text, string fields, or object reflection

## Dependencies

- **MelonLoader** (v0.7.2+)

### Build only

- **Il2CppInterop**
- **Harmony**
- Unity / game interop assemblies from the local Data Center installation

## Build from Source

Requirements:

- .NET 6 SDK
- local Data Center / MelonLoader installation

Build:

```bash
git clone https://github.com/mleem97/gregMod.HexViewer.git
cd gregMod.HexViewer
dotnet build -c Release
```

Release output:

```text
bin/Release/net6.0/gregMod.HexViewer.dll
```

## Project Structure

```
gregMod.HexViewer/
├── HexViewer/                  # Source code
│   ├── HexViewerMod.cs         # MelonLoader entry point, Harmony patches, label management
│   ├── HexViewerFeature.cs     # HUD overlay and F2 color list UI
│   ├── HexTargetResolver.cs    # Crosshair raycast targeting
│   ├── HeldCableKindResolver.cs# Held item hex and port resolution
│   ├── GameObjectColorHex.cs   # Hex extraction from CableSpinner and Rack
│   ├── GameObjectKindResolver.cs# Port type and rack variant detection
│   ├── CablePortKindUtil.cs    # RJ / SFP / QSFP classification
│   ├── CableColorCollector.cs  # Color collection from scene, save, JSON
│   ├── HexColorUtil.cs         # Color ↔ hex conversion utilities
│   └── HexPositionConfig.cs    # Config loader for label positioning
├── references/                 # Game & MelonLoader interop DLLs
├── gregMod.HexViewer.csproj    # Project file
├── manifest.json               # MelonLoader mod manifest
├── build.ps1                   # Build script
├── LICENSE                     # Apache 2.0
└── README.md
```

## Credits

| Role | Contributor |
|------|-------------|
| **Codebase** | [mleem97](https://github.com/mleem97) ([TeamGreg Modding](https://github.com/teamGregModding)) |
| **Hex Label System** | [mleem97](https://github.com/mleem97), [Joniii11](https://github.com/Joniii11) |
| **Community Testing** | Noootry, TheSlickers, Jarvis, Kirei, TeamWaseku |

## License

This project is licensed under the **Apache License 2.0**. See [`LICENSE`](./LICENSE).

---

**gregFramework — powered by the community.**
