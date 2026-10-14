using OpenFive.Engine.Formats;
using OpenFive.Engine.Graphics;
using OpenFive.Engine.World;

namespace OpenFive.Engine.Game;

/// <summary>What the pointer is over: an object or a character.</summary>
/// <param name="Name">The object's or character's name.</param>
/// <param name="Label">The text shown by the pointer ("" for none).</param>
/// <param name="Object">The object, if it is one.</param>
/// <param name="Actor">The character, if it is one.</param>
public readonly record struct HoverTarget(string Name, string Label, ObjectDef? Object, Actor? Actor);

/// <summary>
/// A game in progress: the world, the rucksack, the location scripts and the player's input
/// (pointer, hover labels, clicks, dragging items).
/// </summary>
public sealed class GameSession
{
    /// <summary>The game world: location, characters, tasks.</summary>
    public GameWorld World { get; }

    /// <summary>The rucksack.</summary>
    public Inventory Inventory { get; } = new();

    /// <summary>The script of the current location.</summary>
    public LocationScript Script { get; private set; }

    /// <summary>The player's options.</summary>
    public Settings Settings { get; }

    /// <summary>Story state kept by the location scripts (puzzle progress etc.), saved with the game.</summary>
    public Dictionary<string, int> Vars { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Remembered texts of the story scripts (saved with the game).</summary>
    public Dictionary<string, string> Texts { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The value of a story variable (0 if never set). Some of the original's globals that the
    /// scripts read and write are engine state: the quest, the episode and the language.
    /// </summary>
    public int Var(string name) => name switch
    {
        Story.Vars.QuestNumber => World.Quest,
        Story.Vars.EpisodeNumber => World.Episode,
        Story.Vars.LanguageNumber => World.Data.Language == Language.German ? 1 : 2,
        _ => Vars.TryGetValue(name, out int v) ? v : 0,
    };

    /// <summary>Sets a story variable (the language cannot be set this way).</summary>
    public void SetVar(string name, int value)
    {
        if (Trace.On && Var(name) != value)
            Trace.Log($"{World.Tick}: var {name} = {value}");
        switch (name)
        {
            case Story.Vars.QuestNumber:
                World.Quest = value;
                break;
            case Story.Vars.EpisodeNumber:
                World.Episode = value;
                break;
            case Story.Vars.LanguageNumber:
                break;
            default:
                Vars[name] = value;
                break;
        }
    }

    /// <summary>The location the hero is in (menus are locations too, this one is not one of them).</summary>
    public string GameLocation { get; private set; } = "";
    /// <summary>The location the hero came from.</summary>
    public string PreviousLocation { get; private set; } = "";
    /// <summary>True once a game has been started or loaded.</summary>
    public bool InGame { get; set; }

    /// <summary>Set when the player chose to quit.</summary>
    public bool QuitRequested { get; set; }

    /// <summary>Raised when the session wants a movie played (path relative to the disc, music track).</summary>
    public event Action<string, string>? MovieRequested;

    /// <summary>Asks the platform to play a movie (see <see cref="MovieRequested"/>).</summary>
    public void PlayMovie(string path, string music) => MovieRequested?.Invoke(path, music);

    readonly Func<string, LocationScript> _scriptFactory;

    // ---- pointer state ----

    /// <summary>Pointer x position (screen pixels).</summary>
    public int MouseX { get; private set; } = 400;

    /// <summary>Pointer y position (screen pixels).</summary>
    public int MouseY { get; private set; } = 300;

    /// <summary>What the pointer is over (updated every tick).</summary>
    public HoverTarget? Hover { get; private set; }

    int _pointerFrame, _restTicks, _lastMouseX = -1, _lastMouseY = -1;

    /// <summary>Decoding buffer for the character hit test (reused between ticks).</summary>
    byte[] _hitFrame = Array.Empty<byte>();

    // ---- rucksack state ----

    /// <summary>Is the rucksack open?</summary>
    public bool InventoryOpen { get; private set; }

    /// <summary>The item being dragged, or 0.</summary>
    public int Dragging { get; private set; }

    /// <summary>The open rucksack's panel on screen.</summary>
    public const int BoxX = 268, BoxY = 146, BoxW = 448, BoxH = 308;

    const int ItemsX = BoxX + 60 + 30, ItemsY = BoxY + 30, ItemStepX = 60, ItemStepY = 50, ItemsPerRow = 6;

    /// <summary>
    /// Creates a session for <paramref name="world"/>; <paramref name="scripts"/> makes the
    /// script for a location name (default: plain locations).
    /// </summary>
    public GameSession(GameWorld world, Func<string, LocationScript>? scripts = null, Settings? settings = null)
    {
        World = world;
        Settings = settings ?? new Settings();
        _scriptFactory = scripts ?? (n => new PlainLocation(n));
        Script = new PlainLocation("");
        World.LocationTick = () =>
        {
            if (!IsMenu(World.Scene?.Name ?? ""))
                Script.EveryTick();
        };
    }

    /// <summary>Menu screens are locations too, but do not count as where the hero is.</summary>
    public static bool IsMenu(string name) =>
        name.ToLowerInvariant() is "menue" or "load" or "save" or "options" or "logo" or "locdummy";

    /// <summary>Loads a location and runs its script's entry logic.</summary>
    public void EnterLocation(string name, string from = "")
    {
        // Entering a place directly (loading a game) cancels a change that was still on its way.
        _pendingLocation = null;
        CloseInventory();
        Dragging = 0;
        World.Tasks.Clear();
        World.Texts.Clear();
        World.Chain = null;
        World.CursorVisible = true;
        if (!IsMenu(name))
        {
            if (!IsMenu(from) && from.Length > 0)
                PreviousLocation = from;
            GameLocation = name;
        }
        World.LoadLocation(name);
        if (Story.Ambience.Tracks.TryGetValue(name, out var track))
        {
            if (track.Length == 0)
                World.Audio.StopBackgroundTrack();
            else
                World.Audio.SetBackgroundTrack(track);
        }
        Script = _scriptFactory(name);
        Script.Attach(this);
        World.Hooks = Script;
        World.Dialogue.Hooks = Script;
        Script.Enter(from);
    }

    /// <summary>The mouse button was released.</summary>
    public void ButtonReleased() => Script.ButtonReleased();

    /// <summary>Text input (save names); returns true if it was used.</summary>
    public bool KeyTyped(char c) => Script.KeyTyped(c);

    /// <summary>Starts a new game: the Five arrive at Kirrin Cottage.</summary>
    public void NewGame()
    {
        Reset();
        InGame = true;
        // The rucksack starts with Dick's pocket knife and the newspaper.
        Inventory.Give(Item.ClosedKnife);
        Inventory.Give(Item.Newspaper);
        Story.NewGameState.Apply(this);
        StoryScript.NewGameMenus(this);
        World.Hero = World.Dick;
        ChangeLocation(NewGameLocation);
    }

    /// <summary>Where a new game starts: the garden of Kirrin Cottage.</summary>
    public const string NewGameLocation = "loc06";

    /// <summary>Switches the game's language (texts and speech) and saves the choice.</summary>
    public void SwitchLanguage(Language language)
    {
        World.Data.Language = language;
        World.Data.LoadTexts();
        Settings.Language = language == Language.German ? "de" : "en";
        Settings.Save();
        ChangeLocation(World.Scene?.Name ?? "menue");
    }

    // ---- saving -------------------------------------------------------------------------------

    /// <summary>The game's current state as a save named <paramref name="name"/>.</summary>
    public SaveGame Capture(string name) => new()
    {
        Name = name,
        Saved = DateTime.Now,
        Location = GameLocation,
        PreviousLocation = PreviousLocation,
        Quest = World.Quest,
        Episode = World.Episode,
        Hero = World.Hero.Name,
        Items = Inventory.Items.ToList(),
        Vars = new Dictionary<string, int>(Vars),
        Objects = new Dictionary<string, bool>(World.GlobalObjects),
        AskedOptions = World.Dialogue.AskedOptions.ToDictionary(kv => kv.Key, kv => kv.Value.ToList()),
    };

    /// <summary>Resets everything for a new game (the scripts then set the starting state).</summary>
    public void Reset()
    {
        Inventory.Clear();
        Vars.Clear();
        Texts.Clear();
        World.ResetState();
        PreviousLocation = "";
        GameLocation = "";
    }

    /// <summary>
    /// Continues a saved game: the new-game state first (so flags the save does not mention keep
    /// their defaults), then the saved state; then the saved location is entered.
    /// </summary>
    public void Restore(SaveGame s)
    {
        Reset();
        foreach (int i in s.Items)
            Inventory.Give(i);
        Story.NewGameState.Apply(this);
        StoryScript.NewGameMenus(this);
        foreach (var (k, v) in s.Vars)
            Vars[k] = v;
        foreach (var (k, v) in s.Objects)
            World.GlobalObjects[k] = v;
        foreach (var (k, v) in s.AskedOptions)
            World.Dialogue.AskedOptions[k] = new HashSet<int>(v);
        World.Quest = s.Quest;
        World.Episode = s.Episode;
        World.Hero = World.Actors.FirstOrDefault(a => a.Name == s.Hero) ?? World.Dick;
        PreviousLocation = s.PreviousLocation;
        InGame = true;
        EnterLocation(s.Location, s.PreviousLocation);
    }

    // ---- items -----------------------------------------------------------------------------

    /// <summary>Puts an item into the rucksack.</summary>
    public void GiveItem(int item) => Inventory.Give(item);

    /// <summary>Takes an item out of the rucksack.</summary>
    public void TakeItem(int item) => Inventory.Take(item);

    /// <summary>Opens the rucksack (or redraws it): the panel and the carried items in rows of six.</summary>
    public void OpenInventory()
    {
        InventoryOpen = true;
        World.ShowObject("COMMON_INV_TXT", true);
        World.MoveObject("COMMON_INV_TXT", BoxX, BoxY);
        World.MoveObject("COMMON_INV_BOX", 276 + 70 - BoxX, BoxY - 27);
        World.ShowObject("COMMON_INV_BOX", true);
        World.ShowObject("COMMON_DLGDUMMYBOX", true);
        int i = 0;
        foreach (int item in Inventory.Items)
        {
            string obj = Inventory.ObjectName(item);
            if (item == Dragging)
            {
                World.ShowObject(obj, false);
                continue;
            }
            World.MoveObject(obj, ItemsX + i % ItemsPerRow * ItemStepX, ItemsY + i / ItemsPerRow * ItemStepY);
            World.ShowObject(obj, true);
            i++;
        }
    }

    /// <summary>True when (x, y) is on the open rucksack panel.</summary>
    public static bool InRucksack(int x, int y) => x >= BoxX && x < BoxX + BoxW && y >= BoxY && y < BoxY + BoxH;

    /// <summary>Closes the rucksack.</summary>
    public void CloseInventory()
    {
        if (!InventoryOpen)
            return;
        InventoryOpen = false;
        World.ShowObject("COMMON_INV_TXT", false);
        World.ShowObject("COMMON_INV_BOX", false);
        World.ShowObject("COMMON_DLGDUMMYBOX", false);
        for (int item = 1; item <= Inventory.ItemCount; item++)
            World.ShowObject(Inventory.ObjectName(item), false);
    }

    // ---- pointer and hover -------------------------------------------------------------------

    /// <summary>Moves the pointer (kept on the screen).</summary>
    public void MoveMouse(int x, int y)
    {
        MouseX = Math.Clamp(x, 0, Framebuffer.Width - 1);
        MouseY = Math.Clamp(y, 0, Framebuffer.Height - 1);
    }

    /// <summary>The nearest clickable thing under the pointer (objects and characters).</summary>
    public HoverTarget? FindTarget(int x, int y)
    {
        HoverTarget? best = null;
        int bestDepth = int.MaxValue;
        void Consider(ObjectDef o)
        {
            if (!o.Contains(x, y) || !World.IsObjectOn(o.Name))
                return;
            if (Dragging != 0 && Inventory.ItemOf(o.Name) == Dragging)
                return;
            if (best == null || o.Depth < bestDepth)
            {
                best = new HoverTarget(o.Name, World.Data.ObjectTexts.Get(o.Name)?.Over ?? "", o, null);
                bestDepth = o.Depth;
            }
        }
        if (InventoryOpen)
        {
            foreach (var o in World.CommonObjects)
                if (Inventory.ItemOf(o.Name) != 0 || o.Name.Equals("COMMON_INV_BOX", StringComparison.OrdinalIgnoreCase))
                    Consider(o);
            return best;
        }
        if (World.Scene != null)
            foreach (var o in World.Scene.Script.Objects)
                Consider(o);
        foreach (var a in World.Actors)
        {
            if (!a.Sprite.Visible || !HitsActor(a, x, y))
                continue;
            if (best == null || a.Depth < bestDepth)
            {
                best = new HoverTarget(a.Name, a.Name, null, a);
                bestDepth = a.Depth;
            }
        }
        return best;
    }

    /// <summary>True when (x, y) is on a non-transparent pixel of the character's current frame.</summary>
    bool HitsActor(Actor a, int x, int y)
    {
        var s = a.Sprite;
        if (s.Resource == null || !s.Packed)
            return false;
        var sheet = World.Data.Packed(s.Resource.Path);
        int w = (int)Math.Round(sheet.FrameWidth * s.Scale), h = (int)Math.Round(sheet.FrameHeight * s.Scale);
        if (x < s.X || y < s.Y || x >= s.X + w || y >= s.Y + h)
            return false;
        int size = sheet.FrameWidth * sheet.FrameHeight;
        if (_hitFrame.Length != size)
            _hitFrame = new byte[size];
        int key = sheet.Decode(s.Frame, _hitFrame);
        int sx = Math.Min(sheet.FrameWidth - 1, (int)((x - s.X) / s.Scale));
        int sy = Math.Min(sheet.FrameHeight - 1, (int)((y - s.Y) / s.Scale));
        return _hitFrame[sy * sheet.FrameWidth + sx] != key;
    }

    /// <summary>A location change requested by a script; applied at the start of the next tick.</summary>
    string? _pendingLocation;

    /// <summary>Changes location once <paramref name="after"/> is released (now if null).</summary>
    public void ChangeLocation(string name, TaskHandle? after = null)
    {
        if (after == null)
        {
            _pendingLocation = name;
            return;
        }
        World.Tasks.Add(new ActionTask("CHANGELOC", _ => _pendingLocation = name) { WaitFor = after });
    }

    /// <summary>Called once per game tick: updates the hover target, pointer frame and label timing.</summary>
    public void Tick()
    {
        if (_pendingLocation is { } next)
        {
            _pendingLocation = null;
            EnterLocation(next, World.Scene?.Name ?? "");
        }
        if (InventoryOpen && Dragging != 0 && !InRucksack(MouseX, MouseY))
            CloseInventory();
        Script.Hover(MouseX, MouseY);
        var hover = World.CursorVisible ? FindTarget(MouseX, MouseY) : null;
        Hover = hover;
        bool moved = MouseX != _lastMouseX || MouseY != _lastMouseY;
        if (!moved)
            _restTicks++;
        else
            _restTicks = 0;
        World.UpdateIdle(moved, InventoryOpen || Dragging != 0 || World.Dialogue.OpenMenu != null || IsMenu(World.Scene?.Name ?? ""));
        (_lastMouseX, _lastMouseY) = (MouseX, MouseY);
        if (hover is { Object: { Command: ObjectCommand.ChangeLocation } o })
            _pointerFrame = ArrowFrame(o.CommandArg) + (int)(World.Tick / 3 % 2);
        else if (hover != null)
            _pointerFrame = _pointerFrame is >= 1 and < 4 ? _pointerFrame + 1 : 1;
        else
            _pointerFrame = 0;
    }

    /// <summary>The first pointer frame of the arrow for an exit in direction <paramref name="dir"/>.</summary>
    static int ArrowFrame(string dir) => dir.ToUpperInvariant() switch
    {
        "DOWN" => 5,
        "LEFT" => 7,
        "UP" => 9,
        "RIGHT" => 11,
        _ => 1,
    };

    /// <summary>The label to show above the pointer (after it has rested for a tick), or null.</summary>
    public string? Label => Settings.HoverLabels && _restTicks >= 1 && Hover is { Label.Length: > 0 } h ? h.Label : null;

    // ---- clicks ------------------------------------------------------------------------------

    /// <summary>Handles a click at the pointer position.</summary>
    public void Click()
    {
        int x = MouseX, y = MouseY;
        if (Script.HandleClick(x, y))
            return;
        if (World.Dialogue.OpenMenu is { } menu)
        {
            menu.Click(World, x, y);
            return;
        }
        if (!World.CursorVisible)
        {
            World.SkipRequested = true;
            return;
        }
        var target = FindTarget(x, y);
        Trace.Log($"click {x},{y} on {target?.Name ?? "nothing"} in {World.Scene?.Name} Q{World.Quest}E{World.Episode}");
        if (Dragging != 0)
        {
            if (InventoryOpen && (target == null || Inventory.ItemOf(target.Value.Name) == 0))
            {
                // Dropped back into the rucksack.
                Dragging = 0;
                OpenInventory();
                return;
            }
            DropItem(target);
            return;
        }
        if (InventoryOpen)
        {
            int item = target is { } t ? Inventory.ItemOf(t.Name) : 0;
            if (item != 0 && new Story.ItemUses(this).Click(item))
                return;
            if (item != 0)
            {
                // The item is taken in hand; the rucksack stays open (so it can be used on another
                // item) until the pointer leaves it.
                Dragging = item;
                OpenInventory();
            }
            else
                CloseInventory();
            return;
        }
        if (target is { Actor: { } actor } && actor == World.Hero)
        {
            OpenInventory();
            return;
        }
        if (target is { Object: { } obj } && Script.Click(obj.Name))
            return;
        if (target is { Actor: { } other } && Script.Click(other.Name))
            return;
        if (World.Hero.Visible)
            World.WalkTo(World.Hero, x, y);
    }

    /// <summary>Uses a rucksack item on a named object or character (as if dragged there).</summary>
    public void UseItemOn(int item, string target)
    {
        Dragging = item;
        if (Inventory.ItemOf(target) != 0)
            OpenInventory();
        else
            CloseInventory();
        var o = World.FindObject(target);
        var a = World.Actors.FirstOrDefault(x => x.Name.Equals(target, StringComparison.OrdinalIgnoreCase));
        DropItem(new HoverTarget(target, "", o, a));
    }

    /// <summary>Drops the dragged item on a target: the item-use rules first, then the location script.</summary>
    void DropItem(HoverTarget? target)
    {
        int item = Dragging;
        Dragging = 0;
        if (target == null)
            return;
        if (!new Story.ItemUses(this).Use(item, target.Value.Name))
            Script.UseItem(item, target.Value.Name);
        if (InventoryOpen)
            OpenInventory();
    }

    /// <summary>Draws the pointer, the dragged item and the hover label on top of the scene.</summary>
    public void DrawOverlay(Framebuffer fb)
    {
        if (!World.CursorVisible)
            return;
        if (Dragging != 0 && World.FindObject(Inventory.ObjectName(Dragging)) is { Gfx: >= 0 } io)
        {
            var r = World.Common.Resources[io.Gfx];
            var img = World.Data.Image(r.Path);
            fb.Blit(img, MouseX - img.Width / 2, MouseY + 32 - img.Height / 2, img.Pixels[0]);
        }
        if (Label is { } label)
        {
            int lx = Math.Min(MouseX, 650);
            if (label.Length > 15 && MouseX > 649)
                lx = 590;
            lx = Math.Clamp(lx, 0, Math.Max(0, Framebuffer.Width - World.Font.MeasureWidth(label) - 2));
            int ly = MouseY - 32 < 548 ? MouseY - 20 : MouseY - 68;
            World.Font.Draw(fb, label, lx, Math.Max(1, ly));
        }
        var cursor = World.Data.Image("bmp/common/misc/cursor.brg");
        fb.Blit(cursor, MouseX - 1, MouseY - 1, cursor.Pixels[0], _pointerFrame * 32, 0, 32, 32);
    }
}
