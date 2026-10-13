using OpenFive.Engine.Formats;
using OpenFive.Engine.IO;

namespace OpenFive.Engine;

public enum Language { English, German }

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

    public Language Language { get; set; }
    public string Source => _files.Source;
    public ObjectTexts ObjectTexts { get; private set; } = new();
    public DialogTexts Dialogs { get; private set; } = new();

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

    public bool Exists(string path) => _files.Exists(ResPath(path));
    public byte[] Read(string path) => _files.ReadAll(ResPath(path));
    public IEnumerable<string> List(string dir) => _files.List(ResPath(dir));

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

    public Image16 Image(string path)
    {
        if (!_raw.TryGetValue(path, out var img))
            _raw[path] = img = Brg.LoadRaw(Read(path));
        return img;
    }

    public PackedSheet Packed(string path)
    {
        if (!_packed.TryGetValue(path, out var s))
            _packed[path] = s = Brg.LoadPacked(Read(path));
        return s;
    }

    public bool IsPacked(string path) => Brg.IsPacked(Read(path));

    public Smacker Video(string path) => new(Read(path));

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

    public void Dispose() => _files.Dispose();
}
