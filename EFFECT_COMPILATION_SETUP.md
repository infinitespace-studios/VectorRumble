# Effect Compilation Setup for CoPilot Environment

This document explains how to set up shader effect compilation for MonoGame projects in the CoPilot environment or other Linux-based development environments.

## Quick Start

Run the setup script with auto-detection:

```bash
./setup-effects-compilation.sh auto
```

This will automatically detect your environment and configure the best mode for effect compilation.

## Available Modes

### CoPilot Mode (Recommended for CoPilot Environment)

This mode sets up Wine for full effect compilation capability in CoPilot and similar environments.

```bash
./setup-effects-compilation.sh copilot
```

**Features:**
- ✅ Full shader compilation support with Wine
- ✅ Can compile and modify .fx shader files
- ✅ Works in CoPilot, containers, and CI environments  
- ✅ Compatible with GitHub CoPilot, Codespaces, and similar tools
- ✅ Builds effects from source with full compatibility
- ✅ Enables effect debugging and modification during development

### Wine Mode (Full Compilation Support)

This mode sets up Wine to enable full shader compilation from source .fx files.

```bash
./setup-effects-compilation.sh wine
```

**Features:**
- ✅ Full shader compilation support
- ✅ Can modify .fx shader files
- ✅ Builds effects from source with full compatibility
- ⚠️ Requires Wine installation and setup
- ⚠️ May not work in all containerized environments

## How It Works

### CoPilot Mode
- Uses the original `Content/Content.mgcb` to compile `.fx` files to `.xnb` files
- Installs and configures Wine for full shader compilation support
- Sets up Wine environment with proper variables for MonoGame
- Compiles `.fx` files to `.xnb` during build with full compatibility
- Enables effect debugging and modification during development

### Wine Mode
- Installs Wine on Ubuntu/Linux systems (same as CoPilot mode)
- Sets up Wine environment with proper variables
- Configures MonoGame's MGFXC to use Wine for DirectX shader compilation  
- Compiles `.fx` files to `.xnb` during build with full compatibility

## Environment Detection

The script automatically detects:
- Container environments (Docker, GitHub Actions) → Uses CoPilot mode with Wine
- CI/CD systems → Uses CoPilot mode with Wine
- GitHub CoPilot and Codespaces → Uses CoPilot mode with Wine
- Regular development environments → Uses Wine mode

## Manual Commands

### Building the Project
After running the setup:

```bash
dotnet build
```

### Restoring Original Configuration
```bash
./setup-effects-compilation.sh restore
```

### Getting Help
```bash
./setup-effects-compilation.sh --help
```

## Troubleshooting

### Build Fails with Effect Compilation Errors
If you see errors related to effect compilation in CoPilot mode:

1. First try building anyway - the game may still work:
   ```bash
   dotnet build
   ```

2. If effects are required, switch to Wine mode:
   ```bash
   ./setup-effects-compilation.sh wine
   dotnet build
   ```

3. Or check Wine setup if using Wine mode:
   ```bash
   wine --version
   echo $MGFXC_WINE_PATH
   ```

### Effect Compilation Not Available
In CoPilot mode, if MonoGame's effect compiler is not available:
- The build may succeed but skip effect compilation
- The game will run without post-processing effects
- Visual quality may be reduced but functionality remains

### Permission Errors
Make sure the setup script is executable:
```bash
chmod +x setup-effects-compilation.sh
```

## Technical Details

The project includes three shader effects for bloom post-processing:

1. **BloomExtract.fx** - Extracts bright areas from the scene
2. **GaussianBlur.fx** - Applies gaussian blur filter  
3. **BloomCombine.fx** - Combines bloom with original scene

These effects are compiled to MonoGame XNB format for the DesktopGL platform.

## Files Created/Modified

- `Content/Content.mgcb.original` - Backup of original content file
- `setup-effects-compilation.sh` - Main setup script
- `setup-copilot-wine.sh` - Wine-only setup script (legacy)

## Requirements

### CoPilot Mode
- .NET 9.0 SDK
- MonoGame 3.8.4+
- MonoGame MGFXC tool (may work without effects if not available)

### Wine Mode  
- .NET 9.0 SDK
- MonoGame 3.8.4+
- Ubuntu 24.04+ (or compatible Linux distribution)
- Wine 9.0+
- Administrator access for Wine installation