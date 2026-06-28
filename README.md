# gregMod.HexViewer - Hardware Inspector

> Standalone mod for **Data Center** — inspect hex colors of hardware components.

**Author:** mleem97 (teamGreg) | **License:** MIT

---

## Features

- **F1** — Toggle HexViewer HUD (raycast crosshair targeting)
- **F2** — Open full color list (scene + save data + JSON)
- **F8** — Toggle config panel
- World-space hex labels on CableSpinners and Racks
- Cable port detection (RJ45 / SFP / QSFP)
- Held cable / rack detection
- Colorblind mode
- JADE-inspired dark UI

## Installation

1. Install **MelonLoader** (v0.6+)
2. Place `gregMod.HexViewer.dll` into `Game/Mods/`
3. Start the game and press **F1**

## Dependencies

**None** — fully standalone.

## Building from Source

```bash
dotnet build -c Release
# Output: bin/Release/net6.0/gregMod.HexViewer.dll
```

## Configuration

File: `UserData/hexposition.cfg` (created on first start).

```ini
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

**Live reload:** Ctrl+F1 toggles periodic reload.

## Hotkeys

| Key | Action |
|-----|--------|
| F1 | Toggle HUD |
| F2 | Toggle color list |
| Ctrl+F1 | Toggle live reload |

## Contributors

- @mleem97
- @Joniii11

---

Merged from [gregMod.HexViewer](https://github.com/mleem97/gregMod.HexViewer) and [gregModHexLabelMod](https://github.com/mleem97/gregModHexLabelMod).
