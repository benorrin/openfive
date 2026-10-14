using System.Text;
using OpenFive.Engine.Audio;
using OpenFive.Engine.Formats;
using OpenFive.Engine.Graphics;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.World;

/// <summary>Notified by the world when something a location script cares about happens.</summary>
public interface ILocationHooks
{
    /// <summary>An object was switched on or off (by a script, an animation or a task).</summary>
    void ObjectChanged(GameWorld world, string name, bool on) { }
}

/// <summary>
/// The running game: the current location, its sprites and objects, the characters, the task
/// list, and the state that persists between locations (object switches, story position).
/// </summary>
public sealed class GameWorld
{
    /// <summary>Line spacing of wrapped texts.</summary>
    public const int LineHeight = 22;

    readonly ResScript _common;
    readonly Dictionary<string, AnimInfo> _anims = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, AnimInfo> _commonAnims = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The depth of each region of the location's depth mask (null: not a region).</summary>
    readonly int?[] _regionDepth = new int?[256];

    /// <summary>Ticks left before the idle hero puts their hands in their pockets.</summary>
    int _idle = IdleDelay;

    const int IdleDelay = 200, IdleRepeat = 100;

    /// <summary>Loads the shared resources (font, common objects and animations) and creates the characters.</summary>
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
        Hero = Dick;
        _common = data.Location("common");
        AddAnims(_commonAnims, _common);
        LoadGlobalObjects();
        foreach (var o in _common.Objects)
        {
            CommonObjectVisible[o.Name] = o.On;
            PlaceObjectSprite(_common, o, o.On);
        }
    }

    // ---- shared state ------------------------------------------------------------------------

    /// <summary>The game files.</summary>
    public GameData Data { get; }

    /// <summary>Music, speech and sound effects.</summary>
    public GameAudio Audio { get; }

    /// <summary>The sprites of the common set and the current location.</summary>
    public SpriteBank Sprites { get; }

    /// <summary>The running tasks (animations, lines of dialogue, timed actions).</summary>
    public TaskScheduler Tasks { get; } = new();

    /// <summary>The location being shown (null before the first one is loaded).</summary>
    public Scene? Scene { get; private set; }

    /// <summary>The current location's script, told about object changes.</summary>
    public ILocationHooks? Hooks { get; set; }

    /// <summary>The conversation system.</summary>
    public DialogueSystem Dialogue { get; }

    /// <summary>The game's font (read from the disc).</summary>
    public BitmapFont Font { get; }

    /// <summary>Persistent on/off state of named objects (initialised from "object.res").</summary>
    public Dictionary<string, bool> GlobalObjects { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>On/off state of the objects shared by all locations (dialogue box, rucksack...).</summary>
    public Dictionary<string, bool> CommonObjectVisible { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The objects shared by all locations.</summary>
    public IReadOnlyList<ObjectDef> CommonObjects => _common.Objects;

    /// <summary>The ".res" description of the shared objects and animations.</summary>
    public ResScript Common => _common;

    /// <summary>Game ticks since start (10 per second).</summary>
    public long Tick { get; private set; }

    /// <summary>Story position: the chapter ("quest").</summary>
    public int Quest { get; set; } = 1;

    /// <summary>Story position: the step within the chapter ("episode").</summary>
    public int Episode { get; set; } = 1;

    /// <summary>The end of the sequence being built (scripts chain new steps after it).</summary>
    public TaskHandle? Chain { get; set; }

    /// <summary>Is the mouse pointer shown (the game hides it during scenes)?</summary>
    public bool CursorVisible { get; set; } = true;

    /// <summary>Set when a task moves the pointer; the platform warps the mouse there.</summary>
    public (int X, int Y)? CursorWarp { get; set; }

    /// <summary>Texts currently on screen (speech subtitles, labels, menu options).</summary>
    public List<TextItem> Texts { get; } = new();

    /// <summary>Show the dialogue box and talking heads (option); off shows the text only.</summary>
    public bool DialogueBox { get; set; } = true;

    /// <summary>Characters cast shadows (an option).</summary>
    public bool Shadows { get; set; } = true;

    /// <summary>Set for one tick when the player asks to skip the line being spoken.</summary>
    public bool SkipRequested { get; set; }

    /// <summary>The current location's per-tick routine (run after the characters move).</summary>
    public Action? LocationTick;

    // ---- characters ----------------------------------------------------------------------

    /// <summary>The characters, in the original's order: Dick, George, Julian, Anne, Timmy, Quentin.</summary>
    public IReadOnlyList<Actor> Actors { get; }

    /// <summary>Dick.</summary>
    public Actor Dick => Actors[0];

    /// <summary>George.</summary>
    public Actor George => Actors[1];

    /// <summary>Julian.</summary>
    public Actor Julian => Actors[2];

    /// <summary>Anne.</summary>
    public Actor Anne => Actors[3];

    /// <summary>Timmy the dog.</summary>
    public Actor Timmy => Actors[4];

    /// <summary>Uncle Quentin.</summary>
    public Actor Quentin => Actors[5];

    /// <summary>The character the player controls.</summary>
    public Actor Hero { get; set; }

    /// <summary>The walkable paths and areas of the current location.</summary>
    public WalkGraph? Walk { get; private set; }

    // ---- loading -----------------------------------------------------------------------------

    /// <summary>Back to the state of a new game: objects, story step, characters.</summary>
    public void ResetState()
    {
        GlobalObjects.Clear();
        LoadGlobalObjects();
        Dialogue.AskedOptions.Clear();
        Dialogue.CurrentSpeaker = -1;
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

    /// <summary>Unloads the current location and loads another one.</summary>
    public void LoadLocation(string name)
    {
        if (Scene != null)
            Sprites.Remove(Scene.Script.Resources);
        _anims.Clear();
        Data.TrimCaches();
        Scene = new Scene(Data, name);
        Walk = new WalkGraph(Scene.Script);
        LoadRegionDepths(Scene);
        AddAnims(_anims, Scene.Script);
        foreach (var o in Scene.Script.Objects)
        {
            if (GlobalObjects.TryGetValue(o.Name, out bool on))
                Scene.ObjectVisible[o.Name] = on;
            PlaceObjectSprite(Scene.Script, o, Scene.IsVisible(o));
        }
    }

    /// <summary>Reads the default on/off state of the persistent objects ("NAME ON|OFF" lines of "object.res").</summary>
    void LoadGlobalObjects()
    {
        if (!Data.Exists("object.res"))
            return;
        foreach (string raw in Encoding.Latin1.GetString(Data.Read("object.res")).Split('\n'))
        {
            string[] p = raw.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (p.Length >= 2)
                GlobalObjects[p[0]] = p[1].Equals("ON", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>Works out the depth of each mask region from the location's sample points.</summary>
    void LoadRegionDepths(Scene scene)
    {
        Array.Clear(_regionDepth);
        if (scene.Mask == null)
            return;
        foreach (var m in scene.Script.MaskSamples)
            if (m.X >= 0 && m.Y >= 0 && m.X < Framebuffer.Width && m.Y < Framebuffer.Height)
                _regionDepth[scene.Mask[m.Y * Framebuffer.Width + m.X]] = m.Depth;
    }

    /// <summary>Registers the animations of a ".res" file whose picture exists.</summary>
    static void AddAnims(Dictionary<string, AnimInfo> into, ResScript res)
    {
        foreach (var a in res.Anims)
            if (a.Resource >= 0 && a.Resource < res.Resources.Count)
                into[a.Name] = new AnimInfo { Def = a, Resource = res.Resources[a.Resource], X = a.X, Y = a.Y };
    }

    // ---- objects -----------------------------------------------------------------------------

    /// <summary>Finds an object of the location or of the common set.</summary>
    public ObjectDef? FindObject(string name) => (Scene != null ? Find(Scene.Script, name) : null) ?? Find(_common, name);

    /// <summary>Is the object switched on (location objects, then common ones, then the persistent list)?</summary>
    public bool IsObjectOn(string name)
    {
        if (Scene != null && Find(Scene.Script, name) != null)
            return Scene.ObjectVisible.TryGetValue(name, out bool v) && v;
        return CommonObjectVisible.TryGetValue(name, out bool c) ? c : GlobalObjects.TryGetValue(name, out bool g) && g;
    }

    /// <summary>
    /// Switches an object on or off, in the common set, the location and the persistent list
    /// (wherever it exists), and tells the location script. Returns false if it exists nowhere.
    /// </summary>
    public bool ShowObject(string name, bool on)
    {
        bool found = false;
        if (Find(_common, name) is { } common)
        {
            CommonObjectVisible[common.Name] = on;
            if (ObjectSprite(_common, common) is { } sprite)
                sprite.Visible = on;
            found = true;
        }
        if (Scene != null && Find(Scene.Script, name) is { } o)
        {
            Scene.ObjectVisible[o.Name] = on;
            if (ObjectSprite(Scene.Script, o) is { } sprite)
                sprite.Visible = on;
            found = true;
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
        // The anchor is the picture's position, or else the first corner of the click area.
        (int ax, int ay) = o.Gfx >= 0 ? (o.GfxX, o.GfxY) : o.Click.Length > 0 ? o.Click[0] : (0, 0);
        int dx = x - ax, dy = y - ay;
        if (o.Gfx >= 0)
        {
            o.GfxX += dx;
            o.GfxY += dy;
            if (ObjectSprite(res, o) is { } s)
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

    static ObjectDef? Find(ResScript res, string name) =>
        res.Objects.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>The sprite showing an object's picture, or null if it has none.</summary>
    Sprite? ObjectSprite(ResScript res, ObjectDef o) =>
        o.Gfx >= 0 && o.Gfx < res.Resources.Count ? Sprites.For(res.Resources[o.Gfx]) : null;

    /// <summary>Puts an object's picture where the ".res" file says, shown or hidden.</summary>
    void PlaceObjectSprite(ResScript res, ObjectDef o, bool visible)
    {
        if (ObjectSprite(res, o) is not { } s)
            return;
        s.Visible = visible;
        s.X = o.GfxX;
        s.Y = o.GfxY;
        if (o.Depth != 0)
            s.Depth = o.Depth;
    }

    // ---- animations --------------------------------------------------------------------------

    /// <summary>Finds an animation of the location, or else of the common set.</summary>
    public AnimInfo? FindAnim(string name) =>
        _anims.TryGetValue(name, out var a) ? a : _commonAnims.GetValueOrDefault(name);

    /// <summary>
    /// Starts an animation (<paramref name="loops"/> -1 = forever) once <paramref name="after"/>
    /// is released. Returns its task, or null if the animation is unknown.
    /// </summary>
    public AnimTask? PlayAnim(string name, int loops = 1, TaskHandle? after = null, int delay = 0,
        string? endObject = null, bool endObjectOn = false)
    {
        if (FindAnim(name) is not { } anim)
            return null;
        return Tasks.Add(new AnimTask(anim, loops)
        {
            WaitFor = after,
            Delay = delay,
            EndObject = endObject,
            EndObjectOn = endObjectOn,
        });
    }

    /// <summary>
    /// Stops the first running animation of that name: hides its sprite and applies its
    /// end-object switch. With <paramref name="onlyStarted"/>, animations still waiting are left alone.
    /// </summary>
    public void StopAnim(string name, bool onlyStarted = false)
    {
        var task = Tasks.OfType<AnimTask>().FirstOrDefault(t =>
            t.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !(onlyStarted && (t.WaitFor != null || t.Delay != 0)));
        if (task == null)
            return;
        if (Sprites.For(task.Anim.Resource) is { } s)
            s.Visible = false;
        if (task.EndObject != null)
            ShowObject(task.EndObject, task.EndObjectOn);
        Tasks.Remove(task);
    }

    /// <summary>Moves an animation.</summary>
    public void SetAnimPosition(string name, int x, int y)
    {
        if (FindAnim(name) is { } a)
            (a.X, a.Y) = (x, y);
    }

    /// <summary>Sets the size an animation is drawn at.</summary>
    public void SetAnimScale(string name, double scale)
    {
        if (FindAnim(name) is { } a)
            a.Scale = scale;
    }

    /// <summary>Is the animation on screen (its sprite shown by a task that has started)?</summary>
    public bool IsAnimShown(string name) =>
        FindAnim(name) is { } a && Sprites.For(a.Resource) is { Visible: true } &&
        Tasks.OfType<AnimTask>().Any(t => t.Anim == a && t.WaitFor == null);

    // ---- character orders ----------------------------------------------------------------

    /// <summary>
    /// Puts a character into the current location at the walkable point nearest (x, y), facing
    /// <paramref name="state"/>, and makes it visible. Like the original, this leaves the
    /// character's queue alone: scripts often queue a walk that starts from the new spot.
    /// </summary>
    public void PlaceActor(Actor a, int x, int y, int state)
    {
        a.Location = Scene?.Name ?? "";
        if (Walk?.Nearest(x, y) is { } n)
        {
            a.Shape = n.Shape;
            (a.X, a.Y) = Walk.Contains(n.Shape, x, y) ? (x, y) : (n.X, n.Y);
        }
        else
        {
            a.Shape = -1;
            (a.X, a.Y) = (x, y);
        }
        a.State = state;
        a.Visible = true;
        a.AnimName = "";
        UpdatePerspective(a);
    }

    /// <summary>
    /// Hides a character. Its queue keeps running (a location change queued on the hero still
    /// happens), as in the original.
    /// </summary>
    public void HideActor(Actor a) => a.Visible = false;

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

    /// <summary>The walk legs from the character's position to the walkable point nearest (x, y).</summary>
    List<WalkStep> PlanWalk(Actor a, int x, int y)
    {
        if (Walk == null || Walk.Nearest(x, y) is not { } dest)
            return new List<WalkStep>();
        int from = a.Shape;
        if (!Walk.Valid(from) && Walk.Nearest(a.IX, a.IY) is { } here)
            from = here.Shape;
        bool exact = Walk.Contains(dest.Shape, x, y);
        int tx = exact ? x : dest.X, ty = exact ? y : dest.Y;
        return Walk.Route(from, a.IX, a.IY, dest.Shape, tx, ty)
            .Select(leg => new WalkStep { X = leg.X, Y = leg.Y, Shape = leg.Shape })
            .ToList();
    }

    // ---- the game tick -------------------------------------------------------------------

    /// <summary>Advances the game by one tick (1/10 s).</summary>
    public void Update()
    {
        Tick++;
        if (SkipRequested)
            Audio.Mixer.StopSpeech();
        // As in the original: the characters' queues first, then the location's own tick
        // (it sees what the characters are doing now), then the other tasks.
        foreach (var a in Actors)
            UpdateActor(a);
        LocationTick?.Invoke();
        Tasks.Tick(this);
        SkipRequested = false;
    }

    /// <summary>
    /// After the hero has stood still for 20 seconds without the pointer moving, they put their
    /// hands in their pockets; ten seconds later they take them out again.
    /// </summary>
    public void UpdateIdle(bool pointerMoved, bool busy)
    {
        var h = Hero;
        if (pointerMoved || busy || h.Queue.Count > 0 || !h.Visible || !CursorVisible)
        {
            _idle = IdleDelay;
            return;
        }
        if (--_idle > 0)
            return;
        _idle = IdleRepeat;
        h.State = h.State switch
        {
            StandDownLeft or StandLeft => HandsInPocketsLeft,
            StandDownRight or StandRight => HandsInPocketsRight,
            HandsInPocketsStillRight => HandsOutOfPocketsRight,
            HandsInPocketsStillLeft => HandsOutOfPocketsLeft,
            _ => h.State,
        };
    }

    /// <summary>
    /// Runs the character's queue for one tick, then updates its picture. Finished steps are
    /// removed; as in the original, one step is handled per tick (a pose set by a state step is
    /// seen for at least a tick), except that the legs of a walk and route planning carry on.
    /// </summary>
    void UpdateActor(Actor a)
    {
        for (int guard = 0; guard < 64 && a.Queue.First is { } node; guard++)
        {
            var step = node.Value;
            step.Started = true;
            if (!IsDue(step) || !RunStep(a, node))
                break;
            step.Completed = true;
            a.Queue.RemoveFirst();
            if (step is WalkStep && a.Queue.First?.Value is not WalkStep)
            {
                a.State = StandFor(a.State);
                break;
            }
            if (step is not (WalkStep or RouteStep))
                break;
        }
        UpdateDisplay(a);
    }

    /// <summary>Has the step's wait ended and its delay run out? (Counts the delay down.)</summary>
    static bool IsDue(ActorStep step)
    {
        if (step.WaitFor != null)
        {
            if (!step.WaitFor.Released)
                return false;
            step.WaitFor = null;
        }
        if (step.Delay > 0)
        {
            step.Delay--;
            return false;
        }
        return true;
    }

    /// <summary>Carries out the first step of the character's queue; true when it is finished.</summary>
    bool RunStep(Actor a, LinkedListNode<ActorStep> node)
    {
        switch (node.Value)
        {
            case WalkStep w:
                bool arrived = Advance(a, w);
                if (a.Follower != null)
                    Follow(a, a.Follower);
                return arrived;
            case StateStep s:
                a.State = s.State;
                return true;
            case RouteStep r:
                // The route is planned now and its legs inserted after this step.
                var after = node;
                foreach (var leg in PlanWalk(a, r.X, r.Y))
                    after = a.Queue.AddAfter(after, leg);
                return true;
            case ActionStep act:
                act.Action(this);
                return true;
            default:
                return true;
        }
    }

    /// <summary>
    /// Keeps a follower close: once it is more than 100 pixels (scaled by the nearer character's
    /// size) away from its leader on either axis, it is sent walking to where the leader is.
    /// </summary>
    void Follow(Actor leader, Actor f)
    {
        if (!f.Visible || !f.Location.Equals(leader.Location, StringComparison.OrdinalIgnoreCase))
            return;
        double limit = 100 * (leader.Y <= f.Y ? f.Scale : leader.Scale);
        if (Math.Abs(leader.IX - f.IX) <= limit && Math.Abs(leader.IY - f.IY) <= limit)
            return;
        // Already on its way there.
        if (f.Queue.LastOrDefault() is WalkStep last && Math.Abs(last.X - leader.IX) < 20 && Math.Abs(last.Y - leader.IY) < 20)
            return;
        WalkTo(f, leader.IX, leader.IY);
    }

    /// <summary>Moves the character one tick along a walk step; true when it already stood on the end point.</summary>
    bool Advance(Actor a, WalkStep w)
    {
        if (a.IX == w.X && a.IY == w.Y)
        {
            a.Shape = w.Shape;
            UpdatePerspective(a);
            return true;
        }
        a.State = Direction(a.IX, a.IY, w.X, w.Y, a.State);
        a.StepTowards(w.X, w.Y);
        UpdatePerspective(a);
        if (a.IX == w.X && a.IY == w.Y)
        {
            a.Shape = w.Shape;
            UpdatePerspective(a);
        }
        return false;
    }

    /// <summary>Sets the character's size and depth from where it stands on its path or area.</summary>
    void UpdatePerspective(Actor a)
    {
        if (Walk == null || !Walk.Valid(a.Shape))
            return;
        a.Scale = Walk.ScaleAt(a.Shape, a.IX, a.IY);
        a.Depth = Walk.DepthAt(a.Shape, a.IX, a.IY);
    }

    /// <summary>
    /// Updates the character's sprite: picks the animation for its state and the next frame.
    /// Walks loop, stands show their first frame, actions play once and then change to their
    /// follow-up state (or loop when they have none).
    /// </summary>
    void UpdateDisplay(Actor a)
    {
        var sprite = a.Sprite;
        sprite.Visible = a.Visible && a.Location.Equals(Scene?.Name, StringComparison.OrdinalIgnoreCase);
        if (!sprite.Visible)
            return;
        int state = Math.Clamp(a.State, 0, Suffix.Length - 1);
        string name = a.AnimPrefix + Suffix[state];
        var anim = FindAnim(name);
        if (anim == null || anim.Def.Frames.Length == 0)
        {
            sprite.Visible = false;
            return;
        }
        int frames = anim.Def.Frames.Length;
        if (!name.Equals(a.AnimName, StringComparison.OrdinalIgnoreCase))
        {
            a.AnimName = name;
            a.Frame = 0;
        }
        else if (IsWalk(state))
            a.Frame = (a.Frame + 1) % frames;
        else if (IsStand(state))
            a.Frame = 0;
        else if (a.Frame < frames - 1)
            a.Frame++;
        else if (FollowUp.TryGetValue(state, out int next))
        {
            a.State = next;
            UpdateDisplay(a);
            return;
        }
        else
            a.Frame = 0;
        ShowFrame(a, anim, anim.Def.Frames[a.Frame]);
    }

    /// <summary>Points the character's sprite at one frame of an animation, placed at the character's feet.</summary>
    void ShowFrame(Actor a, AnimInfo anim, AnimFrame f)
    {
        var sprite = a.Sprite;
        if (sprite.Resource != anim.Resource)
        {
            sprite.Resource = anim.Resource;
            sprite.Packed = anim.Resource.Packed || Data.IsPacked(anim.Resource.Path);
        }
        sprite.Frame = anim.Def.X + f.Frame;
        // Some character sheets (standing poses, for example) are stored at twice the resolution
        // of the walking sheets; they are drawn at half size so all poses match.
        double sheetFactor = sprite.Packed && Data.Packed(anim.Resource.Path).FrameHeight > 300 ? 0.5 : 1;
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

    // ---- drawing -----------------------------------------------------------------------------

    /// <summary>
    /// Draws the location: background, sprites and characters furthest first (characters are
    /// hidden behind mask regions in front of them, and cast shadows), then the texts.
    /// </summary>
    public void Draw(Framebuffer fb)
    {
        if (Scene == null)
        {
            fb.Clear();
            return;
        }
        Scene.DrawBackground(fb);
        var mask = Scene.Mask;
        Func<int, int, bool>? Occlusion(Sprite s)
        {
            if (s.MaskDepth is not { } depth || mask == null)
                return null;
            return (x, y) => _regionDepth[mask[y * Framebuffer.Width + x]] is { } region && region < depth;
        }
        Func<Sprite, bool>? castsShadow = Shadows ? s => Actors.Any(a => a.Visible && a.Sprite == s) : null;
        Sprites.Draw(fb, Actors.Select(a => a.Sprite), Occlusion, castsShadow);
        foreach (var t in Texts.ToList())
            DrawText(fb, t);
    }

    /// <summary>Draws a text, wrapped to its width.</summary>
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
