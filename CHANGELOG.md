# Changelog — gregMod.HexViewer

Format: [Keep a Changelog](https://keepachangelog.com/de/1.0.0/). Version: see [`VERSION`](VERSION).

## [1.0.9] — 2026-09-24

### Changed

- English strings and docs.

## [1.0.8] — 2026-09-24

### Changed

- UI fully centralized in gregCore (hard dependency): color list is a
  `GregPanelBuilder` toolkit panel, events go through
  `GregNotificationManager` toasts, aimed/held readout is a throttled
  `ShowRich` toast with color swatch. All own IMGUI removed (250+ lines:
  HUD box, list window, scroll fallback, textures). No standalone fallback;
  fail-fast with a clear error when `gregCore.dll` is missing.
  F1-hub wiring via `GregMenuBinding.BindToggle`.

## [Unreleased]

### Fixed

- F2 panel: `DrawColorList` threw `NotSupportedException` (`GUILayout.BeginScrollView` /
  unstrip-fail) → manual scroll fallback like BetterShop SafeScroll (viewport rect +
  `GUI.BeginScrollView` try/catch → groups + wheel). Version 1.0.7.

### Added

- Configurable toggle hotkey (`ToggleKey` pref, default F2), mod contract, key-HUD entry, and opener for the F1 hub (only with gregCore).
- Unified open-source layout (README, docs, badges) following the gregCore template.

## [0.1.0] — 2026-09-22

- Initial standardized baseline.
