# Developing OpenFive

This page is for people who want to build OpenFive themselves or work on it. To just play, see
the [README](../README.md).

## Building from source

You need the .NET 8 SDK and SDL3.

### openSUSE Tumbleweed

```sh
sudo zypper install dotnet-sdk-8.0 libSDL3-0
git clone https://github.com/benorrin/openfive.git
cd openfive
dotnet build -c Release
dotnet run -c Release --project src/OpenFive -- --disc /path/to/FAMOUS5.cue
```

A self-contained build (no .NET runtime needed on the target machine):

```sh
dotnet publish src/OpenFive -c Release -r linux-x64 --self-contained -o out/linux-x64
./out/linux-x64/OpenFive --disc /path/to/FAMOUS5.cue
```

### Other platforms

The code is platform-neutral (.NET 8 + SDL3). The same `dotnet publish` command works with
`-r win-x64`, `-r osx-arm64`, etc., with the matching SDL3 library placed next to the executable
(`SDL3.dll`, `libSDL3.0.dylib`).

## Packaging

The release downloads are built by GitHub Actions (`.github/workflows/build.yml`) on every push.
To publish a release, set `<Version>` in `Directory.Build.props` (and in
`packaging/opensuse/openfive.spec` and the `metainfo.xml`), then push a matching tag:

```sh
git tag v1.0.0 && git push origin v1.0.0
```

Each download is a self-contained single executable (no .NET needed) with SDL3 next to it. The
scripts the workflow runs also work locally:

* **Linux:** `packaging/linux/build-release.sh [linux-x64|linux-arm64]` makes the `.tar.gz`, the
  `.deb` and the `.rpm` (one RPM for openSUSE and Fedora: everything it needs is inside). It builds SDL3 from source with `build-sdl3.sh` (set `SDL3_LIB` to use a
  `libSDL3.so.0` you already have).
* **Windows:** `packaging/windows/build-release.ps1` makes the `.zip`, with the official `SDL3.dll`.
* **macOS:** `packaging/macos/build-release.sh [osx-arm64|osx-x64]` makes the `.dmg` with
  `OpenFive.app`, signed ad hoc (not notarized).
* **openSUSE source RPM:** `packaging/opensuse/build-rpm.sh` (needs `rpm-build` and
  `dotnet-sdk-8.0`) builds an RPM from source that uses the system's .NET 8 runtime and SDL3, for
  distribution repositories such as the Open Build Service.

## Developer tools

* `--loc NAME` starts a game directly in a location (with `--from`, `--quest Q,E`, `--item N`).
* `--script FILE --frames N` runs the game off-screen with scripted input
  (`TICK move X Y`, `TICK click X Y`, `TICK key SCANCODE`, `TICK shot FILE.bmp`), for automated tests.
* `--play FILE` plays through the game off-screen from a list of commands (`click OBJECT`,
  `use ITEM TARGET`, `choose OPTION`, `expect LOCATION`, `save SLOT`, `load SLOT`, ...; see
  `src/OpenFive/PlayRunner.cs`), skipping speech, and reports anything that did not work.
  `lines SCENE` lists every line of a dialogue scene with its quest and episode.
  `explore SLOT` loads a save and tries every object, character and rucksack item there, each
  from a fresh load; with `OPENFIVE_TRACE=1`, comparing its output before and after a change
  shows whether the change altered the game.
* `tests/play/run.sh /path/to/FAMOUS5.cue` plays the whole game, from a new game to the closing
  credits, in about a minute (nine `--play` scripts in `tests/play`, each continuing from the
  previous one's save). It exits with an error if any step did not work.
* Setting `OPENFIVE_TRACE=1` prints clicks, dialogue lines, story variables and pointer changes.
* `--view LOCATION` opens a scene viewer (e.g. `--view loc05`): the location's background and
  objects, with hover labels.
* `--view LOCATION --screenshot out.bmp --frames N` renders off-screen and saves a picture
  (useful in automated tests, together with `SDL_VIDEO_DRIVER=offscreen`).

## Project layout

| Path | Contents |
|------|----------|
| `src/OpenFive.Engine/IO` | Reading the CD: ISO 9660 images (cooked and raw sectors, cue/ccd sheets), mounted discs |
| `src/OpenFive.Engine/Formats` | File formats: BRG images, Smacker video, WAV audio, `.res` location files, object and dialogue texts |
| `src/OpenFive.Engine/Graphics` | 800x600 framebuffer, bitmap font, screenshots |
| `src/OpenFive.Engine/World` | Scenes, objects, actors, walking, tasks, dialogue |
| `src/OpenFive.Engine/Game` | Game session, saves, rucksack, menus, videos, and `StoryScript` (the commands the story scripts use) |
| `src/OpenFive.Engine/Story` | One story script per location, the story variables and the new-game state |
| `src/OpenFive` | The application: SDL3 window, audio and input |
| `docs/ENGINE.md` | How the engine behaves |
| `docs/PROGRESS.md` | Milestones and per-location status |
