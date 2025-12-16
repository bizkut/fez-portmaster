#!/bin/bash
# Local test script for FEZ Portmaster port

export GAMEDIR="$(pwd)/port-files/fez"
export LIBGL_ES=2
export LIBGL_GL=21
export LIBGL_FB=4
export LIBGL_NPOT=2
export LIBGL_SILENTSTUB=1
export LD_LIBRARY_PATH="$GAMEDIR/libs.aarch64:$LD_LIBRARY_PATH"
export MONO_PATH="$GAMEDIR"
export MONO_IOMAP=all

echo "--- Environment ---"
echo "GAMEDIR: $GAMEDIR"
echo "LD_LIBRARY_PATH: $LD_LIBRARY_PATH"

echo "--- Libraries ---"
ls "$GAMEDIR/libs.aarch64/"

echo "--- Testing FNA ---"
cd "$GAMEDIR"

if [ -f "FEZ.exe" ]; then
    mono FEZ.exe
else
    echo "FEZ.exe not found. Testing FNA initialization..."
    cat > CheckFNA.cs << 'TESTEOF'
using System;
using Microsoft.Xna.Framework;

class Test {
    static void Main() {
        Console.WriteLine("Testing FNA assembly load...");
        Console.WriteLine("FNA: " + typeof(Vector2).Assembly.FullName);
        Console.WriteLine("SUCCESS: FNA loaded correctly!");
    }
}
TESTEOF
    mcs CheckFNA.cs -r:FNA.dll -out:CheckFNA.exe
    mono CheckFNA.exe
    rm -f CheckFNA.cs CheckFNA.exe
fi
