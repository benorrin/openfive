using OpenFive.Engine.Formats;

namespace OpenFive.Engine.World;

/// <summary>A person who can speak: their dialogue folder and talking-head animations.</summary>
public sealed record Speaker(int Code, string Folder, string HeadStill, string HeadTalk);

/// <summary>Called by the dialogue system so a location's script can continue a conversation.</summary>
public interface IDialogueHooks
{
    /// <summary>A line is about to be spoken by <paramref name="speaker"/> (speaker code).</summary>
    void LineStarting(GameWorld world, int speaker) { }

    /// <summary>A line (not the last of its conversation) has been queued; <paramref name="end"/> is released when it has been spoken.</summary>
    void LineQueued(GameWorld world, TaskHandle end) { }

    /// <summary>The last line of a conversation has been queued.</summary>
    void ConversationEnding(GameWorld world, TaskHandle end) { }

    /// <summary>A choice menu was opened (the option the player picks is reported to <see cref="ChoiceMade"/>).</summary>
    void ChoiceOpened(GameWorld world) { }

    /// <summary>The player picked option <paramref name="option"/> (1-3, numbered as in the dialogue file).</summary>
    void ChoiceMade(GameWorld world, string scene, int line, int option) { }

    /// <summary>
    /// The options of a menu still on offer, in display order, or null to let the dialogue system
    /// keep track itself (each option is offered until asked).
    /// </summary>
    IReadOnlyList<int>? AvailableOptions(GameWorld world, string scene, int line, int count) => null;
}

/// <summary>
/// Conversations: single lines spoken with the dialogue box, the speaker's talking head, the
/// recording and its text; and choice menus offering the player up to three questions.
/// </summary>
public sealed class DialogueSystem
{
    /// <summary>Speaker codes used by the scripts.</summary>
    public static readonly Speaker[] Speakers =
    {
        new(0, "DICK", "DickHeadAnimStill", "DickHeadAnimTalk"),
        new(1, "George", "GeorgeHeadAnimStill", "GeorgeHeadAnimTalk"),
        new(2, "Julian", "JulianHeadAnimStill", "JulianHeadAnimTalk"),
        new(3, "Anne", "AnneHeadAnimStill", "AnneHeadAnimTalk"),
        new(5, "Quentin", "QuentinHeadAnimStill", "QuentinHeadAnimTalk"),
        new(6, "Fanny", "FannyHeadAnimStill", "FannyHeadAnimTalk"),
        new(7, "Fischer", "FischerHeadAnimStill", "FischerHeadAnimTalk"),
        new(8, "Alf", "AlfHeadAnimStill", "AlfHeadAnimTalk"),
        new(9, "Profesor", "ProfesorHeadAnimStill", "ProfesorHeadAnimTalk"),
        new(10, "Komplize", "KomplizeHeadAnimStill", "KomplizeHeadAnimTalk"),
        new(11, "Polizist", "PolizistHeadAnimStill", "PolizistHeadAnimTalk"),
        new(0x14, "DICK", "DickHeadAnimStill", "DickHeadAnimTalk"),
        new(0x15, "George", "GeorgeHeadAnimStill", "GeorgeHeadAnimTalk"),
        new(0x16, "Fischer", "FischerHeadAnimStill", "FischerHeadAnimTalk"),
    };

    public static Speaker? SpeakerFor(int code) => Speakers.FirstOrDefault(s => s.Code == code);

    readonly GameWorld _world;
    /// <summary>Options already asked, per choice menu (speaker, scene, line).</summary>
    readonly Dictionary<string, HashSet<int>> _asked = new();

    public IDialogueHooks? Hooks { get; set; }
    /// <summary>The choice menu waiting for the player, if any.</summary>
    public ChoiceTask? OpenMenu { get; internal set; }
    /// <summary>The last option the player picked (1-3), as numbered in the dialogue file.</summary>
    public int LastChoice { get; internal set; }

    public DialogueSystem(GameWorld world) => _world = world;

    internal static string MenuKey(Speaker s, string scene, int line) => $"{s.Folder}|{scene}|{line}";

    internal HashSet<int> Asked(string key)
    {
        if (!_asked.TryGetValue(key, out var set))
            _asked[key] = set = new HashSet<int>();
        return set;
    }

    /// <summary>Persisted state: options already asked.</summary>
    public Dictionary<string, HashSet<int>> AskedOptions => _asked;

    internal DialogEntry? Lookup(Speaker s, bool choice, string scene, int line) =>
        _world.Data.Dialogs.Get(s.Folder, choice, _world.Quest, _world.Episode, scene, line);

    /// <summary>Options of a choice menu that have not been asked yet.</summary>
    public IReadOnlyCollection<int> AskedFor(int speaker, string scene, int line) =>
        Asked(MenuKey(SpeakerFor(speaker) ?? Speakers[0], scene, line));

