#!/bin/sh
# Builds SDL3 from source, to ship with OpenFive's Linux downloads.
#
#   packaging/linux/build-sdl3.sh OUTPUT-DIR        (makes OUTPUT-DIR/libSDL3.so.0)
#
# SDL loads its optional parts (X11, Wayland, PulseAudio, PipeWire, ALSA, ...) at run time, so a
# build made on an old distribution with their -dev packages installed runs on newer ones too.
# Needs cmake, a C compiler and curl. SDL3_VERSION picks the version.
set -eu
version=${SDL3_VERSION:-3.4.18}
out=${1:?usage: $0 OUTPUT-DIR}
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
curl -fsSL "https://github.com/libsdl-org/SDL/releases/download/release-$version/SDL3-$version.tar.gz" |
    tar -xz -C "$work"
cmake -S "$work/SDL3-$version" -B "$work/build" -DCMAKE_BUILD_TYPE=Release \
    -DSDL_SHARED=ON -DSDL_STATIC=OFF -DSDL_TESTS=OFF -DSDL_EXAMPLES=OFF
cmake --build "$work/build" -j"$(nproc)"
mkdir -p "$out"
cp -L "$work/build/libSDL3.so.0" "$out/libSDL3.so.0"
strip "$out/libSDL3.so.0"
