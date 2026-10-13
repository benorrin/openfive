# Engine behaviour notes

This document describes how the game engine behaves, as far as OpenFive needs to reproduce it.
It is written in our own words; OpenFive's code is written from these notes.

## Timing

The game logic runs at 10 ticks per second. Every tick: input is read, tasks are advanced,
the scene is drawn. Delays in scripts are counted in ticks.

## Screen

800x600, 16-bit colour (RGB565). A location is an animated background (Smacker video, usually
1-10 frames, looped), objects and actors drawn on top, sorted by depth (larger depth = further
away, drawn first), and an 8-bit mask whose regions hide actors that stand behind scenery.
Each mask region's depth comes from sample points in the location file ("lmsk file:x,y,depth;...").

## Tasks

Almost everything that happens over time is a *task*. Scripts create tasks and get back a handle;
a task can be told to wait for another task (by handle) and/or for a number of ticks before it
runs. This is how scripts chain actions ("walk there, then turn right, then say this line").

There are two kinds of queues:

* the **global task list** (up to 600 entries), and
* a **queue per actor** (walks, state changes, actor animations), processed in order: only the
  head of an actor's queue runs, and it is removed when it finishes.

A global task waiting on a handle is released when the handle's task has finished (global task
handle) or when the actor queue entry it names has finished (actor task handle). Then its delay
counts down one per tick; at zero it performs its action. Most tasks finish immediately after
acting; some (speech, animations) finish later.

Global task kinds:

| Kind | Action when it runs |
|------|---------------------|
| sound | Play a positioned sound effect (pan, volume; looped if requested) |
| stop sound | Stop a looping sound effect |
| show sprite for a while | Keep a sprite visible for N ticks, then hide it |
| stop animation | End a running named animation (optionally switching an object on/off) |
| hover label | Show the name of what is under the cursor once the mouse rests for a tick; for actors, their name |
| object on/off | Switch a named object on or off |
| cursor | Show/hide the mouse cursor and optionally move it |
| speech line | Play a speech recording, showing its text while it plays; finishes when the recording ends |
| dialogue line | Start a line from the dialogue files (scene id + line number), shown in the dialogue box with the speaker's talking head |
| flag | Set or clear a boolean |
| toggle | Invert a boolean |
| increment | Add one to a counter (scripts use this to advance puzzle states when an action completes) |
| assign | Store a value in a variable |
| place actor | Put an actor at a position with a given state and make it visible |
| hide actor | Remove an actor from the scene |
| drag icon | Draw the inventory item being dragged under the cursor |
| every frame | Housekeeping run each tick |

Actor queue kinds: walk to a point (split into straight segments along paths/areas), set state
(stand/face/talk animations), play an actor animation for a number of loops.

A walk segment finishes only when the actor stands exactly on its end point. When characters
would collide, the avoidance logic inserts short detour segments.

## Hover labels

The label appears above the cursor (about 32 pixels up, kept on screen) after the mouse has
rested for one tick, using the object's "over" text, or the character's name for actors.

## Sound

* One **background track** (music or ambience) loops at a time. Asking for the track that is
  already playing leaves it playing; a different track replaces it.
* **Effects** play with a volume (0-100) and a pan (-10000 left .. 10000 right); some loop until
  stopped.
* **Speech** recordings are looked up by name in the language's speech folders.
* The option sliders (0-100) map to attenuation of (volume - 100) x 100 millibels, so 100 is full
  volume and 0 is silent.

## Sprites and animations

* Every image resource has one **sprite**: visible flag, depth, source rectangle (or frame of a
  packed sheet), screen position. Objects and animations that use the same image share it.
* When a location loads, each object with a picture puts its sprite at the object's position and
  depth, visible if the object is on. Packed sheets default to depth 8000.
* A **named animation** ("anim Name,res,w,h,x,y:frame,dx,dy;...") shows frame *k* of its image as
  the strip `[w*k, 0, w, h]` (or sheet frame *k*) at `(x+dx, y+dy)`. Scripts can move an
  animation's `(x, y)`.
* Playing: each tick advances one frame (the very first tick already shows frame 1). After the last
  frame it restarts while loops remain (-1 = forever). Then it hides the sprite, optionally
  switches an object on/off, and ends. If another animation on the same image is still running,
  the sprite stays visible.
* Switching an object on/off updates the location, the persistent object list ("object.res"),
  and tells the location's script (this is how picking things up grants items).
* Moving an object shifts its picture and its click polygon together.

## Characters

* Six scripted characters: Dick, George, Julian, Anne, Timmy, Quentin. Each state (1-116) shows
  the animation "a&lt;Name&gt;_&lt;state&gt;" from the common data, e.g. 5 = walk right,
  11 = stand facing right.
