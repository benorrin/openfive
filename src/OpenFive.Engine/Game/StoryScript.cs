using OpenFive.Engine.World;

namespace OpenFive.Engine.Game;

/// <summary>Speaker codes used in conversations (see <see cref="DialogueSystem.Speakers"/>).</summary>
public static class Who
{
    /// <summary>Dick, one of the two characters the player controls.</summary>
    public const int Dick = 0;
    /// <summary>George, the other character the player controls.</summary>
    public const int George = 1;
    /// <summary>Julian, Dick's older brother.</summary>
    public const int Julian = 2;
    /// <summary>Anne, the youngest of the Five.</summary>
    public const int Anne = 3;
    /// <summary>Uncle Quentin, George's father.</summary>
    public const int Quentin = 5;
    /// <summary>Aunt Fanny, George's mother.</summary>
    public const int Fanny = 6;
    /// <summary>The fisherman on the beach.</summary>
    public const int Fisherman = 7;
    /// <summary>Alf, the fisher boy who looks after Timmy.</summary>
    public const int Alf = 8;
    /// <summary>The professor, the villain of the story.</summary>
    public const int Professor = 9;
    /// <summary>The professor's accomplice.</summary>
    public const int Accomplice = 10;
    /// <summary>The policeman at the end of the story.</summary>
    public const int Policeman = 11;
    /// <summary>
    /// A second code for Dick (the original's 0x14): the same lines and talking head, but line
    /// hooks see a speaker other than <see cref="Dick"/>.
    /// </summary>
    public const int DickAlt = 0x14;

    /// <summary>A second code for George (the original's 0x15), like <see cref="DickAlt"/>.</summary>
    public const int GeorgeAlt = 0x15;
}

/// <summary>Rucksack item numbers (the original's item ids; see <see cref="Inventory"/>).</summary>
public static class Item
{
    /// <summary>The teapot from the kitchen.</summary>
    public const int Teapot = 1;
    /// <summary>The bucket from the garden.</summary>
    public const int Bucket = 2;
    /// <summary>The tin from the wreck, still closed.</summary>
    public const int ClosedTin = 3;
    /// <summary>The tin from the wreck, opened.</summary>
    public const int OpenTin = 4;
    /// <summary>Dick's pocket knife, opened.</summary>
    public const int OpenKnife = 5;
    /// <summary>Dick's pocket knife, folded (in the rucksack at the start).</summary>
    public const int ClosedKnife = 6;
    /// <summary>The pocket knife after its blade broke.</summary>
    public const int BrokenKnife = 7;
    /// <summary>The binoculars.</summary>
    public const int Binoculars = 8;
    /// <summary>Firewood from the study's log basket.</summary>
    public const int Firewood = 9;
    /// <summary>The olive oil from the kitchen.</summary>
    public const int OliveOil = 10;
    /// <summary>The diary.</summary>
    public const int Diary = 11;
    /// <summary>The rope from the shed.</summary>
    public const int Rope = 12;
    /// <summary>The axe from the shed.</summary>
    public const int Axe = 13;
    /// <summary>The treasure map from the tin.</summary>
    public const int TreasureMap = 14;
    /// <summary>The first stone from the pile in the castle courtyard.</summary>
    public const int Stone1 = 15;
    /// <summary>The second stone from the pile.</summary>
    public const int Stone2 = 16;
    /// <summary>The third stone from the pile.</summary>
    public const int Stone3 = 17;
    /// <summary>The plan of the ship from Uncle Quentin's study.</summary>
    public const int ShipPlan = 18;
    /// <summary>The torch.</summary>
    public const int Torch = 19;
    /// <summary>The umbrella from the hall.</summary>
    public const int Umbrella = 20;
    /// <summary>A plate from the kitchen.</summary>
    public const int Plate = 21;
    /// <summary>The sandwich (for Timmy).</summary>
    public const int Sandwich = 22;
    /// <summary>The lantern, without a candle.</summary>
    public const int Lantern = 23;
    /// <summary>The lantern with a candle in it.</summary>
    public const int LanternWithCandle = 24;
    /// <summary>A candle.</summary>
    public const int Candle = 25;
    /// <summary>The screwdriver from the shed.</summary>
    public const int Screwdriver = 26;
    /// <summary>The newspaper (in the rucksack at the start).</summary>
    public const int Newspaper = 27;
    /// <summary>The oil can, filled with olive oil.</summary>
    public const int FullOilCan = 28;
    /// <summary>The empty oil can from the shed.</summary>
    public const int EmptyOilCan = 29;
    /// <summary>The magnifying glass.</summary>
    public const int MagnifyingGlass = 30;
    /// <summary>The key to Uncle Quentin's suitcase.</summary>
    public const int SuitcaseKey = 31;
    /// <summary>The key from the ship.</summary>
    public const int Key = 32;
}

