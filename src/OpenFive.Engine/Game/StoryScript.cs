using OpenFive.Engine.World;

namespace OpenFive.Engine.Game;

/// <summary>Speaker codes used in conversations.</summary>
public static class Who
{
    public const int Dick = 0, George = 1, Julian = 2, Anne = 3, Quentin = 5, Fanny = 6, Fisherman = 7, Alf = 8,
        Professor = 9, Accomplice = 10, Policeman = 11;
}

/// <summary>Rucksack item numbers.</summary>
public static class Item
{
    public const int Teapot = 1, Bucket = 2, ClosedTin = 3, OpenTin = 4, OpenKnife = 5, ClosedKnife = 6, BrokenKnife = 7,
        Binoculars = 8, Firewood = 9, OliveOil = 10, Diary = 11, Rope = 12, Axe = 13, TreasureMap = 14, Stone1 = 15,
        Stone2 = 16, Stone3 = 17, ShipPlan = 18, Torch = 19, Umbrella = 20, Plate = 21, Sandwich = 22, Lantern = 23,
        LanternWithCandle = 24, Candle = 25, Screwdriver = 26, Newspaper = 27, FullOilCan = 28, EmptyOilCan = 29,
        MagnifyingGlass = 30, SuitcaseKey = 31, Key = 32;
}

/// <summary>
/// Base class of the story locations: the engine commands the location logic is written in.
/// Methods that queue something return a handle other commands can wait for ("after").
/// </summary>
public abstract class StoryScript : LocationScript
{
    // ---- story state -----------------------------------------------------------------------

    protected int Quest { get => World.Quest; set => World.Quest = value; }
    protected int Episode { get => World.Episode; set => World.Episode = value; }

    /// <summary>True at story chapter <paramref name="q"/>, step <paramref name="e"/>.</summary>
    protected bool At(int q, int e) => World.Quest == q && World.Episode == e;

    protected void SetStep(int q, int e)
    {
        World.Quest = q;
        World.Episode = e;
    }

    protected int Var(string name) => Session.Var(name);
    protected void SetVar(string name, int value) => Session.SetVar(name, value);

    /// <summary>Adds one to a story variable once <paramref name="after"/> is released.</summary>
    protected TaskHandle IncAfter(string name, TaskHandle? after, int delay = 0) =>
        World.Tasks.Add(new ActionTask("INC", _ => Session.SetVar(name, Session.Var(name) + 1)) { WaitFor = after, Delay = delay });

    protected TaskHandle SetAfter(string name, int value, TaskHandle? after, int delay = 0) =>
        World.Tasks.Add(new ActionTask("SET", _ => Session.SetVar(name, value)) { WaitFor = after, Delay = delay });

    protected TaskHandle StepAfter(int q, int e, TaskHandle? after, int delay = 0) =>
        World.Tasks.Add(new ActionTask("STEP", w => { w.Quest = q; w.Episode = e; }) { WaitFor = after, Delay = delay });

    protected TaskHandle Do(Action action, TaskHandle? after, int delay = 0) =>
        World.Tasks.Add(new ActionTask("DO", _ => action()) { WaitFor = after, Delay = delay });

    protected TaskHandle ToggleAfter(string name, TaskHandle? after, int delay = 0) =>
        Do(() => Session.SetVar(name, Session.Var(name) == 0 ? 1 : 0), after, delay);

    /// <summary>Remembered texts (for example which character was clicked).</summary>
    protected void SetText(string name, string value)
    {
        Trace.Log($"text {name} = \"{value}\"");
        Session.Texts[name] = value;
    }
    protected string Text(string name) => Session.Texts.TryGetValue(name, out var v) ? v : "";

    // ---- choice menus -------------------------------------------------------------------------

