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