/// <summary>
/// Base class of the story locations: the engine commands the location logic is written in.
/// Methods that queue something return a handle other commands can wait for ("after"); a
/// <c>delay</c> is counted in game ticks (ten per second) from the moment "after" is released.
/// </summary>
public abstract class StoryScript : LocationScript
{
    // ---- story state -----------------------------------------------------------------------

    /// <summary>The story chapter.</summary>
    protected int Quest { get => World.Quest; set => World.Quest = value; }

    /// <summary>The step within the chapter.</summary>
    protected int Episode { get => World.Episode; set => World.Episode = value; }

    /// <summary>True at story chapter <paramref name="quest"/>, step <paramref name="episode"/>.</summary>
    protected bool At(int quest, int episode) => World.Quest == quest && World.Episode == episode;

    /// <summary>Moves the story to chapter <paramref name="quest"/>, step <paramref name="episode"/>.</summary>
    protected void SetStep(int quest, int episode)
    {
        World.Quest = quest;
        World.Episode = episode;
    }

    /// <summary>The value of a story variable (0 if it was never set).</summary>
    protected int Var(string name) => Session.Var(name);

    /// <summary>Sets a story variable.</summary>
    protected void SetVar(string name, int value) => Session.SetVar(name, value);

    /// <summary>Runs <paramref name="action"/> once <paramref name="after"/> is released.</summary>
    protected TaskHandle Do(Action action, TaskHandle? after, int delay = 0) =>
        Queue(new ActionTask("DO", _ => action()), after, delay);

    /// <summary>Adds one to a story variable once <paramref name="after"/> is released.</summary>
    protected TaskHandle IncAfter(string name, TaskHandle? after, int delay = 0) =>
        Queue(new ActionTask("INC", _ => Session.SetVar(name, Session.Var(name) + 1)), after, delay);

    /// <summary>Sets a story variable once <paramref name="after"/> is released.</summary>
    protected TaskHandle SetAfter(string name, int value, TaskHandle? after, int delay = 0) =>
        Queue(new ActionTask("SET", _ => Session.SetVar(name, value)), after, delay);

    /// <summary>Moves the story to a new chapter and step once <paramref name="after"/> is released.</summary>
    protected TaskHandle StepAfter(int quest, int episode, TaskHandle? after, int delay = 0) =>
        Queue(new ActionTask("STEP", w => (w.Quest, w.Episode) = (quest, episode)), after, delay);

    /// <summary>Switches a 0/1 story variable once <paramref name="after"/> is released.</summary>
    protected TaskHandle ToggleAfter(string name, TaskHandle? after, int delay = 0) =>
        Do(() => Session.SetVar(name, Session.Var(name) == 0 ? 1 : 0), after, delay);

    /// <summary>Remembers a text (for example which character was clicked); saved with the game.</summary>
    protected void SetText(string name, string value)
    {
        Trace.Log($"text {name} = \"{value}\"");
        Session.Texts[name] = value;
    }

    /// <summary>A text remembered with <see cref="SetText"/> ("" if none).</summary>
    protected string Text(string name) => Session.Texts.TryGetValue(name, out var v) ? v : "";

    /// <summary>Adds a task to the world's task list, to start once <paramref name="after"/> is released.</summary>
    TaskHandle Queue(GameTask task, TaskHandle? after, int delay)
    {
        task.WaitFor = after;
        task.Delay = delay;
        return World.Tasks.Add(task);
    }

    // ---- choice menus -------------------------------------------------------------------------

