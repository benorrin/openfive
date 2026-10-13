using System.Text;

namespace OpenFive.Engine.Formats;

/// <summary>Hover/use texts of objects: "OBJECT name" followed by "over:", "use:", "forceuse:" lines.</summary>
public sealed class ObjectTexts
{
    public sealed class Entry
    {
        public string Over = "", Use = "", ForceUse = "";
    }

    readonly Dictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);

    public Entry? Get(string name) => _entries.TryGetValue(name, out var e) ? e : null;

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
                    case "over": cur.Over = val; break;
                    case "use": cur.Use = val; break;
                    case "forceuse": cur.ForceUse = val; break;
                }
            }
        }
    }
}

/// <summary>A line (or, for choices, a set of options) of a character's dialogue file.</summary>
public sealed class DialogEntry
{
    public bool IsChoice;
    public int Quest, Episode;
    public string Scene = "";
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

    static string Key(int q, int e, string scene, int id) => $"{q}|{e}|{scene}|{id}";

    public DialogEntry? Get(int quest, int episode, string scene, int id) =>
        _entries.TryGetValue(Key(quest, episode, scene, id), out var d) ? d : null;

    /// <summary>Finds a line of a scene regardless of quest/episode (some scenes are shared).</summary>
    public DialogEntry? Find(string scene, int id) =>
        _entries.Values.FirstOrDefault(d => d.Scene.Equals(scene, StringComparison.OrdinalIgnoreCase) && d.Id == id);

    public IEnumerable<DialogEntry> All => _entries.Values;

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
                _entries[Key(cur.Quest, cur.Episode, cur.Scene, cur.Id)] = cur;
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

    static int ParseTagged(string s)
    {
        s = s.Trim();
        return s.Length > 1 && int.TryParse(s[1..], out int v) ? v : 0;
    }
}
