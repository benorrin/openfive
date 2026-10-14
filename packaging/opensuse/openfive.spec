#
# spec file for package openfive
#
# Copyright (c) 2026 OpenFive contributors
#
# All modifications and additions to the file contributed by third parties
# remain the property of their copyright owners, unless otherwise agreed
# upon. The license for this file, and modifications and additions to the
# file, is the same license as for the pristine package itself (unless the
# license for the pristine package is not an Open Source License, in which
# case the license is the MIT License).
#

%global appid io.github.benorrin.openfive
# .NET assemblies carry no ELF debug information.
%global debug_package %{nil}

Name:           openfive
Version:        1.0.0
Release:        0
Summary:        Engine for The Famous Five: Treasure Island (needs the original CD)
License:        GPL-2.0-only
Group:          Amusements/Games/Other
URL:            https://github.com/benorrin/openfive
Source0:        %{name}-%{version}.tar.gz
BuildRequires:  dotnet-sdk-8.0
BuildRequires:  desktop-file-utils
BuildRequires:  hicolor-icon-theme
Requires:       dotnet-runtime-8.0
# SDL3 is loaded at run time, so it is not found automatically.
Requires:       libSDL3-0
ExclusiveArch:  x86_64 aarch64

%description
OpenFive is an open-source reimplementation of the engine of the 1999 adventure
game The Famous Five: Treasure Island (5 Freunde auf Schatzsuche). It plays the
original adventure natively, in a window or fullscreen.

OpenFive contains no code, graphics, text, speech or music from the original
game. You need your own copy of the game CD (English or German), or a disc image
of it.

%prep
%autosetup

%build
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export HOME=%{_builddir}/home
dotnet publish src/OpenFive/OpenFive.csproj -c Release --no-self-contained \
    -p:UseAppHost=false -o publish

%install
install -d %{buildroot}%{_prefix}/lib/%{name}
cp -a publish/. %{buildroot}%{_prefix}/lib/%{name}/
rm -f %{buildroot}%{_prefix}/lib/%{name}/*.pdb
install -Dm0755 packaging/linux/openfive %{buildroot}%{_bindir}/openfive
install -Dm0644 packaging/linux/%{appid}.desktop %{buildroot}%{_datadir}/applications/%{appid}.desktop
install -Dm0644 packaging/linux/%{appid}.metainfo.xml %{buildroot}%{_datadir}/metainfo/%{appid}.metainfo.xml
install -Dm0644 packaging/linux/%{appid}.svg %{buildroot}%{_datadir}/icons/hicolor/scalable/apps/%{appid}.svg

%check
desktop-file-validate %{buildroot}%{_datadir}/applications/%{appid}.desktop

%files
%license LICENSE
%doc README.md docs/ENGINE.md docs/PROGRESS.md
%{_bindir}/openfive
%{_prefix}/lib/%{name}/
%{_datadir}/applications/%{appid}.desktop
%{_datadir}/metainfo/%{appid}.metainfo.xml
%{_datadir}/icons/hicolor/scalable/apps/%{appid}.svg

%changelog
