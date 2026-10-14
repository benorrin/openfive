using System.Text;

namespace OpenFive.Engine.Formats;

/// <summary>Hover/use texts of objects: "OBJECT name" followed by "over:", "use:", "forceuse:" lines.</summary>
public sealed class ObjectTexts
{
    /// <summary>The texts of one object.</summary>
    public sealed class Entry
    {
        /// <summary>Shown while the pointer is over the object.</summary>
        public string Over = "";

        /// <summary>Said when the object is used (unused by OpenFive).</summary>
        public string Use = "";

        /// <summary>Said when an item is forced onto the object (unused by OpenFive).</summary>
        public string ForceUse = "";
    }

    readonly Dictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The texts of an object, or null if it has none.</summary>
    public Entry? Get(string name) => _entries.GetValueOrDefault(name);

    /// <summary>Adds the objects of a text file (later entries replace earlier ones).</summary>
    public void Load(byte[] data)
    {
        Entry? cur = null;
        foreach (string raw in Encoding.Latin1.GetString(data).Split('\n'))
        {
            string line = raw.Trim();
            if (line.StartsWith("OBJECT", StringComparison.OrdinalIgnoreCase))
            {
                string name = line[6..].Trim();
                cur = new Entry();
                _entries[name] = cur;
            }
            else if (cur != null)
            {
                int colon = line.IndexOf(':');
                if (colon < 0)
                    continue;
                string key = line[..colon].Trim().ToLowerInvariant();
                string val = line[(colon + 1)..].Trim();
                switch (key)
                {
                    case "over":
                        cur.Over = val;
                        break;
                    case "use":
                        cur.Use = val;
                        break;
                    case "forceuse":
                        cur.ForceUse = val;
                        break;
                }
            }
        }
    }
}

/// <summary>A line (or, for choices, a set of options) of a character's dialogue file.</summary>
public sealed class DialogEntry
{
    /// <summary>A choice menu ("DLG") rather than a spoken line ("V").</summary>
    public bool IsChoice;

    /// <summary>The story step the entry belongs to.</summary>
    public int Quest, Episode;

    /// <summary>The scene name ("KUECHE0027").</summary>
    public string Scene = "";

    /// <summary>The line number within the scene.</summary>
    public int Id;
    /// <summary>Text and speech file name of each option (one for normal lines).</summary>
    public List<(string Text, string Speech)> Options = new();
    /// <summary>The character directory the file was loaded from (speech files live next to it).</summary>
    public string Speaker = "";
}

/// <summary>
/// Dialogue files ("Res/TXT/DIALOG/&lt;lang&gt;/&lt;char&gt;/&lt;char&gt;.txt"): blocks introduced by
/// "HEAD=V|DLG,Qn,En,SCENE,id" followed by "showN(text)" / "playN(speech)".
/// </summary>
public sealed class DialogTexts
{
    readonly Dictionary<string, DialogEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    static string Key(string speaker, bool choice, int q, int e, string scene, int id) =>
        $"{speaker}|{(choice ? "DLG" : "V")}|{q}|{e}|{scene}|{id}";

    /// <summary>A line (or choice menu) of a speaker's file for the given story step, scene and number.</summary>
    public DialogEntry? Get(string speaker, bool choice, int quest, int episode, string scene, int id) =>
        _entries.TryGetValue(Key(speaker, choice, quest, episode, scene, id), out var d) ? d : null;

    /// <summary>Finds a line of a scene regardless of quest/episode (some scenes are shared).</summary>
    public DialogEntry? Find(string scene, int id) =>
        _entries.Values.FirstOrDefault(d => d.Scene.Equals(scene, StringComparison.OrdinalIgnoreCase) && d.Id == id);

    /// <summary>All entries loaded.</summary>
    public IEnumerable<DialogEntry> All => _entries.Values;

    /// <summary>Adds the entries of one speaker's dialogue file.</summary>
    public void Load(byte[] data, string speaker)
    {
        DialogEntry? cur = null;
        foreach (string raw in Encoding.Latin1.GetString(data).Split('\n'))
        {
            string line = raw.TrimEnd('\r').Trim();
            if (line.StartsWith("HEAD=", StringComparison.OrdinalIgnoreCase))
            {
                string[] p = line[5..].Split(',');
                if (p.Length < 5)
                {
                    cur = null;
                    continue;
                }
                cur = new DialogEntry
                {
                    IsChoice = p[0].Equals("DLG", StringComparison.OrdinalIgnoreCase),
                    Quest = ParseTagged(p[1]),
                    Episode = ParseTagged(p[2]),
                    Scene = p[3].Trim(),
                    Id = int.TryParse(p[4].Trim(), out int id) ? id : 0,
                    Speaker = speaker,
                };
                _entries[Key(speaker, cur.IsChoice, cur.Quest, cur.Episode, cur.Scene, cur.Id)] = cur;
                continue;
            }
            if (cur == null)
                continue;
            int open = line.IndexOf('('), close = line.LastIndexOf(')');
            if (open < 0)
                continue;
            string verb = line[..open].ToLowerInvariant();
            string arg = close > open ? line[(open + 1)..close] : line[(open + 1)..];
            int n = verb.Length > 4 && char.IsDigit(verb[^1]) ? verb[^1] - '1' : 0;
            while (cur.Options.Count <= n)
                cur.Options.Add(("", ""));
            var o = cur.Options[n];
            if (verb.StartsWith("show"))
                cur.Options[n] = (arg, o.Speech);
            else if (verb.StartsWith("play"))
                cur.Options[n] = (o.Text, arg);
        }
    }

    /// <summary>The number of a tagged field ("Q5" = 5); 0 if there is none.</summary>
    static int ParseTagged(string s)
    {
        s = s.Trim();
        return s.Length > 1 && int.TryParse(s[1..], out int v) ? v : 0;
    }
}