    /// <summary>
    /// Locations with choice menus keep, per original option 1-3, the row it is shown in (or
    /// "none" once asked), the row the player picked, and the value meaning "none".
    /// </summary>
    static readonly Dictionary<string, (string S1, string S2, string S3, string Chosen, string None)> MenuSlots =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["loc03"] = ("v64ef40", "v64ef44", "v64ef48", "v64ef4c", "v64ef50"),
            ["loc05"] = ("v64ef54", "v64ef58", "v64ef5c", "v64ef60", "v64ef64"),
            ["loc06"] = ("v64ef2c", "v64ef30", "v64ef34", "v64ef38", "v64ef3c"),
            ["loc08"] = ("v64ef68", "v64ef6c", "v64ef70", "v64ef74", "v64ef78"),
            ["loc09"] = ("v64ef7c", "v64ef80", "v64ef84", "v64ef88", "v64ef8c"),
            ["loc15"] = ("v64ef90", "v64ef94", "v64ef98", "v64ef9c", "v64efa0"),
            ["loc17"] = ("v64efa4", "v64efa8", "v64efac", "v64efb0", "v64efb4"),
        };

    /// <summary>Sets up the menu slots of a new game: all options on offer, in order.</summary>
    public static void NewGameMenus(GameSession session)
    {
        foreach (var g in MenuSlots.Values)
        {
            session.SetVar(g.S1, 1);
            session.SetVar(g.S2, 2);
            session.SetVar(g.S3, 3);
            session.SetVar(g.Chosen, -1);
            session.SetVar(g.None, -1);
        }
    }

    public override IReadOnlyList<int>? AvailableOptions(GameWorld world, string scene, int line, int count)
    {
        if (!MenuSlots.TryGetValue(Location, out var g))
            return null;
        int none = Var(g.None);
        var rows = new List<(int Option, int Row)>();
        string[] slots = { g.S1, g.S2, g.S3 };
        for (int i = 0; i < 3 && i < count; i++)
            if (Var(slots[i]) != none)
                rows.Add((i + 1, Var(slots[i])));
        return rows.OrderBy(r => r.Row).Select(r => r.Option).ToList();
    }

    public override void ChoiceMade(GameWorld world, string scene, int line, int option)
    {
        if (MenuSlots.TryGetValue(Location, out var g))
        {
            string[] slots = { g.S1, g.S2, g.S3 };
            SetVar(g.Chosen, Var(slots[option - 1]));
            SetVar(ChoicePending, 1);
        }
        AfterChoice(scene, line, option);
    }

    /// <summary>Set while a picked menu option has not been answered yet.</summary>
    protected const string ChoicePending = "v64ef11";

    /// <summary>The location answers the option the player picked (Chain is the end of the spoken option).</summary>
    protected virtual void AfterChoice(string scene, int line, int option) { }

    /// <summary>Takes every option off this location's menu.</summary>
    protected void CloseMenu()
    {
        if (!MenuSlots.TryGetValue(Location, out var g))
            return;
        foreach (var sl in new[] { g.S1, g.S2, g.S3 })
            SetVar(sl, Var(g.None));
    }

    /// <summary>True when no option is left on this location's menu.</summary>
    protected bool MenuExhausted() =>
        MenuSlots.TryGetValue(Location, out var g) && new[] { g.S1, g.S2, g.S3 }.All(sl => Var(sl) == Var(g.None));

    /// <summary>Takes the picked option off the menu (later options move up one row).</summary>
    protected void RemoveChosenOption()
    {
        if (!MenuSlots.TryGetValue(Location, out var g))
            return;
        int none = Var(g.None), chosen = Var(g.Chosen);
        string[] slots = { g.S1, g.S2, g.S3 };
        int k = Array.FindIndex(slots, sl => Var(sl) == chosen && chosen != none);
        if (k < 0)
            return;
        for (int j = k + 1; j < 3; j++)
            if (Var(slots[j]) != none)
                SetVar(slots[j], Var(slots[j]) - 1);
        SetVar(slots[k], none);
    }

    /// <summary>Runs an action, then evaluates a condition (the game did both inside one test).</summary>
    protected static bool Seq(Action action, Func<bool> condition)
    {
        action();
        return condition();
    }

    /// <summary>Case-insensitive name comparison.</summary>
    protected static bool Same(string? a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    /// <summary>The speaker of the line being spoken (set when a line starts).</summary>
    protected int CurrentSpeaker
    {
        get => World.Dialogue.CurrentSpeaker;
        set => World.Dialogue.CurrentSpeaker = value;
    }

    protected bool German => World.Data.Language == Language.German;

    // ---- items -------------------------------------------------------------------------------

    protected bool Has(int item) => Session.Inventory.Has(item);
    protected void Give(int item) => Session.GiveItem(item);
    /// <summary>Opens (or redraws) the rucksack, e.g. after two items were combined.</summary>
    protected void ShowRucksack() => Session.OpenInventory();
    protected void Take(int item) => Session.TakeItem(item);
    protected TaskHandle GiveAfter(int item, TaskHandle? after, int delay = 0) => Do(() => Give(item), after, delay);
    protected TaskHandle TakeAfter(int item, TaskHandle? after, int delay = 0) => Do(() => Take(item), after, delay);

    // ---- characters --------------------------------------------------------------------------

    protected Actor Hero { get => World.Hero; set => World.Hero = value; }
    protected Actor Dick => World.Dick;
    protected Actor George => World.George;
    protected Actor Julian => World.Julian;
    protected Actor Anne => World.Anne;
    protected Actor Timmy => World.Timmy;
    protected Actor Quentin => World.Quentin;

    /// <summary>Speaker code of the hero (Dick or George).</summary>
    protected int HeroSpeaker => Hero == World.George ? Who.George : Who.Dick;

    /// <summary>Puts a character in this location at the walkable point nearest (x, y), in a state.</summary>
    protected void Place(Actor a, int x, int y, int state) => World.PlaceActor(a, x, y, state);

    protected TaskHandle PlaceAfter(Actor a, int x, int y, int state, TaskHandle? after, int delay = 0) =>
        World.Tasks.Add(new PlaceActorTask(a, x, y, state) { WaitFor = after, Delay = delay });

    protected void Hide(Actor a) => World.HideActor(a);

    /// <summary>Makes a character visible again (where they are).</summary>
    protected void Show(Actor a) => a.Visible = true;

    protected bool IsShown(Actor a) => a.Visible && a.Location.Equals(Location, StringComparison.OrdinalIgnoreCase);

    protected void ClearQueue(Actor a) => a.ClearQueue();

    /// <summary>Is the character still doing something (walking, an action)?</summary>
    protected bool IsBusy(Actor a) => a.Queue.Count > 0;

    /// <summary>Is an animation currently on screen?</summary>
    protected bool IsAnimShown(string anim) => World.IsAnimShown(anim);

    /// <summary>Has the task or queue entry not finished yet?</summary>
    protected static bool IsRunning(TaskHandle? h) => h != null && !h.Released;

    /// <summary>Is the task acting right now (started and not finished)?</summary>
    protected static bool IsActive(TaskHandle? h) => h is GameTask { Started: true, Finished: false } ||
                                                    h is ActorStep { Started: true } st && !st.Completed;

    /// <summary>Allows or blocks walking in an area of this location.</summary>
    protected void AreaEnable(int area, bool on)
    {
        if (World.Walk != null && World.Walk.PathCount + area < World.Walk.Enabled.Count)
            World.Walk.Enabled[World.Walk.PathCount + area] = on;
    }

    protected TaskHandle HideAfter(Actor a, TaskHandle? after, int delay = 0) =>
        World.Tasks.Add(new HideActorTask(a) { WaitFor = after, Delay = delay });

    /// <summary>Shows every character who is in this location.</summary>
    protected void ShowParty()
    {
        foreach (var a in World.Actors)
            if (a.Location.Equals(Location, StringComparison.OrdinalIgnoreCase))
                a.Visible = true;
    }

    /// <summary>Drops what the character was doing and walks to (x, y).</summary>
    protected void Walk(Actor a, int x, int y) => World.WalkTo(a, x, y);

    /// <summary>Walks to (x, y) once <paramref name="after"/> is released.</summary>
    protected TaskHandle WalkAfter(Actor a, int x, int y, TaskHandle? after, int delay = 0) =>
        World.WalkToAfter(a, x, y, after, delay);

    /// <summary>Queues a state (turn, take, talk...) after the character's current actions.</summary>
    protected TaskHandle State(Actor a, int state) => World.SetState(a, state);

    protected TaskHandle StateAfter(Actor a, int state, TaskHandle? after, int delay = 0) =>
        World.SetState(a, state, after, delay);

    // ---- objects, animations, sounds ----------------------------------------------------------

    protected void Obj(string name, bool on) => World.ShowObject(name, on);

    protected TaskHandle ObjAfter(string name, bool on, TaskHandle? after, int delay = 0) =>
        World.Tasks.Add(new ObjectTask(name, on) { WaitFor = after, Delay = delay });

    protected bool IsOn(string name) => World.IsObjectOn(name);
    protected void MoveObj(string name, int x, int y) => World.MoveObject(name, x, y);

    protected void AnimPos(string anim, int x, int y, double scale = 1)
    {
        World.SetAnimPosition(anim, x, y);
        World.SetAnimScale(anim, scale);
    }

    protected void AnimScale(string anim, double scale) => World.SetAnimScale(anim, scale);

    protected TaskHandle? Anim(string anim, int loops = 1, TaskHandle? after = null, int delay = 0,
        string? endObject = null, bool endObjectOn = false) =>
        World.PlayAnim(anim, loops, after, delay, endObject, endObjectOn);

    protected void StopAnim(string anim) => World.StopAnim(anim);

    protected TaskHandle StopAnimAfter(string anim, TaskHandle? after, int delay = 0) =>
        World.Tasks.Add(new StopAnimTask(anim) { WaitFor = after, Delay = delay });

    /// <summary>Changes the drawing depth of a loaded picture.</summary>
    protected void Depth(string path, int depth) => World.SetResourceDepth(path, depth);

    /// <summary>Plays a sound file (path below the disc's Res folder) once <paramref name="after"/> is released.</summary>
    protected TaskHandle Sound(string path, TaskHandle? after = null, int delay = 0) =>
        World.Tasks.Add(new SpeechTask(path, "", 0, 0) { WaitFor = after, Delay = delay });

    protected void Music(string path) => World.Audio.SetBackgroundTrack(path);

    /// <summary>Plays a recording with a text shown at (x, y) once <paramref name="after"/> is released.</summary>
    protected TaskHandle Speech(string text, string path, int x, int y, TaskHandle? after = null, int delay = 0) =>
        World.Tasks.Add(new SpeechTask(path, text, x, y) { WaitFor = after, Delay = delay });

    /// <summary>Plays a sound effect at a volume (0-100).</summary>
    protected TaskHandle Sfx(string path, int volume, TaskHandle? after = null, int delay = 0) =>
        Do(() => World.Audio.PlayEffect(path.Contains('\\') || path.Contains('/') ? path : $@"wav\fx\{path}.wav", volume), after, delay);

    /// <summary>Allows or blocks walking on a path of this location.</summary>
    protected void PathEnable(int path, bool on)
    {
        if (World.Walk != null && path >= 0 && path < World.Walk.Enabled.Count)
            World.Walk.Enabled[path] = on;
    }

    // ---- pointer, conversations, location changes ----------------------------------------

    protected void PointerOff() => World.CursorVisible = false;
    protected void PointerOn() => World.CursorVisible = true;

    protected TaskHandle PointerAfter(bool visible, TaskHandle? after, int delay = 0, int x = -1, int y = -1) =>
        World.Tasks.Add(new CursorTask(visible, x, y) { WaitFor = after, Delay = delay });

    /// <summary>The end of the current sequence (what the next line or action waits for).</summary>
    protected TaskHandle? Chain { get => World.Chain; set => World.Chain = value; }

    /// <summary>
    /// A line of conversation: <paramref name="speaker"/> says line <paramref name="line"/> of
    /// <paramref name="scene"/>. Starts after <paramref name="after"/> (default: the current sequence).
    /// </summary>
    protected TaskHandle Say(int speaker, string scene, int line, bool last = false, TaskHandle? after = null, int delay = 0) =>
        World.Dialogue.Say(speaker, scene, line, after ?? Chain, last: last, delay: delay);

    /// <summary>Offers the player a choice menu (the options of <paramref name="line"/> not asked yet).</summary>
    protected TaskHandle Ask(int speaker, string scene, int line, int position = 0, TaskHandle? after = null) =>
        World.Dialogue.Say(speaker, scene, line, after ?? Chain, position, choice: true);

    protected IReadOnlyCollection<int> Asked(int speaker, string scene, int line) => World.Dialogue.AskedFor(speaker, scene, line);

    /// <summary>
    /// Goes to another location when the hero has finished what they are doing (and
    /// <paramref name="after"/> is released), <paramref name="delay"/> ticks later.
    /// </summary>
    protected void ChangeLocation(string location, TaskHandle? after = null, int delay = 0) =>
        Hero.Enqueue(new ActionStep { Action = _ => Session.ChangeLocation(location), WaitFor = after is ActorStep ? null : after, Delay = delay });

    /// <summary>Where the hero came from (location name).</summary>
    protected string From { get; private set; } = "";

    /// <summary>The game's number of a location ("loc14" = 14, "loc17b" = 117, main menu = 50).</summary>
    public static int IdOf(string location) => location.ToLowerInvariant() switch
    {
        "loc17b" => 117,
        "menue" => 50,
        "load" => 52,
        "save" => 51,
        "options" => 53,
        "locextro" => 70,
        "schiff" => 56,
        "morning" => 57,
        "morgen" => 58,
        "hand" => 59,
        "hand2" => 60,
        "schiffc1" => 61,
        "schiffc2" => 62,
        "schiffc3" => 63,
        "lockarte" => 71,
        var l when l.StartsWith("loc") && int.TryParse(l[3..], out int n) => n,
        _ => 0,
    };

    /// <summary>Number of the location the hero came from.</summary>
    protected int FromId => IdOf(From);
    protected int LocationId => IdOf(Location);

    /// <summary>Changes location immediately (at the start of the next tick).</summary>
    protected void ChangeLocationNow(string location) => Session.ChangeLocation(location);

    public sealed override void Enter(string from)
    {
        From = from;
        PointerOn();
        Setup();
        Arrive();
    }

    /// <summary>Location set-up (objects, animations) when the location is loaded.</summary>
    protected virtual void Setup() { }

    /// <summary>Places the characters depending on where the hero came from, and starts scenes.</summary>
    protected virtual void Arrive() => DefaultArrival(From);

    /// <summary>Walks the hero to (x, y) and then turns them to <paramref name="state"/>.</summary>
    protected TaskHandle WalkAndFace(int x, int y, int state)
    {
        Walk(Hero, x, y);
        return State(Hero, state);
    }
}
