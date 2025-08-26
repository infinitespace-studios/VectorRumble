# VectorRumble - MonoGame Vector Graphics Game

VectorRumble is a C# MonoGame-based port of the XNA VectorRumble sample, enhanced as a complete publishable cross-platform game. It's a multiplayer arcade-style space combat game with vector graphics, bloom effects, and multi-language support.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites
- Install .NET 9.0 SDK: `curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 9.0.101`
- Add to PATH: `export PATH="$HOME/.dotnet:$PATH"`
- For MonoGame content building on Linux: Install wine and i386 architecture support
- **CRITICAL**: Content building requires specific wine setup - build may fail without proper MonoGame wine configuration

### Initial Setup Commands
```bash
# Install .NET 9.0 SDK (CRITICAL - must be done first)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 9.0.101
export PATH="$HOME/.dotnet:$PATH"    # MUST set PATH or commands will fail

# Verify .NET 9.0 is available
dotnet --version                     # Should show 9.0.101 or higher

# Restore project dependencies
dotnet restore                       # Takes ~1-2 seconds

# Restore MonoGame dotnet tools
dotnet tool restore                  # Takes ~1 second - NEVER CANCEL
```

### Build Commands
```bash
# Build Core library only (always works)
dotnet build VectorRumble.Core/VectorRumble.Core.csproj -c Release    # Takes ~1-2 seconds

# Build Desktop executable (may fail without proper wine setup)
dotnet build VectorRumble.Desktop/VectorRumble.Desktop.csproj -c Release    # Takes ~6-10 seconds or fails
```

**CRITICAL BUILD TIMING**: 
- dotnet clean: ~1 second - NEVER CANCEL
- dotnet restore: ~1-2 seconds - NEVER CANCEL  
- dotnet tool restore: ~1 second - NEVER CANCEL
- Core build: 1-2 seconds - NEVER CANCEL
- Desktop build: 6-10 seconds when working, may fail due to MonoGame content pipeline - NEVER CANCEL
- Total clean build process: ~4-6 seconds for working builds

### MonoGame Content Pipeline Issues
**IMPORTANT**: This project has known MonoGame content building challenges on Linux:
- The project requires wine for building shader effects (.fx files)
- Content building may fail with "exited with code 3" errors
- Pre-built content (.xnb files) exist in the repository in `Content/` folders
- The GitHub Actions use MonoGame's official wine setup actions from https://github.com/MonoGame/monogame-actions
- Currently using `infinitespace-studios/monogame-actions/install-wine@v1.0` (fork of official actions)
- **Workaround**: Pre-built content is committed to the repository to avoid build failures

**Content File Locations**:
- `Content/Effects/*.xnb` - Pre-built shader effects (Bloom, GaussianBlur)
- `Content/bin/DesktopGL/Content/` - Pre-built game assets (fonts, textures, audio)
- When building manually, copy these to `bin/Release/net9.0/Content/` if needed

### Running the Game
```bash
# Basic run (may fail due to audio/graphics on headless systems)
dotnet run --project VectorRumble.Desktop/VectorRumble.Desktop.csproj -c Release

# For testing on headless systems, disable audio:
# Modify VectorRumble.Desktop.csproj and VectorRumble.Core.csproj
# Change: <DefineConstants>$(DefineConstants);AUDIO;SHADER_EFFECTS</DefineConstants>
# To: <DefineConstants>$(DefineConstants);SHADER_EFFECTS</DefineConstants>
```

**Runtime Requirements**:
- Graphics display (fails on headless systems without virtual display)
- Audio hardware (or disable AUDIO define for testing)
- The game will show an interactive vector graphics space combat interface when running properly

### Testing and Validation
**NO TESTS EXIST** - This project has no unit test infrastructure.

**Manual Validation Scenarios**:
1. **Build Validation**: Both Core and Desktop projects build without errors
2. **Content Validation**: Verify pre-built content exists in `Content/bin/` and `Content/Effects/`
3. **Runtime Validation**: Game starts without crashes (requires display/audio)
4. **Game Flow Test**: If running with display, main menu should appear with options for single/multiplayer

### Project Structure
```
VectorRumble.sln                 # Main solution file
├── VectorRumble.Core/          # Core game library (.NET 9.0)
│   ├── AudioManager.cs        # Audio system management
│   ├── BloomPostprocess/      # Visual bloom effects
│   ├── Rendering/             # Vector graphics rendering
│   ├── ScreenManager/         # Game screen management  
│   ├── Screens/               # Menu and game screens
│   ├── Simulation/            # Game logic and physics
│   └── Strings.*.resx        # Localization resources (EN/DE/ES/FR/ZH-CN)
├── VectorRumble.Desktop/      # Desktop executable (.NET 9.0)
│   └── Program.cs            # Entry point
├── Content/                   # MonoGame content pipeline
│   ├── Content.mgcb          # Content project file
│   ├── Effects/              # Shader effects (*.fx) + pre-built XNB
│   ├── Fonts/                # Sprite fonts + pre-built XNB  
│   ├── Textures/             # Image assets + pre-built XNB
│   ├── Audio/                # Sound assets
│   ├── Ships/                # Ship definition XML files
│   └── Arenas/               # Arena definition XML files
└── .github/workflows/build.yml  # CI build configuration
```

### Key Development Areas
- **Core Gameplay**: `VectorRumble.Core/Simulation/` contains ships, projectiles, powerups
- **Rendering**: `VectorRumble.Core/Rendering/LineBatch.cs` handles vector line drawing
- **UI/Screens**: `VectorRumble.Core/Screens/` contains menus and game screens
- **Content**: All game assets are in `Content/` with MonoGame content pipeline

### Common Development Tasks

**Adding New Content**:
1. Add files to appropriate `Content/` subdirectory
2. Update `Content/Content.mgcb` file with new content references
3. **WARNING**: Content building may fail - consider adding pre-built XNB files

**Localization**:
- Add strings to `VectorRumble.Core/Strings.resx` (English base)
- Create `Strings.{culture}.resx` for new languages
- Supported: English, German (de), Spanish (es), French (fr), Chinese Simplified (zh-CN)

**Ship/Arena Customization**:
- Ships: Add XML files to `Content/Ships/`
- Arenas: Add XML files to `Content/Arenas/`
- See existing files for format examples

### Build Troubleshooting
1. **"dotnet not found"**: Install .NET 9.0 SDK and add to PATH
2. **"No Content References Found"**: Normal warning if content build is disabled
3. **"mgcb exited with code 3"**: MonoGame content build failure - use pre-built content
4. **Audio exceptions**: Disable AUDIO define or ensure audio hardware available
5. **Graphics exceptions**: Requires display - use virtual display (Xvfb) for headless testing

### CI/CD Integration
- GitHub Actions builds for Windows (x64/ARM64), macOS (universal), Linux (x64)
- Uses MonoGame wine setup actions from https://github.com/MonoGame/monogame-actions for content building
- Currently implemented via `infinitespace-studios/monogame-actions/install-wine@v1.0` action
- Publishes to itch.io automatically on tagged releases
- Build artifacts are packaged with MonoPack tool

**MonoGame Actions Integration**:
- The official MonoGame actions repository provides tools for setting up wine on Linux/macOS for content pipeline
- These actions handle the complex wine configuration required for MonoGame's MGFXC shader compiler
- See `.github/workflows/build.yml` for current implementation

**Always validate changes work locally before committing - but understand that full content building may require special CI environment setup.**