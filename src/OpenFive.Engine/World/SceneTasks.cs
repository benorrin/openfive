namespace OpenFive.Engine.World;

/// <summary>A text shown on screen, wrapped between X and <see cref="Right"/>.</summary>
public sealed class TextItem
{
    public required string Text;
    public int X, Y, Right = 780;
}

/// <summary>Shows a text for a number of ticks (-1 = until removed).</summary>
public sealed class TextTask : GameTask
{
    public readonly TextItem Item;
    int _ticks;
    bool _shown;

    public TextTask(TextItem item, int ticks)
    {
        Name = "DISPLAY_TEXT";
        Item = item;
        _ticks = ticks;
    }

    protected internal override bool Run(GameWorld world)
    {
        if (!_shown)
        {
            world.Texts.Add(Item);
            _shown = true;
        }
        if (_ticks == -1)
            return false;
        if (_ticks-- > 0)
            return false;
        world.Texts.Remove(Item);
        return true;
    }

    /// <summary>Removes the text now.</summary>
    public void Close(GameWorld world)
    {
        world.Texts.Remove(Item);
        world.Tasks.Remove(this);
    }
}

/// <summary>
/// Plays a speech recording with its text on screen; finishes when the recording ends. If the
/// recording is missing, the text stays for a time based on its length.
/// </summary>
public sealed class SpeechTask : GameTask
{
    readonly string _speech;
    readonly TextItem? _text;
    int _voice = -1;
    int _fallbackTicks;

    public SpeechTask(string speech, string text, int x, int y)
    {
        Name = "TALK";
        _speech = speech;
        if (text.Length > 0)
            _text = new TextItem { Text = text, X = x, Y = y };
    }

    protected internal override bool Run(GameWorld world)
    {
        if (_voice == -1)
        {
            _voice = _speech.Length > 0 ? world.Audio.PlaySpeech(_speech) : 0;
            if (_text != null)
                world.Texts.Add(_text);
            _fallbackTicks = _voice == 0 ? 10 + (_text?.Text.Length ?? 0) * 6 / 10 : 0;
            return false;
        }
        bool playing = _voice != 0 ? world.Audio.IsPlaying(_voice) : _fallbackTicks-- > 0 && !world.SkipRequested;
        if (playing)
            return false;
        if (_text != null)
            world.Texts.Remove(_text);
        return true;
    }
}

/// <summary>Shows or hides the mouse pointer (optionally moving it) when it runs.</summary>
public sealed class CursorTask : GameTask
{
    readonly bool _visible;
    readonly int _x, _y;

    public CursorTask(bool visible, int x = -1, int y = -1)
    {
        Name = "SWITCHMOUSE";
        _visible = visible;
        _x = x;
        _y = y;
    }

    protected internal override bool Run(GameWorld world)
    {
        world.CursorVisible = _visible;
        if (_x != -1)
            world.CursorWarp = (_x, _y);
        return true;
    }
}

/// <summary>Puts a character at a position and state when it runs.</summary>
public sealed class PlaceActorTask : GameTask
{
    readonly Actor _actor;
    readonly int _x, _y, _state;

    public PlaceActorTask(Actor actor, int x, int y, int state)
    {
        Name = "PLACE";
        _actor = actor;
        _x = x;
        _y = y;
        _state = state;
    }

    protected internal override bool Run(GameWorld world)
    {
        world.PlaceActor(_actor, _x, _y, _state);
        return true;
    }
}

/// <summary>Removes a character from the scene when it runs.</summary>
public sealed class HideActorTask : GameTask
{
    readonly Actor _actor;
    public HideActorTask(Actor actor)
    {
        Name = "HIDE";
        _actor = actor;
    }

    protected internal override bool Run(GameWorld world)
    {
        world.HideActor(_actor);
        return true;
    }
}
