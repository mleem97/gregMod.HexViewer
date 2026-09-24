# Quickstart — gregMod.HexViewer

> gregMod.HexViewer** adds a hardware inspection overlay to **Data Center**. Point your crosshair at a cable reel, rack, or hold a cable to see its hex color code

Repo: [https://github.com/mleem97/gregMod.HexViewer](https://github.com/mleem97/gregMod.HexViewer) · Version: `0.1.0` · License: Apache-2.0.

## 1. Clone

```bash
git clone https://github.com/mleem97/gregMod.HexViewer.git
cd gregMod.HexViewer
```

## 2. Build / Start

Depending on your tech stack, choose **one** path:

```bash
# .NET
dotnet build -c Release
dotnet run --project src/

# Node / pnpm
pnpm install
pnpm build
pnpm start

# Python
python -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
python -m <modul>
```

## 3. Test

```bash
dotnet test            # .NET
pnpm test              # Node
pytest                 # Python
```

Details can be found in [README.md](README.md) and [docs/INDEX.md](docs/INDEX.md).
If you run into problems: file an issue ([Issues](https://github.com/mleem97/gregMod.HexViewer/issues)) or read [CONTRIBUTING.md](CONTRIBUTING.md).
