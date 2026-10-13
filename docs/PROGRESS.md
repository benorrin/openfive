# OpenFive progress

OpenFive is a from-scratch C# reimplementation of the engine of *The Famous Five: Treasure Island*
(Ravensburger, 1999). It contains no code or assets from the original game: all graphics, text,
speech and music are read from the player's own CD (or disc image) at runtime.

Behaviour is reimplemented from documentation of how the original engine works (see
[ENGINE.md](ENGINE.md)), gathered by studying the original program for interoperability.

## Milestones

| # | Milestone | State |
|---|-----------|-------|
| 1 | Disc access (ISO9660, raw .img/.bin, .cue/.ccd, mounted CD), formats (BRG, Smacker, WAV, .res, texts), font, framebuffer, sky + background compositing, SDL3 host, scene viewer (all 43 locations render) | done |
| 2 | Audio mixer (music, ambience, effects, speech), SDL3 output, option volumes | done |
| 3 | Sprites (one per image, depth sorted), named animations with loops and end-of-animation object switches, object on/off with location hooks, global task scheduler | done |
| 4 | Characters: state animations, walk paths/areas, routes between them, scaling and depth with distance, hiding behind scenery (mask), action queues, followers. Not yet: collision avoidance between characters (deliberately left out for now), footstep sounds | done |
| 5 | Task scheduler: actor queues, global tasks, wait-for handles, delays; speech, text, pointer, place/hide character tasks | done |
| 6 | Dialogue: lines with box, talking head, recording and text; choice menus (remaining options, auto-pick of the last one); skip; location hooks | done |
| 7 | Pointer (game cursor frames, exit arrows), hover labels on objects and characters, click dispatch to location scripts, rucksack (open by clicking the hero, drag items onto things) | done |
| 8 | Intro movie, main menu (with quit confirmation), load and save screens (named slots, own JSON format), options (toggles, volume sliders, language), F1 menu, settings file. Not yet: character shadows, the ending | done |
| 9 | Story logic, location by location (loc01 ... loc29, ship, extro) | |
| 10 | Packaging: openSUSE first; later Windows, macOS, other Linux distributions | |

## Story logic status

| Location | Description | State |
|----------|-------------|-------|
| (none yet) | | |
