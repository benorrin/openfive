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

    public GameSession(GameWorld world, Func<string, LocationScript>? scripts = null)
    {
        World = world;
        _scriptFactory = scripts ?? (n => new PlainLocation(n));
        Script = new PlainLocation("");
    }

    /// <summary>Loads a location and runs its script's entry logic.</summary>
    public void EnterLocation(string name, string from = "")
    {
        CloseInventory();
        World.LoadLocation(name);
        Script = _scriptFactory(name);
        Script.Attach(this);
        World.Hooks = Script;
        World.Dialogue.Hooks = Script;
        Script.Enter(from);
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

    /// <summary>Called once per game tick: updates the hover target, pointer frame and label timing.</summary>
    public void Tick()
    {
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
    public string? Label => _restTicks >= 1 && Hover is { Label.Length: > 0 } h ? h.Label : null;

    // ---- clicks ------------------------------------------------------------------------------

    /// <summary>Handles a click at the pointer position.</summary>
    public void Click()
    {
        int x = MouseX, y = MouseY;
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
