#!/bin/sh
# Builds the macOS download: a disk image with OpenFive.app (self-contained, no .NET needed, with
# SDL3 inside). The app is signed ad hoc only, so macOS asks before opening it the first time.
#
#   packaging/macos/build-release.sh [RID] [OUTPUT-DIR]     (RID: osx-arm64 or osx-x64)
#
# Runs on macOS. Needs the .NET 8 SDK and network access (for the runtime and the official SDL3 build).
set -eu
top=$(cd "$(dirname "$0")/../.." && pwd)
rid=${1:-osx-arm64}
out=${2:-$top/dist}
sdl_version=${SDL3_VERSION:-3.4.18}
case "$rid" in
    osx-arm64) arch=arm64 ;;
    osx-x64) arch=x64 ;;
    *) echo "unsupported RID $rid" >&2; exit 1 ;;
esac
version=$(sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p' "$top/Directory.Build.props")
work=$(mktemp -d)
trap 'hdiutil detach -quiet "$work/sdlmount" 2>/dev/null || true; rm -rf "$work"' EXIT
mkdir -p "$out"

app=$work/image/OpenFive.app
mkdir -p "$app/Contents/MacOS" "$app/Contents/Resources"
dotnet publish "$top/src/OpenFive/OpenFive.csproj" -c Release -r "$rid" --self-contained \
    -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -o "$work/publish"
rm -f "$work/publish"/*.xml
cp -a "$work/publish/." "$app/Contents/MacOS/"

# SDL3 from the official release (a universal binary inside SDL3.xcframework).
curl -fsSL -o "$work/sdl.dmg" \
    "https://github.com/libsdl-org/SDL/releases/download/release-$sdl_version/SDL3-$sdl_version.dmg"
hdiutil attach -quiet -nobrowse -readonly -mountpoint "$work/sdlmount" "$work/sdl.dmg"
sdl=$(find "$work/sdlmount" -path '*macos*/SDL3.framework/*' -name SDL3 -type f | head -n 1)
cp "$sdl" "$app/Contents/MacOS/libSDL3.0.dylib"
hdiutil detach -quiet "$work/sdlmount"

# Icon.
iconset=$work/OpenFive.iconset
mkdir "$iconset"
for size in 16 32 128 256 512; do
    sips -z $size $size "$top/packaging/macos/openfive-1024.png" --out "$iconset/icon_${size}x${size}.png" >/dev/null
    double=$((size * 2))
    sips -z $double $double "$top/packaging/macos/openfive-1024.png" --out "$iconset/icon_${size}x${size}@2x.png" >/dev/null
done
iconutil -c icns "$iconset" -o "$app/Contents/Resources/OpenFive.icns"

cat > "$app/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key><string>OpenFive</string>
    <key>CFBundleDisplayName</key><string>OpenFive</string>
    <key>CFBundleIdentifier</key><string>io.github.benorrin.openfive</string>
    <key>CFBundleVersion</key><string>$version</string>
    <key>CFBundleShortVersionString</key><string>$version</string>
    <key>CFBundlePackageType</key><string>APPL</string>
    <key>CFBundleExecutable</key><string>OpenFive</string>
    <key>CFBundleIconFile</key><string>OpenFive</string>
    <key>LSApplicationCategoryType</key><string>public.app-category.adventure-games</string>
    <key>LSMinimumSystemVersion</key><string>12.0</string>
    <key>NSHighResolutionCapable</key><true/>
</dict>
</plist>
EOF

codesign --force --sign - "$app/Contents/MacOS/libSDL3.0.dylib"
codesign --force --sign - "$app"

cp "$top/LICENSE" "$work/image/LICENSE.txt"
ln -s /Applications "$work/image/Applications"
hdiutil create -quiet -volname OpenFive -srcfolder "$work/image" -format UDZO -ov \
    "$out/OpenFive-$version-macos-$arch.dmg"
echo "$out/OpenFive-$version-macos-$arch.dmg"
