using OpenFive.Engine.Formats;
using OpenFive.Engine.IO;

namespace OpenFive.Engine;

/// <summary>The language the game is played in (the disc has both).</summary>
public enum Language
{
    /// <summary>English texts and speech.</summary>
    English,
    /// <summary>German texts and speech.</summary>
    German,
}

/// <summary>
/// Access to the game's assets on the user's disc, with decoding and caching. Nothing from the
/// original game ships with OpenFive; everything is read from the CD at runtime.
/// </summary>
public sealed class GameData : IDisposable
{
    readonly IGameFiles _files;
    readonly Dictionary<string, Image16> _raw = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, PackedSheet> _packed = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, AudioClip?> _audio = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, ResScript> _res = new(StringComparer.OrdinalIgnoreCase);
    Dictionary<string, string>? _speechIndex;

    /// <summary>The language texts and speech are read in (call <see cref="LoadTexts"/> after changing it).</summary>
    public Language Language { get; set; }

    /// <summary>Where the files come from (disc image or folder), for messages.</summary>
    public string Source => _files.Source;

    /// <summary>The objects' hover texts.</summary>
    public ObjectTexts ObjectTexts { get; private set; } = new();

    /// <summary>The dialogue lines and menus.</summary>
    public DialogTexts Dialogs { get; private set; } = new();

    /// <summary>Opens the game's files and reads the texts of <paramref name="language"/>.</summary>
    /// <exception cref="InvalidDataException">The files are not from the game's CD.</exception>
    public GameData(IGameFiles files, Language language = Language.English)
    {
        _files = files;
        Language = language;
        if (!_files.Exists(ResPath("common.res")))
            throw new InvalidDataException(
                $"'{files.Source}' does not look like the Famous Five: Treasure Island CD (Res/common.res is missing).");
        LoadTexts();
    }

    string LangDir => Language == Language.German ? "DEUTSCH" : "ENGLISH";
    string WavLangDir => Language == Language.German ? "Deutsch" : "English";

    /// <summary>Paths inside .res files are relative to the "Res" folder ("\bmp\...") or the disc root ("res\bmp\...").</summary>
    public static string ResPath(string path)
    {
        string p = path.Replace('\\', '/').TrimStart('/');
        return p.StartsWith("res/", StringComparison.OrdinalIgnoreCase) ? p : "Res/" + p;
    }

    /// <summary>Does a file exist (path relative to the Res folder)?</summary>
    public bool Exists(string path) => _files.Exists(ResPath(path));

    /// <summary>Reads a whole file (path relative to the Res folder).</summary>
    public byte[] Read(string path) => _files.ReadAll(ResPath(path));

    /// <summary>The names of the entries of a folder (relative to the Res folder).</summary>
    public IEnumerable<string> List(string dir) => _files.List(ResPath(dir));

    /// <summary>(Re)reads the object and dialogue texts of the current language.</summary>
    public void LoadTexts()
    {
        ObjectTexts = new ObjectTexts();
        foreach (string f in _files.List($"Res/TXT/OBJECT/{LangDir}"))
            if (f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                ObjectTexts.Load(_files.ReadAll($"Res/TXT/OBJECT/{LangDir}/{f}"));
        Dialogs = new DialogTexts();
        foreach (string speaker in _files.List($"Res/TXT/DIALOG/{LangDir}"))
            foreach (string f in _files.List($"Res/TXT/DIALOG/{LangDir}/{speaker}"))
                if (f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    Dialogs.Load(_files.ReadAll($"Res/TXT/DIALOG/{LangDir}/{speaker}/{f}"), speaker);
        _speechIndex = null;
    }

    /// <summary>The ".res" description of a location (cached).</summary>
    public ResScript Location(string name)
    {
        if (!_res.TryGetValue(name, out var r))
            _res[name] = r = ResScript.Parse(Read(ResName(name) + ".res"));
        return r;
    }

    /// <summary>The ".res" file of a location; a few close-up scenes are named differently.</summary>
    public static string ResName(string location) => location.ToLowerInvariant() switch
    {
        "schiffc1" => "schiffcl1",
        "schiffc2" => "schiffcl2",
        "schiffc3" => "schiffcl3",
        "lockarte" => "karte",
        _ => location,
    };

    /// <summary>A raw picture (cached until <see cref="TrimCaches"/>).</summary>
    public Image16 Image(string path)
    {
        if (!_raw.TryGetValue(path, out var img))
            _raw[path] = img = Brg.LoadRaw(Read(path));
        return img;
    }

    /// <summary>A packed sprite sheet (cached until <see cref="TrimCaches"/>).</summary>
    public PackedSheet Packed(string path)
    {
        if (!_packed.TryGetValue(path, out var s))
            _packed[path] = s = Brg.LoadPacked(Read(path));
        return s;
    }

    /// <summary>Is the picture file a packed sprite sheet?</summary>
    public bool IsPacked(string path) => Brg.IsPacked(Read(path));

    /// <summary>Opens a Smacker video (a new decoder each time).</summary>
    public Smacker Video(string path) => new(Read(path));

    /// <summary>A decoded sound, or null if it is missing or unreadable (cached).</summary>
    public AudioClip? Sound(string path)
    {
        if (_audio.TryGetValue(path, out var c))
            return c;
        try
        {
            c = _files.Exists(ResPath(path)) ? Wav.Load(Read(path)) : null;
        }
        catch (InvalidDataException)
        {
            c = null;
        }
        _audio[path] = c;
        return c;
    }

    /// <summary>Finds a speech recording by name in the current language's WAV folders.</summary>
    public AudioClip? Speech(string name)
    {
        if (_speechIndex == null)
        {
            _speechIndex = new(StringComparer.OrdinalIgnoreCase);
            string root = $"Res/WAV/{WavLangDir}";
            foreach (string dir in _files.List(root))
                foreach (string f in _files.List($"{root}/{dir}"))
                    _speechIndex.TryAdd(Path.GetFileNameWithoutExtension(f), $"WAV/{WavLangDir}/{dir}/{f}");
        }
        return _speechIndex.TryGetValue(name, out string? p) ? Sound(p) : null;
    }

    /// <summary>Drops cached location assets (keeps common ones).</summary>
    public void TrimCaches()
    {
        _raw.Clear();
        _packed.Clear();
    }

    /// <summary>Closes the disc image.</summary>
    public void Dispose() => _files.Dispose();
}
