# FEZ Portmaster Port

A port of [FEZ](https://fezgame.com/) for [PortMaster](https://portmaster.games/) on ARM64 handheld devices.

## About FEZ

FEZ is a puzzle platformer by Polytron Corporation where you play as Gomez, a 2D creature who discovers the existence of a third dimension. Rotate the world to solve puzzles and collect golden cubes!

## Requirements

- **GOG version of FEZ** - Purchase from [GOG](https://www.gog.com/game/fez)
- PortMaster-compatible device (aarch64)
- Mono runtime (auto-installed by PortMaster)

## Quick Install

1. Download the latest release (`fez.zip`)
2. Extract to your `ports/` folder
3. Copy from your GOG FEZ installation to `ports/fez/`:
   - `FEZ.exe`
   - `Content/` folder
4. Launch via PortMaster

## Building from Source

### Prerequisites

```bash
sudo apt install mono-complete libsdl2-dev cmake build-essential git
```

### Build Native Libraries

```bash
./build-scripts/build-libs.sh
```

This compiles:
- **gl4es** - OpenGL → OpenGL ES 2.0 translation
- **FNA3D** - Graphics library
- **FAudio** - Audio library

### Create Port Package

```bash
# Add GOG files
cp /path/to/gog/FEZ.exe port-files/fez/
cp -r /path/to/gog/Content port-files/fez/

# Create zip
zip -r fez.zip port-files/
```

## Repository Structure

```
├── src/                    # Source code (decompiled)
│   ├── Components/         # Game components
│   ├── Services/           # Game services
│   ├── Structure/          # Data structures
│   ├── Tools/              # Utilities
│   ├── Libs/               # Dependencies (FezEngine, Common, etc.)
│   └── lib-prebuilt/       # Pre-built DLLs
├── port-files/             # Ready-to-use Portmaster port
│   ├── port.json
│   ├── README.md
│   ├── Fez.sh
│   └── fez/                # Game files
├── build-scripts/          # Build automation
└── README.md               # This file
```

## Technical Details

| Component | Details |
|-----------|---------|
| Runtime | Mono 6.12.0.122 |
| Graphics | GL4ES (OpenGL ES 2.0) |
| Audio | FAudio |
| Architecture | aarch64 |

## Credits

- **Polytron Corporation** - Original game
- **Renaud Bédard** - FNA port
- **FNA Team** - Cross-platform framework
- **ptitSeb** - gl4es

## License

Game assets require purchase from GOG. Native libraries are under their respective licenses (see `port-files/fez/licenses/`).
