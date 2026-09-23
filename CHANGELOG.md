# Changelog — gregMod.HexViewer

Format: [Keep a Changelog](https://keepachangelog.com/de/1.0.0/). Version: siehe [`VERSION`](VERSION).

## [Unreleased]

### Fixed

- F2-Panel: `DrawColorList` erzeugte `NotSupportedException` (`GUILayout.BeginScrollView` /
  unstrip-fail) → manuelle Scroll-Fallback wie BetterShop SafeScroll (Viewport-Rect +
  `GUI.BeginScrollView` try/catch → Groups + Wheel). Version 1.0.7.

### Added

- Konfigurierbarer Toggle-Hotkey (`ToggleKey`-Pref, Default F2), Mod-Vertrag, Tasten-HUD-Eintrag und Oeffner fuers F1-Hub (nur mit gregCore).
- Einheitliches Open-Source-Layout (README, Docs, Badges) nach gregCore-Vorbild.

## [0.1.0] — 2026-09-22

- Initialer standardisierter Stand.
