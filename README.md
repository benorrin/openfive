# OpenFive

OpenFive is an open-source reimplementation of the game engine of **The Famous Five: Treasure
Island** (*5 Freunde auf Schatzsuche*, Ravensburger Interactive, 1999), written in C# for modern
systems. It lets you play the original adventure natively, in a resizable window or fullscreen,
at any resolution.

OpenFive contains **no code, graphics, text, speech or music from the original game**. You need
your own copy of the game CD (or a disc image of it): OpenFive reads all game data from the disc
every time it runs.

> **Status:** the whole adventure can be played, from the intro to the closing credits. Not every
> side path has been tried yet. See
> [docs/PROGRESS.md](docs/PROGRESS.md) for details.

## What you need

* The original *Famous Five: Treasure Island* CD (English or German release), either:
  * a disc image: `.cue` + `.bin`, `.ccd` + `.img`, a raw `.img`/`.bin`, or an `.iso`, or
  * the physical CD, mounted (for example at `/run/media/$USER/FAMOUS5`).
* SDL3 (`libSDL3-0` on openSUSE).
* To build from source: the .NET 8 SDK.

### Making a disc image

If you have the CD, an image keeps the drive free and loads faster. On Linux:

```sh
dd if=/dev/sr0 of=famous5.iso bs=2048 status=progress
```

Images made by other tools (CloneCD `.ccd/.img/.sub`, `.cue/.bin`) work as they are; point
OpenFive at the `.cue` or `.ccd` file.

## Playing

```sh
openfive --disc ~/Games/famous5/FAMOUS5.cue
```

Instead of `--disc` you can set the `OPENFIVE_DISC` environment variable. OpenFive remembers the
disc you used last, so afterwards plain `openfive` (or the menu entry) is enough. Without a disc
given or remembered, it looks for the CD mounted under `/run/media`, `/media` or `/mnt`. From
your file manager, you can also open a disc image (`.cue`, `.iso`, ...) with OpenFive.

| Option | Meaning |
|--------|---------|
| `--disc PATH` | disc image or mounted CD |
| `--lang en` / `--lang de` | English or German (both languages are on every CD) |
| `--fullscreen` | start fullscreen |
| `--sharp` | sharp pixel scaling instead of smooth |
| `--skip-intro` | start at the main menu |

Settings and saved games are kept in `~/.local/share/openfive/` (`$XDG_DATA_HOME/openfive`;
on Windows and macOS in the application data folder). Set `OPENFIVE_HOME` to use another folder.

### Controls

* **Mouse:** move the pointer over the scene; names of things appear above it. Click to walk,
  look at things, talk to people and pick things up. Left and right buttons do the same.
* **Rucksack (inventory):** click the character you are playing. To use an item, click it to take
  it in hand, then click what to use it on: something in the scene, a person, or another item in
  the rucksack. Clicking the treasure map looks at it.
* **Conversations:** click (or press Space) to skip the line being spoken. When you are offered
  choices, click the one you want.
* **F1:** main menu (load, save, options, quit). **Escape** in a menu returns to the game.
* **Saving:** choose *Save game* in the main menu, click a slot, type a name, press **Enter**.
* **F11** or **Alt+Enter:** toggle fullscreen. **F12:** toggle smooth/sharp scaling.

### How to play

You play the Famous Five (mostly George and Dick) on the trail of a treasure on Kirrin Island.
Explore each place, talk to everyone, and look closely at things: the pointer flashes red over
anything you can interact with. Items you collect go into the rucksack; many puzzles are solved by
dragging an item onto a person or an object. If you are stuck, talk to the others again: they
often give hints.

## Installing

### openSUSE Tumbleweed

Build the package from a checkout and install it:

```sh
sudo zypper install rpm-build dotnet-sdk-8.0
git clone https://github.com/benorrin/openfive.git
cd openfive
packaging/opensuse/build-rpm.sh
sudo zypper install dist/x86_64/openfive-*.rpm
```

This installs `openfive`, a menu entry and the .NET 8 runtime and SDL3 it needs.

### Other Linux distributions

`packaging/linux/build-portable.sh` (or `build-portable.sh linux-arm64`) makes
`dist/openfive-VERSION-linux-x64.tar.gz`: a single self-contained executable that needs no .NET
installed, only SDL3 (`libSDL3.so.0`, packaged by most distributions). Unpack it anywhere and run
`./openfive`.

## Building from source

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

The code is platform-neutral (.NET 8 + SDL3). Packages for Windows, macOS and other Linux
distributions are planned; the same `dotnet publish` command works with `-r win-x64`,
`-r osx-arm64`, etc., with the matching SDL3 library placed next to the executable
(`SDL3.dll`, `libSDL3.0.dylib`).

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

## Legal

OpenFive is an independent project, not affiliated with or endorsed by Ravensburger or the
rights holders of *The Famous Five*. "The Famous Five" is a trademark of its respective owner.
You need a legally obtained copy of the original game to play.

OpenFive is free software: you can redistribute it and/or modify it under the terms of the GNU
General Public License, version 2, as published by the Free Software Foundation (see
[LICENSE](LICENSE)). It is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR
PURPOSE.
