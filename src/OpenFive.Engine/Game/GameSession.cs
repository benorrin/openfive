using OpenFive.Engine.Formats;
using OpenFive.Engine.Graphics;
using OpenFive.Engine.World;

namespace OpenFive.Engine.Game;

/// <summary>What the pointer is over.</summary>
public readonly record struct HoverTarget(string Name, string Label, ObjectDef? Object, Actor? Actor);

/// <summary>
/// A game in progress: the world, the rucksack, the location scripts and the player's input
/// (pointer, hover labels, clicks, dragging items).
/// </summary>
public sealed class GameSession
{
    public GameWorld World { get; }
    public Inventory Inventory { get; } = new();
    public LocationScript Script { get; private set; }
    public Settings Settings { get; }

    /// <summary>Story state kept by the location scripts (puzzle progress etc.), saved with the game.</summary>
    public Dictionary<string, int> Vars { get; } = new(StringComparer.OrdinalIgnoreCase);

    public int Var(string name) => Vars.TryGetValue(name, out int v) ? v : 0;
    public void SetVar(string name, int value) => Vars[name] = value;

    /// <summary>The location the hero is in (menus are locations too, this one is not one of them).</summary>
    public string GameLocation { get; private set; } = "";
    /// <summary>The location the hero came from.</summary>
    public string PreviousLocation { get; private set; } = "";
    /// <summary>True once a game has been started or loaded.</summary>
    public bool InGame { get; set; }
    public bool QuitRequested { get; set; }

    /// <summary>Raised when the session wants a movie played (path relative to the disc, music track).</summary>
    public event Action<string, string>? MovieRequested;
    public void PlayMovie(string path, string music) => MovieRequested?.Invoke(path, music);
    readonly Func<string, LocationScript> _scriptFactory;

    // Pointer
    public int MouseX { get; private set; } = 400;
    public int MouseY { get; private set; } = 300;
    public HoverTarget? Hover { get; private set; }
    int _pointerFrame, _restTicks, _lastMouseX = -1, _lastMouseY = -1;

    // Rucksack
    public bool InventoryOpen { get; private set; }
    /// <summary>The item being dragged, or 0.</summary>
    public int Dragging { get; private set; }
    public const int BoxX = 268, BoxY = 146, BoxW = 448, BoxH = 308;
    const int ItemsX = BoxX + 60 + 30, ItemsY = BoxY + 30, ItemStepX = 60, ItemStepY = 50, ItemsPerRow = 6;

    public GameSession(GameWorld world, Func<string, LocationScript>? scripts = null, Settings? settings = null)
    {
        World = world;
        Settings = settings ?? new Settings();
        _scriptFactory = scripts ?? (n => new PlainLocation(n));
        Script = new PlainLocation("");
    }

    /// <summary>Menu screens are locations too, but do not count as where the hero is.</summary>
    public static bool IsMenu(string name) =>
        name.ToLowerInvariant() is "menue" or "load" or "save" or "options" or "logo" or "locdummy";

    /// <summary>Loads a location and runs its script's entry logic.</summary>
    public void EnterLocation(string name, string from = "")
    {
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
        Script = _scriptFactory(name);
        Script.Attach(this);
        World.Hooks = Script;
        World.Dialogue.Hooks = Script;
        Script.Enter(from);
    }

    public void ButtonReleased() => Script.ButtonReleased();

    /// <summary>Text input (save names); returns true if it was used.</summary>
    public bool KeyTyped(char c) => Script.KeyTyped(c);

    /// <summary>Starts a new game: the Five arrive at Kirrin Cottage.</summary>
    public void NewGame()
    {
        Reset();
        InGame = true;
        // The rucksack starts with Dick's pocket knife and the newspaper.
        Inventory.Give(6);
        Inventory.Give(27);
        World.Hero = World.Dick;
        ChangeLocation(NewGameLocation);
    }

    public const string NewGameLocation = "loc06";

    public void SwitchLanguage(Language language)
    {
        World.Data.Language = language;
        World.Data.LoadTexts();
        Settings.Language = language == Language.German ? "de" : "en";
        Settings.Save();
        ChangeLocation(World.Scene?.Name ?? "menue");
    }

    // ---- saving -------------------------------------------------------------------------------

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
        World.ResetState();
        PreviousLocation = "";
        GameLocation = "";
    }

    public void Restore(SaveGame s)
    {
        Reset();
        foreach (int i in s.Items)
            Inventory.Give(i);
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

    public void GiveItem(int item) => Inventory.Give(item);
    public void TakeItem(int item) => Inventory.Take(item);

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
            World.MoveObject(obj, ItemsX + i % ItemsPerRow * ItemStepX, ItemsY + i / ItemsPerRow * ItemStepY);
            World.ShowObject(obj, true);
            i++;
        }
    }

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
            if (o.Click.Length < 3 || !World.IsObjectOn(o.Name) || !Geometry.Contains(o.Click, x, y))
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

    bool HitsActor(Actor a, int x, int y)
    {
        var s = a.Sprite;
        if (s.Resource == null || !s.Packed)
            return false;
        var sheet = World.Data.Packed(s.Resource.Path);
        int w = (int)Math.Round(sheet.FrameWidth * s.Scale), h = (int)Math.Round(sheet.FrameHeight * s.Scale);
        if (x < s.X || y < s.Y || x >= s.X + w || y >= s.Y + h)
            return false;
        var buf = new byte[sheet.FrameWidth * sheet.FrameHeight];
        int key = sheet.Decode(s.Frame, buf);
        int sx = Math.Min(sheet.FrameWidth - 1, (int)((x - s.X) / s.Scale));
        int sy = Math.Min(sheet.FrameHeight - 1, (int)((y - s.Y) / s.Scale));
        return buf[sy * sheet.FrameWidth + sx] != key;
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
        Script.Hover(MouseX, MouseY);
        var hover = World.CursorVisible ? FindTarget(MouseX, MouseY) : null;
        Hover = hover;
        if (MouseX == _lastMouseX && MouseY == _lastMouseY)
            _restTicks++;
        else
            _restTicks = 0;
        (_lastMouseX, _lastMouseY) = (MouseX, MouseY);
        if (hover is { Object: { Command: ObjectCommand.ChangeLocation } o })
            _pointerFrame = ArrowFrame(o.CommandArg) + (int)(World.Tick / 3 % 2);
        else if (hover != null)
            _pointerFrame = _pointerFrame is >= 1 and < 4 ? _pointerFrame + 1 : 1;
        else
            _pointerFrame = 0;
    }

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
        if (Dragging != 0)
        {
            DropItem(target);
            return;
        }
        if (InventoryOpen)
        {
            int item = target is { } t ? Inventory.ItemOf(t.Name) : 0;
            if (item != 0)
            {
                Dragging = item;
                CloseInventory();
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

    void DropItem(HoverTarget? target)
    {
        int item = Dragging;
        Dragging = 0;
        if (target != null)
            Script.UseItem(item, target.Value.Name);
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
