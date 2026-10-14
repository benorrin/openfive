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
| 8 | Intro movie, main menu (with quit confirmation), load and save screens (named slots, own JSON format), options (toggles, volume sliders, language), F1 menu, settings file, title logo, ending and credits, character shadows | done |
| 9 | Story logic, location by location (loc01 ... loc29, ship, extro) | done: the game plays from a new game to the credits |
| 10 | Packaging: openSUSE first; later Windows, macOS, other Linux distributions | |

## Story logic status

The whole story has been played through with the automated play-through (`--play`), following a
published walkthrough, from a new game to the closing credits.

| Location | Description | State |
|----------|-------------|-------|
| loc06 | Garden of Kirrin Cottage: opening scene, Aunt Fanny's questions, bucket, doors | played through |
| loc07, loc08 | Shed, cave | played through |
| loc05 | Kitchen: breakfasts, tea scene, sandwiches, the diary, the tin opener, the note, the teapot | played through |
| loc04 | Hall: umbrella, keyhole, suitcase, the study door | played through |
| loc02 | Guest room: cupboard, bag, rug and the diary | played through |
| loc03 | Study: Quentin, the ship's layout, the tea, the fire, the window, the model ship | played through |
| schiff, morgen/morning, hand/hand2, schiffc1-3, locKarte | Close-ups | played through |
| loc09, loc13, loc15, loc16 | Beach and fisherman, boat, the wreck, the cabin | played through |
| loc14, loc17, loc17b, loc18, loc19 | Kirrin Island: beach, castle courtyard, the well, the big stone | played through |
| loc21-loc28 | Dungeon stairs, labyrinth, treasure door and room, the cell, the hole in the well wall | played through |
| loc17 (quest 11), locextro | Distracting the professor, the rope on the ivy, the radio, the ending and credits | played through |
| loc01, loc20, loc29 | Other locations, and side paths everywhere | converted; not all tried |
