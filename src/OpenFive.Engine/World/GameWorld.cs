using OpenFive.Engine.Audio;
using OpenFive.Engine.Formats;
using OpenFive.Engine.Graphics;

namespace OpenFive.Engine.World;

/// <summary>Notified by the world when something a location script cares about happens.</summary>
public interface ILocationHooks
{
    /// <summary>An object was switched on or off (by a script, an animation or a task).</summary>
    void ObjectChanged(GameWorld world, string name, bool on) { }
}

/// <summary>
/// The running game: the current location, its sprites and objects, the task list, and the state
/// that persists between locations (object switches, story variables).
/// </summary>
public sealed class GameWorld
{
    public GameData Data { get; }
    public GameAudio Audio { get; }
    public SpriteBank Sprites { get; }
    public TaskScheduler Tasks { get; } = new();
    public Scene? Scene { get; private set; }
    public ILocationHooks? Hooks { get; set; }

    readonly ResScript _common;
    readonly Dictionary<string, AnimInfo> _anims = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, AnimInfo> _commonAnims = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>Persistent on/off state of named objects (initialised from "object.res").</summary>
    public Dictionary<string, bool> GlobalObjects { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Game ticks since start (10 per second).</summary>
    public long Tick { get; private set; }

    public GameWorld(GameData data, GameAudio audio)
    {
        Data = data;
        Audio = audio;
        Sprites = new SpriteBank(data);
        _common = data.Location("common");
        foreach (var a in _common.Anims)
            if (a.Resource >= 0 && a.Resource < _common.Resources.Count)
                _commonAnims[a.Name] = new AnimInfo { Def = a, Resource = _common.Resources[a.Resource], X = a.X, Y = a.Y };
        LoadGlobalObjects();
    }

    void LoadGlobalObjects()
    {
        if (!Data.Exists("object.res"))
            return;
        foreach (string raw in System.Text.Encoding.Latin1.GetString(Data.Read("object.res")).Split('\n'))
        {
            string[] p = raw.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (p.Length >= 2)
                GlobalObjects[p[0]] = p[1].Equals("ON", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>Unloads the current location and loads another one.</summary>
    public void LoadLocation(string name)
    {
        if (Scene != null)
            Sprites.Remove(Scene.Script.Resources);
        _anims.Clear();
        Data.TrimCaches();
        Scene = new Scene(Data, name);
        foreach (var a in Scene.Script.Anims)
            if (a.Resource >= 0 && a.Resource < Scene.Script.Resources.Count)
                _anims[a.Name] = new AnimInfo { Def = a, Resource = Scene.Script.Resources[a.Resource], X = a.X, Y = a.Y };
        foreach (var o in Scene.Script.Objects)
        {
            if (GlobalObjects.TryGetValue(o.Name, out bool on))
                Scene.ObjectVisible[o.Name] = on;
            if (o.Gfx < 0 || o.Gfx >= Scene.Script.Resources.Count)
                continue;
            var s = Sprites.For(Scene.Script.Resources[o.Gfx]);
            if (s == null)
                continue;
            s.Visible = Scene.IsVisible(o);
            s.X = o.GfxX;
            s.Y = o.GfxY;
            if (o.Depth != 0)
                s.Depth = o.Depth;
        }
    }

    public AnimInfo? FindAnim(string name) =>
        _anims.TryGetValue(name, out var a) ? a : _commonAnims.TryGetValue(name, out a) ? a : null;

    /// <summary>Starts an animation (loops -1 = forever). Returns its task, or null if unknown.</summary>
    public AnimTask? PlayAnim(string name, int loops = 1, TaskHandle? after = null, int delay = 0,
        string? endObject = null, bool endObjectOn = false)
    {
        var a = FindAnim(name);
        if (a == null)
            return null;
        return Tasks.Add(new AnimTask(a, loops) { WaitFor = after, Delay = delay, EndObject = endObject, EndObjectOn = endObjectOn });
    }

    /// <summary>Stops a running animation: hides its sprite and applies its end-object switch.</summary>
    public void StopAnim(string name, bool onlyStarted = false)
    {
        foreach (var t in Tasks.OfType<AnimTask>().ToList())
        {
            if (!t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                continue;
            if (onlyStarted && (t.WaitFor != null || t.Delay != 0))
                continue;
            var s = Sprites.For(t.Anim.Resource);
            if (s != null)
                s.Visible = false;
            if (t.EndObject != null)
                ShowObject(t.EndObject, t.EndObjectOn);
            Tasks.Remove(t);
            return;
        }
    }

    public void SetAnimPosition(string name, int x, int y)
    {
        var a = FindAnim(name);
        if (a != null)
            (a.X, a.Y) = (x, y);
    }

    /// <summary>Switches an object on or off, in the location and in the persistent list.</summary>
    public bool ShowObject(string name, bool on)
    {
        bool found = false;
        if (Scene != null)
        {
            var o = Scene.Script.Objects.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (o != null)
            {
                Scene.ObjectVisible[o.Name] = on;
                if (o.Gfx >= 0 && o.Gfx < Scene.Script.Resources.Count && Sprites.For(Scene.Script.Resources[o.Gfx]) is { } s)
                    s.Visible = on;
                found = true;
            }
        }
        if (GlobalObjects.ContainsKey(name))
        {
            GlobalObjects[name] = on;
            found = true;
        }
        Hooks?.ObjectChanged(this, name, on);
        return found;
    }

    /// <summary>Moves an object (its picture and click area) so that its anchor lands at (x, y).</summary>
    public void MoveObject(string name, int x, int y)
    {
        var o = Scene?.Script.Objects.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (o == null)
            return;
        (int ax, int ay) = o.Gfx >= 0 ? (o.GfxX, o.GfxY) : o.Click.Length > 0 ? o.Click[0] : (0, 0);
        int dx = x - ax, dy = y - ay;
        if (o.Gfx >= 0)
        {
            o.GfxX += dx;
            o.GfxY += dy;
            if (Sprites.For(Scene!.Script.Resources[o.Gfx]) is { } s)
            {
                s.X += dx;
                s.Y += dy;
            }
        }
        o.Click = o.Click.Select(p => (p.X + dx, p.Y + dy)).ToArray();
    }

    /// <summary>Sets the drawing depth of a loaded image resource (path fragment match).</summary>
    public void SetResourceDepth(string pathPart, int depth)
    {
        if (Sprites.FindByPath(pathPart) is { } s)
            s.Depth = depth;
    }

    internal void OnAnimFrame(AnimTask t) { }

    /// <summary>Advances the game by one tick (1/10 s).</summary>
    public void Update()
    {
        Tick++;
        Tasks.Tick(this);
    }

    public void Draw(Framebuffer fb)
    {
        if (Scene == null)
        {
            fb.Clear();
            return;
        }
        Scene.DrawBackground(fb);
        Sprites.Draw(fb);
    }
}
