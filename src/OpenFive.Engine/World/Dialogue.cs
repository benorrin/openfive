using OpenFive.Engine.Formats;

namespace OpenFive.Engine.World;

/// <summary>A person who can speak: their dialogue folder and talking-head animations.</summary>
public sealed record Speaker(int Code, string Folder, string HeadStill, string HeadTalk);

/// <summary>Called by the dialogue system so a location's script can continue a conversation.</summary>
public interface IDialogueHooks
{
    /// <summary>A line (not the last of its conversation) has been queued; <paramref name="end"/> is released when it has been spoken.</summary>
    void LineQueued(GameWorld world, TaskHandle end) { }

    /// <summary>The last line of a conversation has been queued.</summary>
    void ConversationEnding(GameWorld world, TaskHandle end) { }

    /// <summary>A choice menu was opened (the option the player picks is reported to <see cref="ChoiceMade"/>).</summary>
    void ChoiceOpened(GameWorld world) { }

    /// <summary>The player picked option <paramref name="option"/> (1-3, numbered as in the dialogue file).</summary>
    void ChoiceMade(GameWorld world, string scene, int line, int option) { }
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

    internal DialogEntry? Lookup(Speaker s, string scene, int line) =>
        _world.Data.Dialogs.Get(s.Folder, _world.Quest, _world.Episode, scene, line);

    /// <summary>
    /// Queues a line of <paramref name="speaker"/> from dialogue scene <paramref name="scene"/>.
    /// It starts once <paramref name="after"/> (default: the current sequence) is released. When
    /// the dialogue file holds a choice menu for this line, the player is asked instead.
    /// Returns a handle that is released when the line has been spoken; it also becomes the
    /// current sequence.
    /// </summary>
    public TaskHandle Say(int speaker, string scene, int line, TaskHandle? after = null, int menuPosition = 2,
        bool last = false, int delay = 0)
    {
        var s = SpeakerFor(speaker) ?? Speakers[0];
        var task = _world.Tasks.Add(new DialogueTask(this, s, scene, line, menuPosition, last)
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
    readonly bool _last;
    GameTask? _inner;
    bool _started;

    internal DialogueTask(DialogueSystem sys, Speaker speaker, string scene, int line, int menuPosition, bool last)
    {
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
            var entry = _sys.Lookup(_speaker, _scene, _line);
            if (entry == null)
            {
                Console.Error.WriteLine($"dialogue: no line {_speaker.Folder} Q{world.Quest} E{world.Episode} {_scene} {_line}");
                _sys.LineQueued(this, _last);
                return true;
            }
            if (entry.IsChoice)
            {
                var key = DialogueSystem.MenuKey(_speaker, _scene, _line);
                var asked = _sys.Asked(key);
                var open = Enumerable.Range(1, entry.Options.Count).Where(i => !asked.Contains(i) && entry.Options[i - 1].Text.Length > 0).ToList();
                if (open.Count == 0)
                    open = Enumerable.Range(1, entry.Options.Count).ToList();
                if (open.Count == 1)
                {
                    asked.Add(open[0]);
                    _sys.LastChoice = open[0];
                    _sys.Hooks?.ChoiceMade(world, _scene, _line, open[0]);
                    _inner = new SpokenLine(_sys, _speaker, entry.Options[open[0] - 1]);
                }
                else
                {
                    var menu = new ChoiceTask(_sys, _speaker, entry, open, _scene, _line, _menuPosition, key);
                    _sys.OpenMenu = menu;
                    _sys.Hooks?.ChoiceOpened(world);
                    _inner = menu;
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
    SpokenLine? _spoken;
    bool _shown;

    internal ChoiceTask(DialogueSystem sys, Speaker speaker, DialogEntry entry, List<int> options, string scene, int line,
        int position, string key)
    {
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
            _sys.Asked(_key).Add(option);
            _sys.LastChoice = option;
            _sys.Hooks?.ChoiceMade(world, _scene, _line, option);
            _spoken = new SpokenLine(_sys, _speaker, _entry.Options[option - 1]);
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
        if (_spoken == null)
            return false;
        world.StopAnim(_speaker.HeadStill);
        return _spoken.Run(world);
    }
}
