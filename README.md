# gregMod.HexViewer

> Hardware inspector for **Data Center** — hex color codes, cable type detection, and live overlay.

[![Discord](https://img.shields.io/discord/1392073682133848075?style=for-the-badge&logo=discord&logoColor=white&label=Discord)](https://discord.gg/greg)
[![gregFramework](https://img.shields.io/badge/gregFramework-Website-blue?style=for-the-badge)](https://gregframework.eu)
[![License](https://img.shields.io/badge/License-Apache%202.0-green?style=for-the-badge)](./LICENSE)
[![Version](https://img.shields.io/badge/Version-1.0.6-orange?style=for-the-badge)]()
[![GameVersion](https://img.shields.io/badge/Game%20Version-1.1.0-yellow?style=for-the-badge)]()
[![Unity](https://img.shields.io/badge/Unity-6000.4.12f1-black?style=for-the-badge&logo=unity&logoColor=white)]()

## Links

- **Repository:** [github.com/mleem97/gregMod.HexViewer](https://github.com/mleem97/gregMod.HexViewer)
- **Discord / Support:** [discord.gg/greg](https://discord.gg/greg)
- **Website:** [gregframework.eu](https://gregframework.eu)

## Overview

**gregMod.HexViewer** adds a hardware inspection overlay to **Data Center**. Point your crosshair at a cable reel, rack, or hold a cable to see its hex color code and cable type (RJ / SFP / QSFP) — rendered as an opaque overlay in the top-right corner.

Zero configuration required — just install and play.

## Features

- Context-sensitive HUD overlay (only visible when inspecting a rack, cable, or cable reel)
- Hex color code display with color swatch box
- Cable type detection (RJ / SFP / QSFP) for cable reels and held cables
- Rack variant detection (Normal / Colored)
- Opaque dark UI design (Jade-style)
- No configuration needed — works out of the box

## Installation

1. Install **MelonLoader** (v0.7.2+) for **Data Center**
2. Copy the release DLL into the mod folder:

   ```text
   Game/Mods/gregMod.HexViewer.dll
   ```

3. Start the game
4. Point at a rack, cable reel, or hold a cable to see the overlay

## Dependencies

- **MelonLoader** (v0.7.2+)

### Build only

- **Il2CppInterop**
- Unity / game interop assemblies from the local Data Center installation

## Build from Source

Requirements:

- .NET 6 SDK
- local Data Center / MelonLoader installation

> **Note:** This mod was built on Linux using Proton-GE 10-34. The `references/` directory contains the required game and MelonLoader DLLs. When building on a different system, adjust the `<HintPath>` entries in the `.csproj` to point to your local MelonLoader and game interop assemblies.

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
├── src/                       # Source code
│   ├── HexViewerMod.cs        # MelonLoader entry point
│   ├── HexViewerFeature.cs    # HUD overlay (Jade-style)
│   ├── HexTargetResolver.cs   # Crosshair raycast targeting
│   ├── HeldCableKindResolver.cs# Held item hex and port resolution
│   ├── GameObjectColorHex.cs  # Hex extraction from CableSpinner and Rack
│   ├── GameObjectKindResolver.cs# Port type and rack variant detection
│   ├── CablePortKindUtil.cs   # RJ / SFP / QSFP classification
│   └── HexColorUtil.cs        # Color ↔ hex conversion utilities
├── references/                 # Game & MelonLoader interop DLLs
├── gregMod.HexViewer.csproj    # Project file
├── LICENSE                     # Apache 2.0
└── README.md
```

## Credits

| Role | Contributor |
|------|-------------|
| **Codebase** | [mleem97](https://github.com/mleem97) ([TeamGreg Modding](https://github.com/teamGregModding)) |
| **Community Testing** | Noootry, TheSlickers, Jarvis, Kirei, TeamWaseku |

## License

This project is licensed under the **Apache License 2.0**. See [`LICENSE`](./LICENSE).

## 🚀 Join the gregFramework Team!

Building the ultimate modding framework for Data Center is a massive undertaking. gregFramework is currently maintained by a passionate core team of three, and we are looking for fellow creators to help us scale this mission!

**Your place in the team:** We won't throw you into the deep end. Depending on your individual strengths and skills, we will match you with the right areas of the project so you can contribute exactly where you have the most fun.

**🌍 Language Requirement:** A solid grasp of written English is required (without relying on machine translation). Being comfortable speaking English in voice chats is a huge plus, but we completely respect those who prefer to stick to text!

**We are looking for motivated volunteers to join our crew across several roles:**

- 💻 **Code Wizards** (C#, Rust, Lua, TS, GO) — Build and expand the core framework and mod packages
- 🎨 **Asset Creators** (3D Models, hardware assets) — Bring the framework to life visually
- 📚 **Technical Writers** — Craft wiki entries, maintain documentation, and write user guides
- 🎮 **Alpha Testers** — Hunt down bugs, stress-test the framework, and provide critical feedback
- ⚙️ **System Guardians** — Maintain our Linux servers, Docker containers, and infrastructure
- 🤝 **Community Managers** — Foster our Discord community, gather feedback, and keep the energy high

Interested in joining the project? Everyone is absolutely welcome! Send us an email at **apply@gregframework.eu**, shoot a quick DM, or drop a message on [Discord](https://discord.gg/greg).

---

**gregFramework — powered by the community.**
