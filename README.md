<div align="center">

<img src="packaging/linux/io.github.benorrin.openfive.svg" alt="" width="96">

# OpenFive

**Play *The Famous Five: Treasure Island* (1999) on modern computers.**

[![Build](https://github.com/benorrin/openfive/actions/workflows/build.yml/badge.svg)](https://github.com/benorrin/openfive/actions/workflows/build.yml)
[![Latest release](https://img.shields.io/github/v/release/benorrin/openfive?include_prereleases&label=download)](https://github.com/benorrin/openfive/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/benorrin/openfive/total)](https://github.com/benorrin/openfive/releases)
[![License: GPL v2](https://img.shields.io/badge/license-GPL--2.0-blue)](LICENSE)
[![Platform: Linux](https://img.shields.io/badge/platform-Linux-informational?logo=linux&logoColor=white)](#download)

<img src="docs/screenshots/beach.png" alt="The Famous Five on the beach outside the fisherman's hut" width="800">

</div>

OpenFive is a free, open-source remake of the engine behind **The Famous Five: Treasure Island**
(*5 Freunde auf Schatzsuche*, Ravensburger Interactive, 1999). Pop in your old CD (or a disc
image of it) and play the original adventure again, in a window or fullscreen, at any size, with
no emulator or old copy of Windows needed.

* 🗺️ **The whole adventure**, from the intro to the closing credits
* 🇬🇧🇩🇪 **English and German**: both languages are on every CD, pick either
* 🖥️ **Any window size or fullscreen**, with smooth or sharp pixel scaling
* 💾 **Save and load** at any time

> [!IMPORTANT]
> OpenFive does **not** include the game itself. You need your own copy of the original
> *Famous Five: Treasure Island* CD (English or German). OpenFive reads the graphics, speech and
> music from it every time it runs.

## Download

Get the latest version from the **[Releases page](https://github.com/benorrin/openfive/releases/latest)**.

| You use | Download | |
|---------|----------|---|
| openSUSE | `openfive-…x86_64.rpm` | Install with `sudo zypper install ./openfive-*.rpm`. Adds a menu entry. |
| Any other Linux (64-bit PC) | `openfive-…-linux-x64.tar.gz` | Unpack anywhere and run `./openfive`. |
| Linux on ARM (e.g. Raspberry Pi 5) | `openfive-…-linux-arm64.tar.gz` | Unpack anywhere and run `./openfive`. |
| Windows, macOS | — | Planned, not available yet. |

The `.tar.gz` downloads need **SDL3**, which most distributions package:

| Distribution | Command |
|--------------|---------|
| Ubuntu 25.04+, Debian 13+ | `sudo apt install libsdl3-0` |
| Fedora | `sudo dnf install SDL3` |
| Arch Linux | `sudo pacman -S sdl3` |
| openSUSE | `sudo zypper install libSDL3-0` |

## Getting started

**1. Have your game disc ready.** Either insert the CD, or (better) make a disc image of it once
so you don't need the CD again:

```sh
dd if=/dev/sr0 of=famous5.iso bs=2048 status=progress
```

Images you already have also work: `.cue`/`.bin`, CloneCD `.ccd`/`.img`, or `.iso`.

**2. Start OpenFive** and tell it where the game is:

```sh
openfive --disc ~/Games/famous5.iso
```

That's it. OpenFive remembers the disc, so next time just run `openfive` (or use the menu entry).
You can also open a disc image with OpenFive straight from your file manager. If the CD is
inserted, OpenFive usually finds it by itself.

## How to play

You play the Famous Five (mostly George and Dick) on the trail of a treasure on Kirrin Island.
Explore each place, talk to everyone and look closely at things: the pointer flashes red over
anything you can interact with. Stuck? Talk to the others again; they often have a hint.

<p align="center">
  <img src="docs/screenshots/hall.png" alt="Kirrin Cottage hall" width="32%">
  <img src="docs/screenshots/wreck.png" alt="Rowing out to the wreck" width="32%">
  <img src="docs/screenshots/dungeon.png" alt="The dungeons under Kirrin Castle" width="32%">
</p>

### Controls

| | |
|---|---|
| **Click** | Walk, look at things, talk to people, pick things up |
| **Click your character** | Open the rucksack (inventory) |
| **Use an item** | Click it in the rucksack, then click what to use it on: something in the scene, a person, or another item |
| **Click** or **Space** during speech | Skip the current line |
| **F1** | Main menu: load, save, options, quit |
| **Esc** | Back to the game from a menu |
| **F11** or **Alt+Enter** | Toggle fullscreen |
| **F12** | Toggle smooth / sharp scaling |

To save, press **F1**, choose *Save game*, click a slot, type a name and press **Enter**.

## Options

| Option | What it does |
|--------|--------------|
| `--disc PATH` | Disc image or mounted CD to use (or set `OPENFIVE_DISC`) |
| `--lang en` / `--lang de` | Play in English or German |
| `--fullscreen` | Start fullscreen |
| `--sharp` | Sharp pixel scaling instead of smooth |
| `--skip-intro` | Start at the main menu |

Settings and saved games are kept in `~/.local/share/openfive/`. Set `OPENFIVE_HOME` to use a
different folder.

## Help and feedback

The game can be played all the way through, but not every side path has been tried yet. If
something goes wrong, please [open an issue](https://github.com/benorrin/openfive/issues) and
say where you were in the game; a saved game helps a lot. For detailed status, see
[docs/PROGRESS.md](docs/PROGRESS.md).

Want to build OpenFive yourself or help out? See **[docs/DEVELOPING.md](docs/DEVELOPING.md)**.

## Legal

OpenFive is an independent fan project, not affiliated with or endorsed by Ravensburger or the
rights holders of *The Famous Five*. "The Famous Five" is a trademark of its respective owner.
The program contains no code, graphics, text, speech or music from the original game; the
screenshots above show it running with an original CD. You need a legally obtained copy of the
game to play.

OpenFive is free software, licensed under the GNU General Public License, version 2 (see
[LICENSE](LICENSE)). It comes with no warranty.
