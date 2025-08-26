# Effect Compilation Setup for CoPilot Environment

This document explains how to set up shader effect compilation for MonoGame projects in the CoPilot environment or other Linux-based development environments.

## Quick Start

Run the setup script with auto-detection:

```bash
./setup-effects-compilation.sh auto
```

This will automatically detect your environment and configure the best mode for effect compilation.

## Available Modes

### CoPilot Mode (Recommended)

This mode uses pre-compiled shader effects and works in any environment without requiring Wine or additional setup.

```bash
./setup-effects-compilation.sh copilot
```

**Features:**
- ✅ No Wine installation required
- ✅ Fast builds
- ✅ Works in containers and CI environments
- ✅ Compatible with GitHub CoPilot, Codespaces, and similar tools
- ⚠️ Uses pre-compiled effects (cannot modify shaders without recompilation)

### Wine Mode (Full Compilation)

This mode sets up Wine to enable full shader compilation from source .fx files.

```bash
./setup-effects-compilation.sh wine
```

**Features:**
- ✅ Full shader compilation support
- ✅ Can modify .fx shader files
- ✅ Builds effects from source
- ⚠️ Requires Wine installation and setup
- ⚠️ May not work in all containerized environments

## How It Works

### CoPilot Mode
- Modifies `Content/Content.mgcb` to copy pre-compiled `.xnb` files instead of compiling `.fx` files
- Uses existing `BloomCombine.xnb`, `BloomExtract.xnb`, and `GaussianBlur.xnb` files
- No effect compilation step required during build

### Wine Mode
- Installs Wine on Ubuntu/Linux systems
- Sets up Wine environment with proper variables
- Configures MonoGame's MGFXC to use Wine for DirectX shader compilation
- Compiles `.fx` files to `.xnb` during build

## Environment Detection

The script automatically detects:
- Container environments (Docker, GitHub Actions)
- CI/CD systems
- GitHub CoPilot and Codespaces
- Regular development environments

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
If you see errors related to Wine or effect compilation:

1. Switch to CoPilot mode:
   ```bash
   ./setup-effects-compilation.sh copilot
   dotnet build
   ```

2. Or check Wine setup if using Wine mode:
   ```bash
   wine --version
   echo $MGFXC_WINE_PATH
   ```

### Pre-compiled Effects Not Found
Ensure the `.xnb` files exist in `Content/Effects/`:
- `BloomCombine.xnb`
- `BloomExtract.xnb` 
- `GaussianBlur.xnb`

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
- `Content/Content-CoPilot.mgcb` - CoPilot mode content configuration
- `setup-effects-compilation.sh` - Main setup script
- `setup-copilot-wine.sh` - Wine-only setup script (legacy)

## Requirements

### CoPilot Mode
- .NET 9.0 SDK
- MonoGame 3.8.4+

### Wine Mode  
- .NET 9.0 SDK
- MonoGame 3.8.4+
- Ubuntu 24.04+ (or compatible Linux distribution)
- Wine 9.0+
- Administrator access for Wine installation