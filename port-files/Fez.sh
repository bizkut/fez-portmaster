#!/bin/bash
# FEZ - Portmaster Launch Script

XDG_DATA_HOME=${XDG_DATA_HOME:-$HOME/.local/share}

if [ -d "/opt/system/Tools/PortMaster/" ]; then
  controlfolder="/opt/system/Tools/PortMaster"
elif [ -d "/opt/tools/PortMaster/" ]; then
  controlfolder="/opt/tools/PortMaster"
elif [ -d "$XDG_DATA_HOME/PortMaster/" ]; then
  controlfolder="$XDG_DATA_HOME/PortMaster"
else
  controlfolder="/roms/ports/PortMaster"
fi

source $controlfolder/control.txt
[ -f "${controlfolder}/mod_${CFW_NAME}.txt" ] && source "${controlfolder}/mod_${CFW_NAME}.txt"

get_controls

GAMEDIR=/$directory/ports/fez

cd $GAMEDIR

# Log output
> "$GAMEDIR/log.txt" && exec > >(tee "$GAMEDIR/log.txt") 2>&1

# Config directory for saves
mkdir -p "$GAMEDIR/conf"
export XDG_DATA_HOME="$GAMEDIR/conf"

# GL4ES settings for OpenGL ES 2.0 translation
export LIBGL_ES=2
export LIBGL_GL=21
export LIBGL_FB=4
export LIBGL_NPOT=2
export LIBGL_SILENTSTUB=1

# Native libraries
export LD_LIBRARY_PATH="$GAMEDIR/libs.aarch64:$LD_LIBRARY_PATH"

# SDL controller config
export SDL_GAMECONTROLLERCONFIG="$sdl_controllerconfig"

# Mono path for assemblies
export MONO_PATH="$GAMEDIR"
export MONO_IOMAP=all

# Use GL4ES
if [ -f "${controlfolder}/libgl_${CFW_NAME}.txt" ]; then
  source "${controlfolder}/libgl_${CFW_NAME}.txt"
else
  source "${controlfolder}/libgl_default.txt"
fi

# Start gptokeyb for controller mapping
$GPTOKEYB "mono" -c "$GAMEDIR/fez.gptk" &

# Platform helper
pm_platform_helper mono

# Launch FEZ with Mono runtime
mono FEZ.exe

# Cleanup
pm_finish
