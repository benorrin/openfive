#!/bin/sh
# Builds the Linux downloads: a portable archive, a Debian/Ubuntu package and an RPM for
# openSUSE and Fedora. Each holds one self-contained executable (no .NET needed on the target)
# with SDL3 next to it.
#
#   packaging/linux/build-release.sh [RID] [OUTPUT-DIR]     (RID: linux-x64 or linux-arm64)
#
# Needs the .NET 8 SDK, dpkg-deb and rpmbuild. SDL3 is built with build-sdl3.sh unless SDL3_LIB names an
# already built libSDL3.so.0.
set -eu
top=$(cd "$(dirname "$0")/../.." && pwd)
rid=${1:-linux-x64}
out=${2:-$top/dist}
case "$rid" in
    linux-x64) arch=amd64 rpmarch=x86_64 ;;
    linux-arm64) arch=arm64 rpmarch=aarch64 ;;
    *) echo "unsupported RID $rid" >&2; exit 1 ;;
esac
version=$(sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p' "$top/Directory.Build.props")
appid=io.github.benorrin.openfive
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
mkdir -p "$out"

sdl=${SDL3_LIB:-}
if [ -z "$sdl" ]; then
    "$top/packaging/linux/build-sdl3.sh" "$work/sdl"
    sdl=$work/sdl/libSDL3.so.0
fi

app=$work/app
dotnet publish "$top/src/OpenFive/OpenFive.csproj" -c Release -r "$rid" --self-contained \
    -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -o "$app"
rm -f "$app"/*.xml
mv "$app/OpenFive" "$app/openfive"
cp "$sdl" "$app/libSDL3.so.0"

# Portable archive: unpack anywhere and run ./openfive.
name=openfive-$version-$rid
mkdir "$work/$name"
cp -a "$app/." "$work/$name/"
cp "$top/LICENSE" "$top/README.md" "$top/packaging/linux/$appid.desktop" "$top/packaging/linux/$appid.svg" \
    "$work/$name/"
tar -C "$work" -czf "$out/$name.tar.gz" "$name"
echo "$out/$name.tar.gz"

# Debian package: the program in /usr/lib/openfive, a menu entry and an icon.
deb=$work/deb
install -d "$deb/DEBIAN" "$deb/usr/lib/openfive" "$deb/usr/bin"
cp -a "$app/." "$deb/usr/lib/openfive/"
printf '#!/bin/sh\nexec /usr/lib/openfive/openfive "$@"\n' > "$deb/usr/bin/openfive"
chmod 0755 "$deb/usr/bin/openfive"
install -Dm0644 "$top/packaging/linux/$appid.desktop" "$deb/usr/share/applications/$appid.desktop"
install -Dm0644 "$top/packaging/linux/$appid.metainfo.xml" "$deb/usr/share/metainfo/$appid.metainfo.xml"
install -Dm0644 "$top/packaging/linux/$appid.svg" "$deb/usr/share/icons/hicolor/scalable/apps/$appid.svg"
install -Dm0644 "$top/LICENSE" "$deb/usr/share/doc/openfive/copyright"
size=$(du -sk "$deb/usr" | cut -f1)
cat > "$deb/DEBIAN/control" <<EOF
Package: openfive
Version: $version
Architecture: $arch
Maintainer: Ben <ben@orrin.uk>
Installed-Size: $size
Depends: libc6 (>= 2.35), libgcc-s1, libstdc++6, zlib1g
Section: games
Priority: optional
Homepage: https://github.com/benorrin/openfive
Description: Play The Famous Five: Treasure Island from your own CD
 OpenFive is an open-source reimplementation of the engine of the 1999 adventure
 game The Famous Five: Treasure Island (5 Freunde auf Schatzsuche). It plays the
 original adventure natively, in a window or fullscreen.
 .
 OpenFive contains no code, graphics, text, speech or music from the original
 game. You need your own copy of the game CD (English or German), or a disc image
 of it.
EOF
dpkg-deb --root-owner-group --build "$deb" "$out/openfive_${version}_$arch.deb"

# RPM for openSUSE and Fedora: the same files as the Debian package.
rpm=$work/rpm
mkdir -p "$rpm/SPECS"
cat > "$rpm/SPECS/openfive.spec" <<EOF
Name:           openfive
Version:        $version
Release:        1
Summary:        Play The Famous Five: Treasure Island from your own CD
License:        GPL-2.0-only
URL:            https://github.com/benorrin/openfive
# The bundled SDL3 is private: it must not stand in for the system's.
AutoProv:       no
# The executable has the .NET runtime appended to it: stripping it would break it.
%global __os_install_post %{nil}
%global debug_package %{nil}

%description
OpenFive is an open-source reimplementation of the engine of the 1999 adventure
game The Famous Five: Treasure Island (5 Freunde auf Schatzsuche). It plays the
original adventure natively, in a window or fullscreen.

OpenFive contains no code, graphics, text, speech or music from the original
game. You need your own copy of the game CD (English or German), or a disc image
of it.

%install
cp -a $deb/usr %{buildroot}/

%files
/usr/bin/openfive
/usr/lib/openfive
/usr/share/applications/$appid.desktop
/usr/share/metainfo/$appid.metainfo.xml
/usr/share/icons/hicolor/scalable/apps/$appid.svg
/usr/share/doc/openfive
EOF
rpmbuild -bb "$rpm/SPECS/openfive.spec" --target "$rpmarch" \
    --define "_topdir $rpm" --define "_rpmdir $rpm/RPMS" --define "_build_id_links none"
cp "$rpm/RPMS/$rpmarch/openfive-$version-1.$rpmarch.rpm" "$out/"
echo "$out/openfive-$version-1.$rpmarch.rpm"
