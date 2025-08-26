#!/bin/bash

# Setup script for MonoGame effect compilation in CoPilot environment
# This script installs Wine and configures MGFXC for shader compilation

set -e

echo "Setting up MonoGame effect compilation for CoPilot environment..."

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

# Determine Wine source based on Ubuntu version
if [[ "$VERSION_ID" = "24.04" ]]; then
    WINESOURCE="noble"
else
    WINESOURCE="jammy"
fi

echo "Using Wine source: $WINESOURCE"

# Install prerequisites
echo "Installing prerequisites..."
sudo apt update
sudo apt install -y wine p7zip-full curl

# Verify Wine installation
echo "Verifying Wine installation..."
wine --version

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

# Set MGFXC_WINE_PATH for MonoGame
export MGFXC_WINE_PATH="$WINEPREFIX"

echo "Setup completed successfully!"
echo ""
echo "To use this setup, export the following environment variables:"
echo "export WINEPREFIX=\"$WINEPREFIX\""
echo "export WINEARCH=win64"
echo "export WINEDEBUG=-all"
echo "export MVK_CONFIG_LOG_LEVEL=0"
echo "export WINEDLLOVERRIDES=\"d3dcompiler_47=d,explorer.exe=e,services.exe=f\""
echo "export MGFXC_WINE_PATH=\"$WINEPREFIX\""
echo ""
echo "You can now build MonoGame projects with effect compilation."