    /// <summary>Marks options of a choice menu as asked (they will not be offered again).</summary>
    public void MarkAsked(int speaker, string scene, int line, params int[] options)
    {
        var set = Asked(MenuKey(SpeakerFor(speaker) ?? Speakers[0], scene, line));
        foreach (int o in options)
            set.Add(o);
    }

    /// <summary>Makes all options of a choice menu available again.</summary>
    public void ResetMenu(int speaker, string scene, int line) =>
        Asked(MenuKey(SpeakerFor(speaker) ?? Speakers[0], scene, line)).Clear();

    internal void LineStarting(int speaker) => Hooks?.LineStarting(_world, speaker);

    /// <summary>
    /// The player picked <paramref name="option"/>: the asking character says it, then the
    /// location continues the conversation after it.
    /// </summary>
    internal GameTask Choose(GameWorld world, DialogueTask owner, Speaker speaker, DialogEntry entry, string scene, int line,
        int option, string key)
    {
        Asked(key).Add(option);
        LastChoice = option;
        var spoken = new SpokenLine(this, speaker, entry.Options[option - 1]);
        world.Chain = owner;
        Hooks?.LineQueued(world, owner);
        Hooks?.ChoiceMade(world, scene, line, option);
        return spoken;
    }

    /// <summary>
    /// Queues a line of <paramref name="speaker"/> from dialogue scene <paramref name="scene"/>.
    /// It starts once <paramref name="after"/> (default: the current sequence) is released. When
    /// the dialogue file holds a choice menu for this line, the player is asked instead.
    /// Returns a handle that is released when the line has been spoken; it also becomes the
    /// current sequence.
    /// </summary>
    public TaskHandle Say(int speaker, string scene, int line, TaskHandle? after = null, int menuPosition = 2,
        bool last = false, int delay = 0, bool choice = false)
    {
        var s = SpeakerFor(speaker) ?? Speakers[0];
        var task = _world.Tasks.Add(new DialogueTask(this, s, scene, line, menuPosition, last, choice)
        {
            WaitFor = after ?? _world.Chain,
            Delay = delay,
        });
        _world.Chain = task;
        return task;
    }

    internal void ShowBox(bool on)
    {
        _world.MoveObject("COMMON_TEXTBOX2", 4, BoxY - 10);
        _world.MoveObject("COMMON_DLG1", 0, BoxY);
        _world.ShowObject("COMMON_TEXTBOX2", on);
        _world.ShowObject("COMMON_DLG1", on);
        _world.ShowObject("COMMON_DLGDUMMY", on);
    }

    /// <summary>Top of the text in the dialogue box at the bottom of the screen.</summary>
    public const int BoxY = 500;

    internal void LineQueued(TaskHandle end, bool last)
    {
        if (last)
        {
            _world.Tasks.Add(new CursorTask(true, 400, 300) { WaitFor = end });
            Hooks?.ConversationEnding(_world, end);
        }
        else
            Hooks?.LineQueued(_world, end);
    }
}

/// <summary>One line of dialogue (or a choice menu), from start to the end of the recording.</summary>
public sealed class DialogueTask : GameTask
{
    readonly DialogueSystem _sys;
    readonly Speaker _speaker;
    readonly string _scene;
    readonly int _line, _menuPosition;
    readonly bool _last, _choice;
    GameTask? _inner;
    bool _started;

    internal DialogueTask(DialogueSystem sys, Speaker speaker, string scene, int line, int menuPosition, bool last, bool choice)
    {
        _choice = choice;
        Name = "DIALOG";
        _sys = sys;
        _speaker = speaker;
        _scene = scene;
        _line = line;
        _menuPosition = menuPosition;
        _last = last;
    }

    protected internal override bool Run(GameWorld world)
    {
        if (!_started)
        {
            _started = true;
            var entry = _sys.Lookup(_speaker, _choice, _scene, _line);
            if (entry == null)
            {
                Console.Error.WriteLine($"dialogue: no {(_choice ? "menu" : "line")} {_speaker.Folder} Q{world.Quest} E{world.Episode} {_scene} {_line}");
                _sys.LineQueued(this, _last);
                return true;
            }
            if (entry.IsChoice)
            {
                var key = DialogueSystem.MenuKey(_speaker, _scene, _line);
                var asked = _sys.Asked(key);
                var open = _sys.Hooks?.AvailableOptions(world, _scene, _line, entry.Options.Count)?.ToList()
                           ?? Enumerable.Range(1, entry.Options.Count).Where(i => !asked.Contains(i)).ToList();
                open = open.Where(i => i >= 1 && i <= entry.Options.Count && entry.Options[i - 1].Text.Length > 0).ToList();
                if (open.Count == 0)
                {
                    _sys.LineQueued(this, _last);
                    return true;
                }
                if (open.Count == 1)
                {
                    _inner = _sys.Choose(world, this, _speaker, entry, _scene, _line, open[0], key);
                    return _inner.Run(world);
                }
                else
                {
                    var menu = new ChoiceTask(_sys, this, _speaker, entry, open, _scene, _line, _menuPosition, key);
                    _sys.OpenMenu = menu;
                    _sys.Hooks?.ChoiceOpened(world);
                    _inner = menu;
                    return _inner.Run(world);
                }
            }
            else
                _inner = new SpokenLine(_sys, _speaker, entry.Options.Count > 0 ? entry.Options[0] : ("", ""));
            _sys.LineQueued(this, _last);
        }
        return _inner!.Run(world);
    }
}

