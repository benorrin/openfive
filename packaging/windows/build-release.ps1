# Builds the Windows download: a zip with OpenFive.exe (self-contained, no .NET needed) and SDL3.dll.
#
#   packaging/windows/build-release.ps1 [-Rid win-x64] [-Out dist]
#
# Needs the .NET 8 SDK and network access (for the runtime and the official SDL3 build).
param(
    [string]$Rid = "win-x64",
    [string]$Out = "",
    [string]$SdlVersion = "3.4.18"
)
$ErrorActionPreference = "Stop"
$top = Resolve-Path "$PSScriptRoot/../.."
if (-not $Out) { $Out = "$top/dist" }
New-Item -ItemType Directory -Force $Out | Out-Null
$version = ([xml](Get-Content "$top/Directory.Build.props")).Project.PropertyGroup.Version
$arch = $Rid.Substring(4)
$work = Join-Path ([IO.Path]::GetTempPath()) ([Guid]::NewGuid())
$name = "OpenFive-$version-windows-$arch"
$app = "$work/$name"

dotnet publish "$top/src/OpenFive/OpenFive.csproj" -c Release -r $Rid --self-contained `
    -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -o $app
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Remove-Item "$app/*.xml"

Invoke-WebRequest "https://github.com/libsdl-org/SDL/releases/download/release-$SdlVersion/SDL3-$SdlVersion-win32-$arch.zip" `
    -OutFile "$work/sdl.zip"
Expand-Archive "$work/sdl.zip" "$work/sdl"
Copy-Item (Get-ChildItem "$work/sdl" -Recurse -Filter SDL3.dll | Select-Object -First 1).FullName $app
Copy-Item "$top/LICENSE" "$app/LICENSE.txt"

Compress-Archive -Path $app -DestinationPath "$Out/$name.zip" -Force
Remove-Item -Recurse -Force $work
Write-Output "$Out/$name.zip"
