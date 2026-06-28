# Source layout

All source lives under `HexViewer/` with root namespace **`GregModHexViewer`**. No sub-namespaces.

## Tree

```
HexViewer/
├── HexViewerMod.cs            # MelonLoader entry point (MelonMod)
├── HexViewerFeature.cs        # HUD overlay + F2 color list window
├── HexTargetResolver.cs       # Raycast from camera for aimed-at cable/rack
├── HeldCableKindResolver.cs   # Resolves held item type + hex via reflection
├── GameObjectColorHex.cs      # Extracts hex color from CableSpinner / Rack
├── GameObjectKindResolver.cs  # Port type (RJ/SFP/QSFP) + rack variant detection
├── CablePortKindUtil.cs       # Port string classification helpers
├── CableColorCollector.cs     # Aggregates colors from scene, save data, JSON
└── HexColorUtil.cs            # Color ↔ hex conversion utilities
```

## File descriptions

### `HexViewerMod.cs`
MelonLoader entry point. Registers `HexviewerFeature` on init, delegates `OnUpdate`, `OnGUI`, and `OnDeinitializeMelon`. Waits for `NetworkMap.instance` before marking the mod as ready.

### `HexViewerFeature.cs`
Core feature class (static). Manages the always-on HUD (top-right corner, Jade-style dark panel) and the F2 toggle overlay (centered window with scrollable color list). Renders hex code, color swatch, port tag (RJ/SFP/QSFP), and detail text. Draws `CableColorEntry` items from `CableColorCollector`.

### `HexTargetResolver.cs`
Fires a raycast from `Camera.main` (max 48 m). On hit, checks for `CableSpinner` or `Rack` components and delegates hex extraction to `GameObjectColorHex`. Returns hex + detail suffix (e.g. "Kabelrolle · RJ").

### `HeldCableKindResolver.cs`
Uses reflection on `PlayerClass` fields/properties to find the held item. Classifies port kind via `CablePortKindUtil`, extracts hex from `Rack`, `CableSpinner`, `GameObject`, or `Component`. Provides `TryGetHeldItemHex` (HUD path) and `TryGetHeldCableHex` (fallback).

### `GameObjectColorHex.cs`
Shared hex resolution for in-world objects. `TryGetSpinnerHex` reads `CableSpinner.rgbColor` or falls back to material `_BaseColor` / `_Color`. `TryGetRackHex` iterates child renderers for the first material color.

### `GameObjectKindResolver.cs`
Detects port type from `CableSpinner` text labels and rack variant ("Normal" / "Colored") via TMP text scanning, string member reflection, and game object name heuristics.

### `CablePortKindUtil.cs`
Static helper. `ClassifyPortString` maps UI text to "RJ45", "SFP", or "QSFP". `ToShortPortLabel` converts "RJ45" → "RJ" for HUD display.

### `CableColorCollector.cs`
Aggregates unique hex colors from three sources:
1. **Scene** — all `CableSpinner` instances (`rgbColor` or material color)
2. **Save reflection** — walks `Save.member_values` / `SaveData` via reflection for color strings
3. **Save JSON files** — scans `Application.persistentDataPath` for `.json` / `.txt` / `.save` / `.dat` files containing cable color keys

Returns `List<CableColorEntry>` sorted by hex.

### `HexColorUtil.cs`
Color utility. `ToHex(Color)` → `#RRGGBB`. `TryNormalizeHex` handles `#RGB`, `#RRGGBB`, `#AARRGGBB`, HTML strings, and `r,g,b` comma-separated values. `TryHexToColor` converts back to `UnityEngine.Color`.
