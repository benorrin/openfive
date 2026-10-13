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

    /// <summary>The characters, in the original's order: Dick, George, Julian, Anne, Timmy, Quentin.</summary>
    public IReadOnlyList<Actor> Actors { get; }
    public Actor Dick => Actors[0];
    public Actor George => Actors[1];
    public Actor Julian => Actors[2];
    public Actor Anne => Actors[3];
    public Actor Timmy => Actors[4];
    public Actor Quentin => Actors[5];
    /// <summary>The character the player controls.</summary>
    public Actor Hero { get; set; }

    public WalkGraph? Walk { get; private set; }
    readonly int?[] _regionDepth = new int?[256];

    public GameWorld(GameData data, GameAudio audio)
    {
        Data = data;
        Audio = audio;
        Sprites = new SpriteBank(data);
        Font = new BitmapFont(data.Image("bmp/common/misc/char.brg"));
        Dialogue = new DialogueSystem(this);
        Actors = new[]
        {
            new Actor("Dick", "aDick_"), new Actor("George", "aGeorge_"), new Actor("Julian", "aJulian_"),
            new Actor("Anne", "aAnne_"), new Actor("Timmy", "aTimmy_"), new Actor("Quentin", "aQuentin_"),
        };
        Hero = Actors[0];
        _common = data.Location("common");
        foreach (var a in _common.Anims)
            if (a.Resource >= 0 && a.Resource < _common.Resources.Count)
                _commonAnims[a.Name] = new AnimInfo { Def = a, Resource = _common.Resources[a.Resource], X = a.X, Y = a.Y };
        LoadGlobalObjects();
        foreach (var o in _common.Objects)
        {
            CommonObjectVisible[o.Name] = o.On;
            if (o.Gfx >= 0 && o.Gfx < _common.Resources.Count && Sprites.For(_common.Resources[o.Gfx]) is { } sp)
            {
                sp.Visible = o.On;
                sp.X = o.GfxX;
                sp.Y = o.GfxY;
                if (o.Depth != 0)
                    sp.Depth = o.Depth;
            }
        }
    }

    /// <summary>On/off state of the objects shared by all locations (dialogue box, rucksack...).</summary>
    public Dictionary<string, bool> CommonObjectVisible { get; } = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyList<ObjectDef> CommonObjects => _common.Objects;
    public ResScript Common => _common;

    /// <summary>The end of the sequence being built (scripts chain new steps after it).</summary>
    public TaskHandle? Chain { get; set; }

    /// <summary>Is the mouse pointer shown (the game hides it during scenes)?</summary>
    public bool CursorVisible { get; set; } = true;
    /// <summary>Set when a task moves the pointer; the platform warps the mouse there.</summary>
    public (int X, int Y)? CursorWarp { get; set; }

    /// <summary>Texts currently on screen (speech subtitles, labels, menu options).</summary>
    public List<TextItem> Texts { get; } = new();

    /// <summary>Story position: the chapter ("quest") and the step within it ("episode").</summary>
    public int Quest { get; set; } = 1;
    public int Episode { get; set; } = 1;

    public DialogueSystem Dialogue { get; }

    /// <summary>The game's font (read from the disc).</summary>
    public BitmapFont Font { get; }

    /// <summary>Show the dialogue box and talking heads (option); off shows the text only.</summary>
    public bool DialogueBox { get; set; } = true;

    /// <summary>Set for one tick when the player asks to skip the line being spoken.</summary>
    public bool SkipRequested { get; set; }

    /// <summary>Back to the state of a new game: objects, story step, characters.</summary>
    public void ResetState()
    {
        GlobalObjects.Clear();
        LoadGlobalObjects();
        Dialogue.AskedOptions.Clear();
        Quest = 1;
        Episode = 1;
        foreach (var a in Actors)
        {
            a.Visible = false;
            a.Location = "";
            a.Follower = null;
            a.ClearQueue();
        }
        Hero = Dick;
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
        Walk = new WalkGraph(Scene.Script);
        Array.Clear(_regionDepth);
        if (Scene.Mask != null)
            foreach (var m in Scene.Script.MaskSamples)
                if (m.X >= 0 && m.Y >= 0 && m.X < Framebuffer.Width && m.Y < Framebuffer.Height)
                    _regionDepth[Scene.Mask[m.Y * Framebuffer.Width + m.X]] = m.Depth;
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

    public void SetAnimScale(string name, double scale)
    {
        var a = FindAnim(name);
        if (a != null)
            a.Scale = scale;
    }

    /// <summary>Finds an object of the location or of the common set.</summary>
    public ObjectDef? FindObject(string name) =>
        Scene?.Script.Objects.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ??
        _common.Objects.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public bool IsObjectOn(string name)
    {
        if (Scene != null && Scene.Script.Objects.Any(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            return Scene.ObjectVisible.TryGetValue(name, out bool v) && v;
        return CommonObjectVisible.TryGetValue(name, out bool c) ? c : GlobalObjects.TryGetValue(name, out bool g) && g;
    }

    /// <summary>Switches an object on or off, in the location and in the persistent list.</summary>
    public bool ShowObject(string name, bool on)
    {
        bool found = false;
        var common = _common.Objects.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (common != null)
        {
            CommonObjectVisible[common.Name] = on;
            if (common.Gfx >= 0 && common.Gfx < _common.Resources.Count && Sprites.For(_common.Resources[common.Gfx]) is { } cs)
                cs.Visible = on;
            found = true;
        }
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
        var o = FindObject(name);
        if (o == null)
            return;
        var res = Scene != null && Scene.Script.Objects.Contains(o) ? Scene.Script : _common;
        (int ax, int ay) = o.Gfx >= 0 ? (o.GfxX, o.GfxY) : o.Click.Length > 0 ? o.Click[0] : (0, 0);
        int dx = x - ax, dy = y - ay;
        if (o.Gfx >= 0)
        {
            o.GfxX += dx;
            o.GfxY += dy;
            if (o.Gfx < res.Resources.Count && Sprites.For(res.Resources[o.Gfx]) is { } s)
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

    // ---- characters ----------------------------------------------------------------------

    /// <summary>
    /// Puts a character into the current location at the walkable point nearest (x, y), facing
    /// <paramref name="state"/>, and makes it visible.
    /// </summary>
    public void PlaceActor(Actor a, int x, int y, int state)
    {
        a.ClearQueue();
        a.Location = Scene?.Name ?? "";
        var near = Walk?.Nearest(x, y);
        if (near is { } n)
        {
            a.Shape = n.Shape;
            bool on = Walk!.Contains(n.Shape, x, y);
            (a.X, a.Y) = on ? (x, y) : (n.X, n.Y);
        }
        else
        {
            a.Shape = -1;
            (a.X, a.Y) = (x, y);
        }
        a.State = state;
        a.Visible = true;
        a._animName = "";
        UpdatePerspective(a);
    }

    public void HideActor(Actor a)
    {
        a.Visible = false;
        a.ClearQueue();
    }

    void UpdatePerspective(Actor a)
    {
        if (Walk == null || !Walk.Valid(a.Shape))
            return;
        a.Scale = Walk.ScaleAt(a.Shape, a.IX, a.IY);
        a.Depth = Walk.DepthAt(a.Shape, a.IX, a.IY);
    }

    /// <summary>Adds the walk from the character's position to the walkable point nearest (x, y).</summary>
    List<WalkStep> PlanWalk(Actor a, int x, int y)
    {
        var steps = new List<WalkStep>();
        if (Walk == null || Walk.Nearest(x, y) is not { } dest)
            return steps;
        int from = a.Shape;
        if (!Walk.Valid(from) && Walk.Nearest(a.IX, a.IY) is { } here)
            from = here.Shape;
        bool exact = Walk.Contains(dest.Shape, x, y);
        int tx = exact ? x : dest.X, ty = exact ? y : dest.Y;
        foreach (var leg in Walk.Route(from, a.IX, a.IY, dest.Shape, tx, ty))
            steps.Add(new WalkStep { X = leg.X, Y = leg.Y, Shape = leg.Shape });
        return steps;
    }

    /// <summary>The player's order: drop what the character was doing and walk to (x, y).</summary>
    public TaskHandle? WalkTo(Actor a, int x, int y)
    {
        a.ClearQueue();
        ActorStep? last = null;
        foreach (var s in PlanWalk(a, x, y))
            last = a.Enqueue(s);
        return last;
    }

    /// <summary>Queues a walk to (x, y) that starts when <paramref name="after"/> is released; the route is planned then.</summary>
    public TaskHandle WalkToAfter(Actor a, int x, int y, TaskHandle? after = null, int delay = 0) =>
        a.Enqueue(new RouteStep { X = x, Y = y, WaitFor = after, Delay = delay });

    /// <summary>Queues a state change (turn, stand, take, talk...).</summary>
    public TaskHandle SetState(Actor a, int state, TaskHandle? after = null, int delay = 0) =>
        a.Enqueue(new StateStep { State = state, WaitFor = after, Delay = delay });

    /// <summary>Queues an action to run when the character gets to this point of its queue.</summary>
    public TaskHandle QueueAction(Actor a, Action<GameWorld> action) => a.Enqueue(new ActionStep { Action = action });

    void UpdateActor(Actor a)
    {
        // Process the queue: finished steps are removed, at most one step acts per tick.
        for (int guard = 0; guard < 64 && a.Queue.First is { } node; guard++)
        {
            var step = node.Value;
            step.Started = true;
            if (step.WaitFor != null)
            {
                if (!step.WaitFor.Released)
                    break;
                step.WaitFor = null;
            }
            if (step.Delay > 0)
            {
                step.Delay--;
                break;
            }
            bool done;
            switch (step)
            {
                case WalkStep w:
                    done = Advance(a, w);
                    if (a.Follower != null)
                        Follow(a, a.Follower);
                    break;
                case StateStep st:
                    a.State = st.State;
                    done = true;
                    break;
                case RouteStep r:
                {
                    var after = node;
                    foreach (var ws in PlanWalk(a, r.X, r.Y))
                        after = a.Queue.AddAfter(after, ws);
                    done = true;
                    break;
                }
                case ActionStep act:
                    act.Action(this);
                    done = true;
                    break;
                default:
                    done = true;
                    break;
            }
            if (!done)
                break;
            step.Completed = true;
            a.Queue.RemoveFirst();
            if (step is WalkStep && a.Queue.First?.Value is not WalkStep)
            {
                a.State = ActorStates.StandFor(a.State);
                break;
            }
            if (step is WalkStep)
                continue;
        }
        UpdateDisplay(a);
    }

    /// <summary>
    /// Keeps a follower close: once it is more than 100 pixels (scaled by the nearer character's
    /// size) away from its leader on either axis, it is sent walking to where the leader is.
    /// </summary>
    void Follow(Actor leader, Actor f)
    {
        if (!f.Visible || !f.Location.Equals(leader.Location, StringComparison.OrdinalIgnoreCase))
            return;
        double s = leader.Y <= f.Y ? f.Scale : leader.Scale;
        double limit = 100 * s;
        if (Math.Abs(leader.IX - f.IX) <= limit && Math.Abs(leader.IY - f.IY) <= limit)
            return;
        if (f.Queue.LastOrDefault() is WalkStep last && Math.Abs(last.X - leader.IX) < 20 && Math.Abs(last.Y - leader.IY) < 20)
            return;
        WalkTo(f, leader.IX, leader.IY);
    }

    /// <summary>Moves the character one tick along a walk step; true when it stands on the end point.</summary>
    bool Advance(Actor a, WalkStep w)
    {
        if (a.IX == w.X && a.IY == w.Y)
        {
            a.Shape = w.Shape;
            UpdatePerspective(a);
            return true;
        }
        a.State = ActorStates.Direction(a.IX, a.IY, w.X, w.Y, a.State);
        a.StepTowards(w.X, w.Y);
        UpdatePerspective(a);
        if (a.IX == w.X && a.IY == w.Y)
        {
            a.Shape = w.Shape;
            UpdatePerspective(a);
        }
        return false;
    }

    void UpdateDisplay(Actor a)
    {
        var sprite = a.Sprite;
        sprite.Visible = a.Visible && a.Location.Equals(Scene?.Name, StringComparison.OrdinalIgnoreCase);
        if (!sprite.Visible)
            return;
        int state = Math.Clamp(a.State, 0, ActorStates.Suffix.Length - 1);
        string name = a.AnimPrefix + ActorStates.Suffix[state];
        var anim = FindAnim(name);
        if (anim == null || anim.Def.Frames.Length == 0)
        {
            sprite.Visible = false;
            return;
        }
        if (!name.Equals(a._animName, StringComparison.OrdinalIgnoreCase))
        {
            a._animName = name;
            a._frame = 0;
        }
        else if (ActorStates.IsWalk(state))
            a._frame = (a._frame + 1) % anim.Def.Frames.Length;
        else if (ActorStates.IsStand(state))
            a._frame = 0;
        else if (a._frame < anim.Def.Frames.Length - 1)
            a._frame++;
        else if (ActorStates.FollowUp.TryGetValue(state, out int next))
        {
            a.State = next;
            UpdateDisplay(a);
            return;
        }
        else
            a._frame = 0;
        var f = anim.Def.Frames[a._frame];
        if (sprite.Resource != anim.Resource)
        {
            sprite.Resource = anim.Resource;
            sprite.Packed = anim.Resource.Packed || Data.IsPacked(anim.Resource.Path);
        }
        sprite.Frame = anim.Def.X + f.Frame;
        // Some character sheets (standing poses, for example) are stored at twice the resolution
        // of the walking sheets; they are drawn at half size so all poses match.
        double sheetFactor = 1;
        if (sprite.Packed && Data.Packed(anim.Resource.Path).FrameHeight > 300)
            sheetFactor = 0.5;
        sprite.Scale = a.Scale * sheetFactor;
        sprite.X = (int)Math.Round(a.X - f.X * a.Scale * 0.5);
        sprite.Y = (int)Math.Round(a.Y - f.Y * a.Scale);
        sprite.Depth = a.Depth;
        sprite.MaskDepth = a.Depth;
        if (!sprite.Packed)
        {
            sprite.SrcX = anim.Def.Width * (anim.Def.X + f.Frame);
            sprite.SrcW = anim.Def.Width;
            sprite.SrcH = anim.Def.Height;
        }
    }

    int _idle = 200;

    /// <summary>
    /// After the hero has stood still for 20 seconds without the pointer moving, they put their
    /// hands in their pockets; ten seconds later they take them out again.
    /// </summary>
    public void UpdateIdle(bool pointerMoved, bool busy)
    {
        var h = Hero;
        if (pointerMoved || busy || h.Queue.Count > 0 || !h.Visible || !CursorVisible)
        {
            _idle = 200;
            return;
        }
        if (--_idle > 0)
            return;
        _idle = 100;
        h.State = h.State switch
        {
            ActorStates.StandDownLeft or ActorStates.StandLeft => 57,
            ActorStates.StandDownRight or ActorStates.StandRight => 58,
            60 => 116,
            59 => 115,
            _ => h.State,
        };
    }

    /// <summary>Advances the game by one tick (1/10 s).</summary>
    public void Update()
    {
        Tick++;
        if (SkipRequested)
            Audio.Mixer.StopSpeech();
        Tasks.Tick(this);
        SkipRequested = false;
        foreach (var a in Actors)
            UpdateActor(a);
    }

    public void Draw(Framebuffer fb)
    {
        if (Scene == null)
        {
            fb.Clear();
            return;
        }
        Scene.DrawBackground(fb);
        var mask = Scene.Mask;
        Sprites.Draw(fb, Actors.Select(a => a.Sprite), s =>
        {
            if (s.MaskDepth is not { } d || mask == null)
                return null;
            return (x, y) => _regionDepth[mask[y * Framebuffer.Width + x]] is { } rd && rd < d;
        });
        foreach (var t in Texts.ToList())
            DrawText(fb, t);
    }

    /// <summary>Line spacing of wrapped texts.</summary>
    public const int LineHeight = 22;

    public void DrawText(Framebuffer fb, TextItem t)
    {
        int y = t.Y;
        foreach (string line in Font.Wrap(t.Text, Math.Max(40, t.Right - t.X)))
        {
            Font.Draw(fb, line, t.X, y);
            y += LineHeight;
        }
    }
}
