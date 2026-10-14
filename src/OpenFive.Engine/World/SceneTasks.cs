namespace OpenFive.Engine.World;

/// <summary>A text shown on screen, wrapped between X and <see cref="Right"/>.</summary>
public sealed class TextItem
{
    /// <summary>The text.</summary>
    public required string Text;

    /// <summary>Top left corner of the first line.</summary>
    public int X, Y;

    /// <summary>Right edge the lines are wrapped at.</summary>
    public int Right = 780;
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

    /// <summary>
    /// Creates the task: <paramref name="speech"/> is a speech file name, or a path with a folder
    /// for a sound effect ("" for silence); <paramref name="text"/> ("" for none) is shown at (x, y).
    /// </summary>
    public SpeechTask(string speech, string text, int x, int y)
    {
        Name = "TALK";
        _speech = speech;
        if (text.Length > 0)
            _text = new TextItem { Text = text, X = x, Y = y };
    }

    /// <summary>Starts the recording and shows the text; finishes when the recording has ended.</summary>
    protected internal override bool Run(GameWorld world)
    {
        if (_voice == -1)
        {
            _voice = _speech.Length == 0 ? 0
                : _speech.Contains('\\') || _speech.Contains('/') ? world.Audio.PlayEffect(_speech)
                : world.Audio.PlaySpeech(_speech);
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