/// <summary>Speaks one line: box, talking head, recording and text.</summary>
sealed class SpokenLine : GameTask
{
    readonly DialogueSystem _sys;
    readonly Speaker _speaker;
    readonly (string Text, string Speech) _line;
    SpeechTask? _speech;

    public SpokenLine(DialogueSystem sys, Speaker speaker, (string Text, string Speech) line)
    {
        _sys = sys;
        _speaker = speaker;
        _line = line;
    }

    protected internal override bool Run(GameWorld world)
    {
        if (_speech == null)
        {
            _sys.LineStarting(_speaker.Code);
            world.CursorVisible = false;
            if (world.DialogueBox)
            {
                _sys.ShowBox(true);
                world.SetAnimPosition(_speaker.HeadTalk, 10, DialogueSystem.BoxY - 8);
                world.PlayAnim(_speaker.HeadTalk, -1);
            }
            _speech = new SpeechTask(_line.Speech, _line.Text, 50, DialogueSystem.BoxY);
        }
        if (!_speech.Run(world))
            return false;
        world.StopAnim(_speaker.HeadTalk);
        _sys.ShowBox(false);
        return true;
    }
}

/// <summary>A choice menu: shows the remaining options and waits for the player's click.</summary>
public sealed class ChoiceTask : GameTask
{
    readonly DialogueSystem _sys;
    readonly Speaker _speaker;
    readonly DialogEntry _entry;
    readonly List<int> _options;
    readonly string _scene, _key;
    readonly int _line, _top;
    readonly List<TextItem> _texts = new();
    bool _shown;

    readonly DialogueTask _owner;
    GameTask? _answer;

    internal ChoiceTask(DialogueSystem sys, DialogueTask owner, Speaker speaker, DialogEntry entry, List<int> options, string scene, int line,
        int position, string key)
    {
        _owner = owner;
        Name = "CHOICE";
        _sys = sys;
        _speaker = speaker;
        _entry = entry;
        _options = options;
        _scene = scene;
        _line = line;
        _key = key;
        _top = position == 0 ? 20 : position == 1 ? 260 : 350;
    }

    /// <summary>Screen rectangles of the options, top to bottom.</summary>
    public IEnumerable<(int Option, int Y0, int Y1)> Rows =>
        _options.Select((o, i) => (o, _top + i * 70, _top + i * 70 + 50));

    public IReadOnlyList<TextItem> Texts => _texts;

    /// <summary>The player clicked at (x, y); returns true if it picked an option.</summary>
    public bool Click(GameWorld world, int x, int y)
    {
        foreach (var (option, y0, y1) in Rows)
        {
            if (y < y0 || y >= y1)
                continue;
            Close(world);
            _answer = _sys.Choose(world, _owner, _speaker, _entry, _scene, _line, option, _key);
            return true;
        }
        return false;
    }

    void Close(GameWorld world)
    {
        foreach (var t in _texts)
            world.Texts.Remove(t);
        world.ShowObject("COMMON_TEXTBOX1", false);
        world.StopAnim(_speaker.HeadTalk);
        if (_sys.OpenMenu == this)
            _sys.OpenMenu = null;
    }

    protected internal override bool Run(GameWorld world)
    {
        if (!_shown)
        {
            _shown = true;
            world.MoveObject("COMMON_TEXTBOX1", 4, _top - 10);
            world.ShowObject("COMMON_TEXTBOX1", true);
            world.SetAnimPosition(_speaker.HeadStill, 10, _top - 8);
            world.PlayAnim(_speaker.HeadStill, -1);
            foreach (var (option, y0, _) in Rows)
            {
                var t = new TextItem { Text = _entry.Options[option - 1].Text, X = 50, Y = y0 };
                _texts.Add(t);
                world.Texts.Add(t);
            }
            world.CursorVisible = true;
            world.CursorWarp = (50, _top + 10);
        }
        if (_answer == null)
            return false;
        world.StopAnim(_speaker.HeadStill);
        return _answer.Run(world);
    }
}