    /// <summary>
    /// The story variables of a location's choice menu. For each of the original options 1-3
    /// they hold the row it is shown in (or <c>None</c> once it has been asked); <c>Chosen</c> is
    /// the row the player picked and <c>None</c> the value meaning "not on the menu".
    /// </summary>
    sealed record MenuVars(string Slot1, string Slot2, string Slot3, string Chosen, string None)
    {
        public string[] Slots => new[] { Slot1, Slot2, Slot3 };
    }

    /// <summary>The menu variables of the locations that have a choice menu.</summary>
    static readonly Dictionary<string, MenuVars> Menus = new(StringComparer.OrdinalIgnoreCase)
    {
        ["loc03"] = new("v64ef40", "v64ef44", "v64ef48", "v64ef4c", "v64ef50"),
        ["loc05"] = new("v64ef54", "v64ef58", "v64ef5c", "v64ef60", "v64ef64"),
        ["loc06"] = new("v64ef2c", "v64ef30", "v64ef34", "v64ef38", "v64ef3c"),
        ["loc08"] = new("v64ef68", "v64ef6c", "v64ef70", "v64ef74", "v64ef78"),
        ["loc09"] = new("v64ef7c", "v64ef80", "v64ef84", "v64ef88", "v64ef8c"),
        ["loc15"] = new("v64ef90", "v64ef94", "v64ef98", "v64ef9c", "v64efa0"),
        ["loc17"] = new("v64efa4", "v64efa8", "v64efac", "v64efb0", "v64efb4"),
    };

    /// <summary>Sets up the menu variables of a new game: all options on offer, in order.</summary>
    public static void NewGameMenus(GameSession session)
    {
        foreach (var menu in Menus.Values)
        {
            session.SetVar(menu.Slot1, 1);
            session.SetVar(menu.Slot2, 2);
            session.SetVar(menu.Slot3, 3);
            session.SetVar(menu.Chosen, -1);
            session.SetVar(menu.None, -1);
        }
    }

    /// <summary>The options still on this location's menu, in the order they are shown.</summary>
    public override IReadOnlyList<int>? AvailableOptions(GameWorld world, string scene, int line, int count)
    {
        if (!Menus.TryGetValue(Location, out var menu))
            return null;
        int none = Var(menu.None);
        return menu.Slots
            .Take(count)
            .Select((slot, i) => (Option: i + 1, Row: Var(slot)))
            .Where(o => o.Row != none)
            .OrderBy(o => o.Row)
            .Select(o => o.Option)
            .ToList();
    }

