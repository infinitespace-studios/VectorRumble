#!/bin/bash

# Setup script for MonoGame effect compilation in CoPilot environment
# This script provides two modes:
# 1. CoPilot mode: Simple setup without Wine (may skip effects if compilation fails)
# 2. Wine mode: Sets up Wine for full effect compilation

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONTENT_DIR="$SCRIPT_DIR/Content"

show_help() {
    echo "Usage: $0 [mode]"
    echo ""
    echo "Modes:"
    echo "  copilot    - Setup for CoPilot environment (graceful effect compilation)"
    echo "  wine       - Setup Wine for full effect compilation"
    echo "  auto       - Auto-detect environment and choose best mode"
    echo "  restore    - Restore original Content.mgcb file"
    echo ""
    echo "The CoPilot mode attempts to build effects without Wine and provides"
    echo "graceful fallbacks if shader compilation is not available."
}

backup_content_file() {
    if [[ ! -f "$CONTENT_DIR/Content.mgcb.original" ]]; then
        echo "Creating backup of original Content.mgcb..."
        cp "$CONTENT_DIR/Content.mgcb" "$CONTENT_DIR/Content.mgcb.original"
    fi
}

setup_copilot_mode() {
    echo "Setting up CoPilot mode (graceful effect compilation)..."
    
    backup_content_file
    
    # Use the original content file which compiles from .fx sources
    if [[ -f "$CONTENT_DIR/Content.mgcb.original" ]]; then
        cp "$CONTENT_DIR/Content.mgcb.original" "$CONTENT_DIR/Content.mgcb"
    fi
    
    echo "✓ CoPilot mode setup complete!"
    echo ""
    echo "This mode attempts to compile shader effects from .fx files."
    echo "If effect compilation fails during build, the game will still run"
    echo "but without post-processing effects. To build:"
    echo "  dotnet build"
    echo ""
    echo "If you encounter effect compilation errors, switch to Wine mode:"
    echo "  ./setup-effects-compilation.sh wine"
}

setup_wine_mode() {
    echo "Setting up Wine mode for full effect compilation..."
    
    backup_content_file
    
    # Restore original content file (if it was changed)
    if [[ -f "$CONTENT_DIR/Content.mgcb.original" ]]; then
        cp "$CONTENT_DIR/Content.mgcb.original" "$CONTENT_DIR/Content.mgcb"
    fi
    
    # Check if we're on Ubuntu and get version
    if [[ -f /etc/os-release ]]; then
        source /etc/os-release
        echo "Detected OS: $ID $VERSION_ID"
        
        if [[ "$ID" != "ubuntu" ]]; then
            echo "Warning: This script is designed for Ubuntu. Your OS may not be supported."
        fi
    else
        echo "Cannot detect OS version. Assuming Ubuntu 24.04."
        VERSION_ID="24.04"
    fi

    # Install Wine if not present
    if ! command -v wine &> /dev/null; then
        echo "Installing Wine..."
        sudo apt update
        sudo apt install -y wine p7zip-full curl
    else
        echo "Wine is already installed: $(wine --version)"
    fi

    # Setup Wine environment variables
    export WINEPREFIX="$HOME/.winemonogame"
    export WINEARCH=win64
    export WINEDEBUG=-all
    export MVK_CONFIG_LOG_LEVEL=0
    export WINEDLLOVERRIDES="d3dcompiler_47=d,explorer.exe=e,services.exe=f"

    echo "Wine prefix: $WINEPREFIX"

    # Create Wine prefix directory
    mkdir -p "$WINEPREFIX"

    # Initialize Wine prefix (headless)
    echo "Initializing Wine prefix..."
    DISPLAY="" wine wineboot --init 2>/dev/null || echo "Wine boot completed (warnings ignored)"

    echo "✓ Wine mode setup complete!"
    echo ""
    echo "To use Wine mode, export the following environment variables:"
    echo "export WINEPREFIX=\"$WINEPREFIX\""
    echo "export WINEARCH=win64"
    echo "export WINEDEBUG=-all"
    echo "export MVK_CONFIG_LOG_LEVEL=0"
    echo "export WINEDLLOVERRIDES=\"d3dcompiler_47=d,explorer.exe=e,services.exe=f\""
    echo "export MGFXC_WINE_PATH=\"$WINEPREFIX\""
    echo ""
    echo "Then build with: dotnet build"
}

auto_detect_mode() {
    echo "Auto-detecting best mode for current environment..."
    
    # Check if we're in a container or restricted environment
    if [[ -f /.dockerenv ]] || [[ "$USER" == "runner" ]] || [[ -n "$GITHUB_ACTIONS" ]] || [[ -n "$COPILOT" ]]; then
        echo "Detected container/CI environment - using CoPilot mode"
        setup_copilot_mode
    else
        echo "Detected regular environment - using Wine mode"
        setup_wine_mode
    fi
}

restore_original() {
    echo "Restoring original Content.mgcb file..."
    
    if [[ -f "$CONTENT_DIR/Content.mgcb.original" ]]; then
        cp "$CONTENT_DIR/Content.mgcb.original" "$CONTENT_DIR/Content.mgcb"
        echo "✓ Original Content.mgcb restored"
    else
        echo "No backup found - Content.mgcb may already be in original state"
    fi
}

# Main script logic
case "${1:-auto}" in
    "copilot")
        setup_copilot_mode
        ;;
    "wine")
        setup_wine_mode
        ;;
    "auto")
        auto_detect_mode
        ;;
    "restore")
        restore_original
        ;;
    "-h"|"--help"|"help")
        show_help
        ;;
    *)
        echo "Unknown mode: $1"
        echo ""
        show_help
        exit 1
        ;;
esac