* **Walkable space** is made of paths (polylines) and areas (convex polygons) joined by link
  points. A point is "on" a path when its y matches the line at that x; points on an area's edge
  count as inside. The nearest walkable point is the closest of the nearest point on any path
  (perpendicular foot, clamped to the segment) and the nearest in any area.
* **Routes** chain shapes from the character's shape to the target's through link points,
  choosing the cheapest chain (distance along paths, straight-line across areas). A walk is a
  list of straight legs: across an area directly, along a path via its corners. A disabled shape
  ends the route there.
* **Speed** is 10 pixels per tick horizontally and 4 vertically, times the character's scale;
  the shorter axis is slowed in proportion (counted in whole speed units) and once the longer
  axis is under one unit away the rest is covered at once. A leg ends only when the character is
  exactly on its end point. Direction: slope shallower than 1:4 is sideways, else diagonal.
* **Scale and depth** come from the shape's per-point values at the character's height (scale
  values are percent x 2). Sheets stored at double resolution (about 400 pixels tall) are drawn at
  half the scale. The sheet frame is drawn so the frame's offset point (x halved) sits on the
  character's position.
* **Hiding behind scenery**: each mask region gets a depth from the location's sample points; a
  character's pixel is hidden where the region in front is nearer than the character.
* **Queues**: walk legs, "walk there (plan the route when reached)", state changes, and actions
  to run on arrival. Tasks waiting on a queue entry start once the character reaches it.
* **Followers**: while a character walks, a follower more than 100 pixels away (scaled) on
  either axis is sent walking to the leader.
* The original also steers walking characters around each other; OpenFive leaves this out for
  now, because it can make characters oscillate and block scenes.

## Dialogue

* Lines live in the dialogue files of each speaker, keyed by kind ("V" = line, "DLG" = choice
  menu), story chapter and step, scene id and line number. Speech recordings are named in the
  file and stored in the speaker's speech folder.
* A **line** shows the dialogue box (an object of the common set) at the bottom of the screen,
  the speaker's talking-head animation at its left, the text from x=50 wrapping before x=780,
  and plays the recording; the pointer is hidden. When the recording ends the box and head go
  away. Without a recording the text stays for a time based on its length.
* Lines chain: each starts after the previous step of the current sequence, and becomes the new
  end of the sequence. After queuing a line, the location's script is told (so it can plan what
  comes next); after the **last** line the pointer comes back.
* A **choice menu** shows the options not yet asked, in a box at the top, middle or bottom of the
  screen (rows 70 pixels apart), with the asking character's head. The player clicks an option;
  that character speaks it and the location's script learns which option (as numbered in the
  file) was picked. When only one option is left it is asked without showing the menu.
* A click (or Space) while a line is being spoken skips to its end (an addition over the
  original).

## Pointer, labels and the rucksack

* The pointer is a 13-frame strip (32x32 each): frame 0 plain blue arrow, 1-4 the blue arrow
  with a pulsing outline over anything clickable, then red arrows for exits (down, left, up,
  right, two frames each, alternating). The hot spot is one pixel in from the top-left.
* The nearest clickable thing (lowest depth) under the pointer wins: objects by their click
  polygon, characters by a non-transparent pixel of their current frame.
* The label (object's "over" text, or the character's name) is drawn about 20 pixels above the
  pointer once the mouse has rested for a tick, or below it near the bottom edge. OpenFive keeps
  long labels fully on screen.
* Clicking the character you are playing opens the **rucksack**: a 448x308 panel centred on the
  screen with the rucksack picture to its left and the items in rows of up to six, 60 pixels
  apart horizontally and 50 vertically. Clicking an item picks it up; it then follows the
  pointer until the next click, which uses it on whatever is under the pointer. Clicking
  anything else closes the rucksack.
* Other clicks go to the location's script; if it does not handle them, the hero walks to the
  clicked point.

## Menus

* The intro is the language's Smacker movie with its title song; a click or key skips it.
* Menu screens are locations of their own. Buttons are pairs of objects, the plain picture and
  the highlighted one (name + "1"), swapped while the pointer is over the button. German and
  English screens use different objects.
* Main menu: new game, load, save (only during a game), quit (with a yes/no box), options, back
  to the game (only during a game).
* Load/save: ten slots listed as "n.) NAME". Saving asks for a name typed on the keyboard.
  OpenFive saves in its own JSON format (location, story step, rucksack, story variables, object
  switches, questions already asked), not the original's binary files.
* Options: shadows, background animation, smoothing, dialogue box, pointer labels (on/off), speech
  and music volume sliders (0-100), and the language flag.
* A new game starts in the garden of Kirrin Cottage with the pocket knife and the newspaper in
  the rucksack.