    /// <summary>Records the picked option in the menu variables, then lets the location answer it.</summary>
    public override void ChoiceMade(GameWorld world, string scene, int line, int option)
    {
        if (Menus.TryGetValue(Location, out var menu))
        {
            SetVar(menu.Chosen, Var(menu.Slots[option - 1]));
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
        if (!Menus.TryGetValue(Location, out var menu))
            return;
        foreach (string slot in menu.Slots)
            SetVar(slot, Var(menu.None));
    }

    /// <summary>Puts every option back on a location's menu (default: this one), in order.</summary>
    protected void ReopenMenu(string? location = null)
    {
        if (!Menus.TryGetValue(location ?? Location, out var menu))
            return;
        SetVar(menu.None, -1);
        SetVar(menu.Chosen, -1);
        SetVar(menu.Slot1, 1);
        SetVar(menu.Slot2, 2);
        SetVar(menu.Slot3, 3);
    }

    /// <summary>True when no option is left on this location's menu.</summary>
    protected bool MenuExhausted() =>
        Menus.TryGetValue(Location, out var menu) && menu.Slots.All(slot => Var(slot) == Var(menu.None));

    /// <summary>Takes the picked option off the menu (later options move up one row).</summary>
    protected void RemoveChosenOption()
    {
        if (!Menus.TryGetValue(Location, out var menu))
            return;
        int none = Var(menu.None), chosen = Var(menu.Chosen);
        string[] slots = menu.Slots;
        int k = Array.FindIndex(slots, slot => Var(slot) == chosen && chosen != none);
        if (k < 0)
            return;
        foreach (string later in slots.Skip(k + 1))
            if (Var(later) != none)
                SetVar(later, Var(later) - 1);
        SetVar(slots[k], none);
    }

    // ---- helpers -----------------------------------------------------------------------------

    /// <summary>
    /// Runs an action, then evaluates a condition. The original sometimes did both inside one
    /// test; this keeps the order of events where that matters.
    /// </summary>
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

    /// <summary>True when the game is played in German.</summary>
    protected bool German => World.Data.Language == Language.German;

    // ---- items -------------------------------------------------------------------------------

    /// <summary>Is the item in the rucksack?</summary>
    protected bool Has(int item) => Session.Inventory.Has(item);

    /// <summary>Puts an item into the rucksack.</summary>
    protected void Give(int item) => Session.GiveItem(item);

    /// <summary>Takes an item out of the rucksack.</summary>
    protected void Take(int item) => Session.TakeItem(item);

    /// <summary>Opens (or redraws) the rucksack, e.g. after two items were combined.</summary>
    protected void ShowRucksack() => Session.OpenInventory();

    /// <summary>Puts an item into the rucksack once <paramref name="after"/> is released.</summary>
    protected TaskHandle GiveAfter(int item, TaskHandle? after, int delay = 0) => Do(() => Give(item), after, delay);

    /// <summary>Takes an item out of the rucksack once <paramref name="after"/> is released.</summary>
    protected TaskHandle TakeAfter(int item, TaskHandle? after, int delay = 0) => Do(() => Take(item), after, delay);

    // ---- characters --------------------------------------------------------------------------

    /// <summary>The character the player controls (Dick or George).</summary>
    protected Actor Hero { get => World.Hero; set => World.Hero = value; }

    /// <summary>Dick.</summary>
    protected Actor Dick => World.Dick;

    /// <summary>George.</summary>
    protected Actor George => World.George;

    /// <summary>Julian.</summary>
    protected Actor Julian => World.Julian;

    /// <summary>Anne.</summary>
    protected Actor Anne => World.Anne;

    /// <summary>Timmy the dog.</summary>
    protected Actor Timmy => World.Timmy;

    /// <summary>Uncle Quentin.</summary>
    protected Actor Quentin => World.Quentin;

    /// <summary>Speaker code of the hero (Dick or George).</summary>
    protected int HeroSpeaker => Hero == World.George ? Who.George : Who.Dick;

    /// <summary>Puts a character in this location at the walkable point nearest (x, y), in a state.</summary>
    protected void Place(Actor a, int x, int y, int state) => World.PlaceActor(a, x, y, state);

    /// <summary>Places a character (see <see cref="Place"/>) once <paramref name="after"/> is released.</summary>
    protected TaskHandle PlaceAfter(Actor a, int x, int y, int state, TaskHandle? after, int delay = 0) =>
        Queue(new ActionTask("PLACE", w => w.PlaceActor(a, x, y, state)), after, delay);

    /// <summary>Hides a character (its queued actions carry on).</summary>
    protected void Hide(Actor a) => World.HideActor(a);

    /// <summary>Hides a character once <paramref name="after"/> is released.</summary>
    protected TaskHandle HideAfter(Actor a, TaskHandle? after, int delay = 0) =>
        Queue(new ActionTask("HIDE", w => w.HideActor(a)), after, delay);

    /// <summary>Makes a character visible again (where they are).</summary>
    protected void Show(Actor a) => a.Visible = true;

    /// <summary>Is the character visible in this location?</summary>
    protected bool IsShown(Actor a) => a.Visible && a.Location.Equals(Location, StringComparison.OrdinalIgnoreCase);

    /// <summary>Shows every character who is in this location.</summary>
    protected void ShowParty()
    {
        foreach (var a in World.Actors)
            if (a.Location.Equals(Location, StringComparison.OrdinalIgnoreCase))
                a.Visible = true;
    }

    /// <summary>Drops everything the character has queued.</summary>
    protected void ClearQueue(Actor a) => a.ClearQueue();

    /// <summary>Is the character still doing something (walking, an action)?</summary>
    protected bool IsBusy(Actor a) => a.Queue.Count > 0;

    /// <summary>Has the task or queue entry not finished yet?</summary>
    protected static bool IsRunning(TaskHandle? h) => h != null && !h.Released;

    /// <summary>Is the task acting right now (started and not finished)?</summary>
    protected static bool IsActive(TaskHandle? h) =>
        h is GameTask { Started: true, Finished: false } || h is ActorStep { Started: true, Completed: false };

    /// <summary>Drops what the character was doing and walks to (x, y).</summary>
    protected void Walk(Actor a, int x, int y) => World.WalkTo(a, x, y);

    /// <summary>Walks to (x, y) once <paramref name="after"/> is released.</summary>
    protected TaskHandle WalkAfter(Actor a, int x, int y, TaskHandle? after, int delay = 0) =>
        World.WalkToAfter(a, x, y, after, delay);

    /// <summary>Walks the hero to (x, y) and then turns them to <paramref name="state"/>.</summary>
    protected TaskHandle WalkAndFace(int x, int y, int state)
    {
        Walk(Hero, x, y);
        return State(Hero, state);
    }

    /// <summary>Queues a state (turn, take, talk...) after the character's current actions.</summary>
    protected TaskHandle State(Actor a, int state) => World.SetState(a, state);

    /// <summary>Queues a state once <paramref name="after"/> is released.</summary>
    protected TaskHandle StateAfter(Actor a, int state, TaskHandle? after, int delay = 0) =>
        World.SetState(a, state, after, delay);

    /// <summary>Allows or blocks walking on a path of this location.</summary>
    protected void PathEnable(int path, bool on)
    {
        if (World.Walk != null && path >= 0 && path < World.Walk.Enabled.Count)
            World.Walk.Enabled[path] = on;
    }

    /// <summary>Allows or blocks walking in an area of this location (areas are numbered after the paths).</summary>
    protected void AreaEnable(int area, bool on)
    {
        if (World.Walk != null)
            PathEnable(World.Walk.PathCount + area, on);
    }

    // ---- objects, animations, sounds ----------------------------------------------------------

    /// <summary>Switches an object on or off.</summary>
    protected void Obj(string name, bool on) => World.ShowObject(name, on);

    /// <summary>Switches an object on or off once <paramref name="after"/> is released.</summary>
    protected TaskHandle ObjAfter(string name, bool on, TaskHandle? after, int delay = 0) =>
        Queue(new ActionTask("OBJECT", w => w.ShowObject(name, on)), after, delay);

    /// <summary>Is the object switched on?</summary>
    protected bool IsOn(string name) => World.IsObjectOn(name);

    /// <summary>Moves an object (picture and click area) so that its anchor is at (x, y).</summary>
    protected void MoveObj(string name, int x, int y) => World.MoveObject(name, x, y);

    /// <summary>Sets where an animation is drawn, and its size.</summary>
    protected void AnimPos(string anim, int x, int y, double scale = 1)
    {
        World.SetAnimPosition(anim, x, y);
        World.SetAnimScale(anim, scale);
    }

    /// <summary>Sets the size an animation is drawn at.</summary>
    protected void AnimScale(string anim, double scale) => World.SetAnimScale(anim, scale);

    /// <summary>
    /// Plays an animation <paramref name="loops"/> times (-1 = until stopped) once
    /// <paramref name="after"/> is released. When it ends, <paramref name="endObject"/> (if given)
    /// is switched to <paramref name="endObjectOn"/>. Returns null if the animation is unknown.
    /// </summary>
    protected TaskHandle? Anim(string anim, int loops = 1, TaskHandle? after = null, int delay = 0,
        string? endObject = null, bool endObjectOn = false) =>
        World.PlayAnim(anim, loops, after, delay, endObject, endObjectOn);

    /// <summary>Stops an animation now.</summary>
    protected void StopAnim(string anim) => World.StopAnim(anim);

    /// <summary>Stops an animation once <paramref name="after"/> is released.</summary>
    protected TaskHandle StopAnimAfter(string anim, TaskHandle? after, int delay = 0) =>
        Queue(new ActionTask("STOP_ANIM", w => w.StopAnim(anim, onlyStarted: true)), after, delay);

    /// <summary>Is an animation currently on screen?</summary>
    protected bool IsAnimShown(string anim) => World.IsAnimShown(anim);

    /// <summary>Changes the drawing depth of a loaded picture.</summary>
    protected void Depth(string path, int depth) => World.SetResourceDepth(path, depth);

    /// <summary>Plays a sound file (path below the disc's Res folder) once <paramref name="after"/> is released.</summary>
    protected TaskHandle Sound(string path, TaskHandle? after = null, int delay = 0) =>
        Queue(new SpeechTask(path, "", 0, 0), after, delay);

    /// <summary>Plays a recording with a text shown at (x, y) once <paramref name="after"/> is released.</summary>
    protected TaskHandle Speech(string text, string path, int x, int y, TaskHandle? after = null, int delay = 0) =>
        Queue(new SpeechTask(path, text, x, y), after, delay);

    /// <summary>
    /// Plays a sound effect at a volume (0-100). A bare name means "wav\fx\NAME.wav".
    /// </summary>
    protected TaskHandle Sfx(string path, int volume, TaskHandle? after = null, int delay = 0)
    {
        string file = path.Contains('\\') || path.Contains('/') ? path : $@"wav\fx\{path}.wav";
        return Do(() => World.Audio.PlayEffect(file, volume), after, delay);
    }

    /// <summary>Changes the background music.</summary>
    protected void Music(string path) => World.Audio.SetBackgroundTrack(path);

    // ---- talking poses ----------------------------------------------------------------------

    /// <summary>The character a speaker is in this place, with its pose while talking and afterwards.</summary>
    /// <param name="Actor">The character.</param>
    /// <param name="Talking">Its state while the line is spoken.</param>
    /// <param name="Standing">Its state once the line has been spoken.</param>
    protected readonly record struct TalkPoses(Actor Actor, int Talking, int Standing);

    /// <summary>The poses of a speaker in this place (null: the speaker takes no pose here).</summary>
    protected virtual TalkPoses? PosesOf(int speaker) => null;

    /// <summary>
    /// The poses used in the dungeon passages: Anne and George talk facing down-right, Julian and
    /// the hero (as Dick) down-left.
    /// </summary>
    protected TalkPoses? DungeonPoses(int speaker) => speaker switch
    {
        Who.Anne => new TalkPoses(Anne, ActorStates.SpeakDownRight, ActorStates.StandDownRight),
        Who.Julian => new TalkPoses(Julian, ActorStates.SpeakDownLeft, ActorStates.StandDownLeft),
        Who.George => new TalkPoses(George, ActorStates.SpeakDownRight, ActorStates.StandDownRight),
        Who.Dick => new TalkPoses(Hero, ActorStates.SpeakDownLeft, ActorStates.StandDownLeft),
        _ => null,
    };

    /// <summary>
    /// Poses for the hero (as Dick) and George talking to each other: each faces the other,
    /// depending on who stands further left. Null for other speakers.
    /// </summary>
    protected TalkPoses? HeroAndGeorgePoses(int speaker)
    {
        bool georgeOnTheLeft = George.X <= Hero.X;
        return speaker switch
        {
            Who.George => georgeOnTheLeft
                ? new TalkPoses(George, ActorStates.SpeakDownRight, ActorStates.StandDownRight)
                : new TalkPoses(George, ActorStates.SpeakDownLeft, ActorStates.StandDownLeft),
            Who.Dick => georgeOnTheLeft
                ? new TalkPoses(Hero, ActorStates.SpeakDownLeft, ActorStates.StandDownLeft)
                : new TalkPoses(Hero, ActorStates.SpeakDownRight, ActorStates.StandDownRight),
            _ => null,
        };
    }

    /// <summary>The state that turns the hero towards George.</summary>
    protected int FacingGeorge => George.X <= Hero.X ? ActorStates.StandDownLeft : ActorStates.StandDownRight;

    /// <summary>The current speaker takes their talking pose (see <see cref="PosesOf"/>).</summary>
    protected void StartTalking()
    {
        if (PosesOf(CurrentSpeaker) is { } p)
            StateAfter(p.Actor, p.Talking, null);
    }

    /// <summary>The current speaker stands again once <paramref name="end"/> is released (see <see cref="PosesOf"/>).</summary>
    protected void StopTalking(TaskHandle end)
    {
        if (PosesOf(CurrentSpeaker) is { } p)
            StateAfter(p.Actor, p.Standing, end);
    }

    // ---- pointer, conversations, location changes ----------------------------------------

    /// <summary>Hides the mouse pointer (the player cannot act during a scene).</summary>
    protected void PointerOff() => World.CursorVisible = false;

    /// <summary>Shows the mouse pointer.</summary>
    protected void PointerOn() => World.CursorVisible = true;

    /// <summary>
    /// Shows or hides the pointer once <paramref name="after"/> is released; (x, y), when given,
    /// moves it there.
    /// </summary>
    protected TaskHandle PointerAfter(bool visible, TaskHandle? after, int delay = 0, int x = -1, int y = -1) =>
        Queue(new ActionTask("SWITCHMOUSE", w =>
        {
            Trace.Log($"{w.Tick}: pointer {(visible ? "on" : "off")}");
            w.CursorVisible = visible;
            if (x != -1)
                w.CursorWarp = (x, y);
        }), after, delay);

    /// <summary>The end of the current sequence (what the next line or action waits for).</summary>
    protected TaskHandle? Chain { get => World.Chain; set => World.Chain = value; }

    /// <summary>
    /// A line of conversation: <paramref name="speaker"/> says line <paramref name="line"/> of
    /// <paramref name="scene"/>. Starts after <paramref name="after"/> (default: the current
    /// sequence) and becomes the current sequence. <paramref name="last"/> marks the end of a
    /// conversation (the pointer comes back after it).
    /// </summary>
    protected TaskHandle Say(int speaker, string scene, int line, bool last = false, TaskHandle? after = null, int delay = 0) =>
        World.Dialogue.Say(speaker, scene, line, after ?? Chain, last: last, delay: delay);

    /// <summary>Offers the player a choice menu (the options of <paramref name="line"/> not asked yet).</summary>
    protected TaskHandle Ask(int speaker, string scene, int line, int position = 0, TaskHandle? after = null) =>
        World.Dialogue.Say(speaker, scene, line, after ?? Chain, position, choice: true);

    /// <summary>The options of a choice menu that have already been asked.</summary>
    protected IReadOnlyCollection<int> Asked(int speaker, string scene, int line) =>
        World.Dialogue.AskedFor(speaker, scene, line);

    /// <summary>
    /// Goes to another location when the hero has finished what they are doing (and
    /// <paramref name="after"/> is released), <paramref name="delay"/> ticks later.
    /// </summary>
    protected void ChangeLocation(string location, TaskHandle? after = null, int delay = 0) =>
        Hero.Enqueue(new ActionStep
        {
            Action = _ => Session.ChangeLocation(location),
            WaitFor = after is ActorStep ? null : after,
            Delay = delay,
        });

    /// <summary>Changes location immediately (at the start of the next tick).</summary>
    protected void ChangeLocationNow(string location) => Session.ChangeLocation(location);

    /// <summary>Where the hero came from (location name).</summary>
    protected string From { get; private set; } = "";

    /// <summary>The original's number of the location the hero came from.</summary>
    protected int FromId => IdOf(From);

    /// <summary>The original's number of this location.</summary>
    protected int LocationId => IdOf(Location);

    /// <summary>The original's number of a location ("loc14" = 14, "loc17b" = 117, main menu = 54).</summary>
    public static int IdOf(string location) => location.ToLowerInvariant() switch
    {
        "loc17b" => 117,
        "logo" => 50,
        "load" => 52,
        "save" => 53,
        "menue" => 54,
        "options" => 55,
        "schiff" => 56,
        "morning" => 57,
        "morgen" => 58,
        "hand" => 59,
        "hand2" => 60,
        "schiffc1" => 61,
        "schiffc2" => 62,
        "schiffc3" => 63,
        "locextro" => 70,
        "lockarte" => 71,
        var l when l.StartsWith("loc") && int.TryParse(l[3..], out int n) => n,
        _ => 0,
    };

    /// <summary>Remembers where the hero came from, then sets the location up and places the characters.</summary>
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
}
