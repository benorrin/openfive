#!/bin/sh
# Builds the openSUSE RPM locally from the committed sources (HEAD).
#
#   packaging/opensuse/build-rpm.sh [OUTPUT-DIR]
#
# Needs rpm-build and dotnet-sdk-8.0. The packages end up in OUTPUT-DIR (default: ./dist).
set -eu
top=$(cd "$(dirname "$0")/../.." && pwd)
out=${1:-$top/dist}
version=$(sed -n 's/^Version:[[:space:]]*//p' "$top/packaging/opensuse/openfive.spec")
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
mkdir -p "$work/SOURCES" "$out"
git -C "$top" archive --format=tar.gz --prefix="openfive-$version/" -o "$work/SOURCES/openfive-$version.tar.gz" HEAD
rpmbuild -ba "$top/packaging/opensuse/openfive.spec" \
    --define "_topdir $work" --define "_rpmdir $out" --define "_srcrpmdir $out"
echo "Packages are in $out"
