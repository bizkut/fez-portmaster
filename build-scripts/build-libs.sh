#!/bin/bash
# FEZ Portmaster Build Script
# Builds native libraries for aarch64

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
BUILD_DIR="$PROJECT_ROOT/build"
OUTPUT_DIR="$PROJECT_ROOT/port-files/fez/libs.aarch64"

echo "=== FEZ Portmaster Build Script ==="
echo "Build directory: $BUILD_DIR"
echo "Output directory: $OUTPUT_DIR"

mkdir -p "$BUILD_DIR" "$OUTPUT_DIR"

# Build gl4es
build_gl4es() {
    echo ""
    echo "=== Building gl4es ==="
    cd "$BUILD_DIR"
    if [ ! -d "gl4es" ]; then
        git clone --depth 1 https://github.com/ptitSeb/gl4es.git
    fi
    cd gl4es
    mkdir -p build && cd build
    cmake .. -DNOX11=ON -DGLX_STUBS=ON -DEGL_WRAPPER=ON -DGBM=ON
    make -j$(nproc)
    cp ../lib/libGL.so.1 ../lib/libEGL.so.1 "$OUTPUT_DIR/"
    echo "gl4es built successfully"
}

# Build FNA3D
build_fna3d() {
    echo ""
    echo "=== Building FNA3D ==="
    cd "$BUILD_DIR"
    if [ ! -d "FNA3D" ]; then
        git clone --depth 1 --branch 24.09 https://github.com/FNA-XNA/FNA3D.git
    fi
    cd FNA3D
    git submodule update --init --recursive
    mkdir -p build && cd build
    cmake ..
    make -j$(nproc)
    cp libFNA3D.so* "$OUTPUT_DIR/"
    echo "FNA3D built successfully"
}

# Build FAudio
build_faudio() {
    echo ""
    echo "=== Building FAudio ==="
    cd "$BUILD_DIR"
    if [ ! -d "FAudio" ]; then
        git clone --depth 1 --branch 24.09 https://github.com/FNA-XNA/FAudio.git
    fi
    cd FAudio
    mkdir -p build && cd build
    cmake ..
    make -j$(nproc)
    cp libFAudio.so* "$OUTPUT_DIR/"
    echo "FAudio built successfully"
}

# Copy prebuilt DLLs
copy_dlls() {
    echo ""
    echo "=== Copying prebuilt DLLs ==="
    cp "$PROJECT_ROOT/src/lib-prebuilt/"*.dll "$PROJECT_ROOT/port-files/fez/"
    cp "$PROJECT_ROOT/src/Libs/FezEngine/lib/ContentSerialization.dll" "$PROJECT_ROOT/port-files/fez/"
    echo "DLLs copied successfully"
}

# Main
echo ""
echo "Building for: $(uname -m)"

build_gl4es
build_fna3d
build_faudio
copy_dlls

echo ""
echo "=== Build Complete ==="
echo "Port files ready in: $PROJECT_ROOT/port-files/"
echo ""
echo "Next steps:"
echo "1. Copy FEZ.exe and Content/ from GOG to port-files/fez/"
echo "2. Create zip: cd $PROJECT_ROOT && zip -r fez.zip port-files/"
