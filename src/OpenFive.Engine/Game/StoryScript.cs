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

    // ---- items -------------------------------------------------------------------------------

    protected bool Has(int item) => Session.Inventory.Has(item);
    protected void Give(int item) => Session.GiveItem(item);
    protected void Take(int item) => Session.TakeItem(item);
    protected TaskHandle GiveAfter(int item, TaskHandle? after, int delay = 0) => Do(() => Give(item), after, delay);
    protected TaskHandle TakeAfter(int item, TaskHandle? after, int delay = 0) => Do(() => Take(item), after, delay);

    // ---- characters --------------------------------------------------------------------------

    protected Actor Hero => World.Hero;
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
