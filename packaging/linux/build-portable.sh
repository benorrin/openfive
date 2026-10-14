#!/bin/sh
# Builds a portable Linux archive: one self-contained executable (no .NET needed on the target)
# with the desktop entry, icon and docs. The target system needs SDL3 (libSDL3.so.0).
#
#   packaging/linux/build-portable.sh [RID] [OUTPUT-DIR]     (RID: linux-x64 or linux-arm64)
#
# Needs the .NET 8 SDK and network access (to fetch the runtime for RID).
set -eu
top=$(cd "$(dirname "$0")/../.." && pwd)
rid=${1:-linux-x64}
out=${2:-$top/dist}
version=$(sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p' "$top/Directory.Build.props")
name="openfive-$version-$rid"
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
dotnet publish "$top/src/OpenFive/OpenFive.csproj" -c Release -r "$rid" --self-contained \
    -p:PublishSingleFile=true -p:DebugType=none -o "$work/$name"
mv "$work/$name/OpenFive" "$work/$name/openfive"
cp "$top/LICENSE" "$top/README.md" "$work/$name/"
cp "$top/packaging/linux/io.github.benorrin.openfive.desktop" "$top/packaging/linux/io.github.benorrin.openfive.svg" "$work/$name/"
mkdir -p "$out"
tar -C "$work" -czf "$out/$name.tar.gz" "$name"
echo "$out/$name.tar.gz"
