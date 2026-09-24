# AGENTS.md — Notes for AI agents (gregMod.HexViewer)

Repo: https://github.com/mleem97/gregMod.HexViewer · License: Apache-2.0 · Version: see `VERSION` (1.0.8).

MelonMod for Data Center. Shows HEX color values of cables and game objects
in the scene.

## Duties

1. **Read first:** `README.md`, `docs/INDEX.md`, `docs/ARCHITECTURE.md` — only then make changes.
2. **Do not commit secrets** (keys, tokens, `.env`). Use keys only via environment variables.
3. **Preserve history:** no `push --force`, no history rewrite without instruction.
4. **Verify changes:** before reporting done, build the mod (`dotnet build gregMod.HexViewer.csproj -c Release` or `./build.sh HexViewer` from `ModRepositories/`).
5. **Keep docs in sync:** for new features update `README.md` + `docs/` + `CHANGELOG.md` (Unreleased).
6. **Conventions:** Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:` …), one logical change per commit.
7. **When unsure:** stop and ask instead of guessing — especially for deletes, migrations, CI.

## Build and references

- Target: `net6.0`, x64. Game: Data Center (`MelonGame("Waseku", "Data Center")`).
- `references/` holds absolute symlinks into the Steam Data Center install.
  Never commit `references/*.dll`, `bin/`, or `obj/`.
- After a fresh clone, run `../tools/sync-melon-assemblies.sh`.
- Deploy only with `./build.sh HexViewer --deploy`.

## Hard rules

- Read-only inspection — never mutate scene objects, materials, or cable state.
- **Never** touch gregCore types outside a soft-probe/JIT-split bridge — the
  mod must load without `gregCore.dll`.
- Defensive `try/catch` in every per-frame path; no per-frame reflection.

## Layout

- `src/HexViewerMod.cs` — MelonMod entry. `src/HexViewerFeature.cs` — feature logic.
- `src/HexTargetResolver.cs`, `src/GameObjectKindResolver.cs`,
  `src/HeldCableKindResolver.cs`, `src/CablePortKindUtil.cs` — target/kind resolution.
- `src/HexColorUtil.cs`, `src/GameObjectColorHex.cs`,
  `src/CableColorCollector.cs` — color collection and formatting.
- `docs/` — `INDEX.md`, `ARCHITECTURE.md`, `SOURCE_LAYOUT.md`, `CHANGELOG.md`.
