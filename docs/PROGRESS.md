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
| 4 | Actors: hero state animations, walk paths/areas, route finding, avoidance, scaling with distance, hiding behind scenery (mask) | |
| 5 | Task scheduler (actor queues, global tasks, wait-for handles, delays) | |
| 6 | Dialogue (speech lines, talking heads, choice menus, skip) | |
| 7 | Cursor, hover labels, click handling, inventory (rucksack, drag/drop, combine) | |
| 8 | Menus: main menu, options, load/save (own save format), language switch, intro/extro videos | |
| 9 | Story logic, location by location (loc01 ... loc29, ship, extro) | |
| 10 | Packaging: openSUSE first; later Windows, macOS, other Linux distributions | |

## Story logic status

| Location | Description | State |
|----------|-------------|-------|
| (none yet) | | |
