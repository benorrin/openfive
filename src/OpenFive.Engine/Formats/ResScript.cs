using System.Globalization;
using System.Text;

namespace OpenFive.Engine.Formats;

public enum ResourceKind { Gfx, Smk, Sfx }

public sealed record ResourceEntry(ResourceKind Kind, string Path, bool Packed, bool NoLoad);

/// <summary>One frame of an animation: sheet frame index and drawing offset.</summary>
public readonly record struct AnimFrame(int Frame, int X, int Y);

/// <summary>A named animation over a resource: "anim Name,res,w,h,x,y:frame,dx,dy;...".</summary>
public sealed record AnimDef(string Name, int Resource, int Width, int Height, int X, int Y, AnimFrame[] Frames);

/// <summary>A point of a walk path/area polygon with its depth (draw order) and scale (percent).</summary>
public readonly record struct WalkPoint(int X, int Y, int Depth, int Scale);

/// <summary>Connection from a path/area to another one (1-based index into the combined list).</summary>
public readonly record struct WalkLink(int Target, int X, int Y);

public sealed record WalkShape(bool IsArea, WalkPoint[] Points, WalkLink[] Links);

/// <summary>Sample point of the depth mask: pixels of the mask region at (X,Y) lie at Depth.</summary>
public readonly record struct MaskSample(int X, int Y, int Depth);

public enum ObjectCommand { None, Examine, ChangeLocation }

public sealed class ObjectDef
{
    public string Name = "";
    public bool On;
    public int Gfx = -1, GfxX, GfxY;
    public int Depth;
    public double Size = 1.0;
    public (int X, int Y)[] Click = Array.Empty<(int, int)>();
    public int GotoX, GotoY;
    public ObjectCommand Command;
    public string CommandArg = "";
}

/// <summary>
/// A parsed ".res" location description: resources, animations, background, walkable
/// paths/areas and clickable objects.
/// </summary>
public sealed class ResScript
{
    public string Name = "";
    public List<ResourceEntry> Resources = new();
    public List<AnimDef> Anims = new();
    public int Background = -1;
    public string? MaskPath;
    public List<MaskSample> MaskSamples = new();
    public List<WalkShape> Paths = new();
    public List<WalkShape> Areas = new();
    public List<ObjectDef> Objects = new();

    static int Int(string s) => int.Parse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture);

    public static ResScript Parse(byte[] data)
    {
        var res = new ResScript();
        string text = Encoding.Latin1.GetString(data);
        string section = "";
        ObjectDef? obj = null;
        foreach (string raw in text.Split('\n'))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line == "{")
                continue;
            if (line == "}")
            {
                if (obj != null)
                {
                    res.Objects.Add(obj);
                    obj = null;
                }
                continue;
            }
            int sp = line.IndexOfAny(new[] { ' ', '\t' });
            string key = sp < 0 ? line : line[..sp];
            string arg = sp < 0 ? "" : line[(sp + 1)..].Trim();
            switch (key.ToLowerInvariant())
            {
                case "location":
                    res.Name = arg;
                    continue;
                case "path" when section == "":
                    continue; // original build path, unused
                case "resource":
                case "back_ground":
                case "movement":
                case "objects":
                    section = key.ToLowerInvariant();
                    continue;
            }
            if (obj != null)
            {
                ParseObjectLine(obj, key.ToLowerInvariant(), arg);
                continue;
            }
            switch (section)
            {
                case "resource":
                    ParseResource(res, key.ToLowerInvariant(), arg);
                    break;
                case "back_ground":
                    if (key.Equals("background", StringComparison.OrdinalIgnoreCase))
                        res.Background = Int(arg);
                    else if (key.Equals("lmsk", StringComparison.OrdinalIgnoreCase))
                    {
                        int colon = arg.IndexOf(':');
                        res.MaskPath = colon < 0 ? arg : arg[..colon];
                        if (colon >= 0)
                            foreach (string s in arg[(colon + 1)..].Split(';', StringSplitOptions.RemoveEmptyEntries))
                            {
                                string[] p = s.Split(',');
                                res.MaskSamples.Add(new MaskSample(Int(p[0]), Int(p[1]), Int(p[2])));
                            }
                    }
                    break;
                case "movement":
                    if (key is "path" or "area")
                        (key == "path" ? res.Paths : res.Areas).Add(ParseShape(key == "area", arg));
                    break;
                case "objects":
                    if (key.Equals("object", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] p = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        obj = new ObjectDef { Name = p[0], On = p.Length > 1 && p[1].Equals("ON", StringComparison.OrdinalIgnoreCase) };
                    }
                    break;
            }
        }
        return res;
    }

    static void ParseResource(ResScript res, string key, string arg)
    {
        switch (key)
        {
            case "gfx":
            case "smk":
            case "sfx":
            {
                string[] p = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                bool packed = p.Skip(1).Any(x => x.Equals("PACKED", StringComparison.OrdinalIgnoreCase));
                bool noLoad = p.Skip(1).Any(x => x.Equals("NO_LOAD", StringComparison.OrdinalIgnoreCase));
                var kind = key == "gfx" ? ResourceKind.Gfx : key == "smk" ? ResourceKind.Smk : ResourceKind.Sfx;
                res.Resources.Add(new ResourceEntry(kind, p[0], packed, noLoad));
                break;
            }
            case "anim":
            {
                int colon = arg.IndexOf(':');
                string[] h = (colon < 0 ? arg : arg[..colon]).Split(',');
                var frames = new List<AnimFrame>();
                if (colon >= 0)
                    foreach (string f in arg[(colon + 1)..].Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] q = f.Split(',');
                        frames.Add(new AnimFrame(Int(q[0]), q.Length > 1 ? Int(q[1]) : 0, q.Length > 2 ? Int(q[2]) : 0));
                    }
                res.Anims.Add(new AnimDef(h[0].Trim(), Int(h[1]), Int(h[2]), Int(h[3]), Int(h[4]), Int(h[5]), frames.ToArray()));
                break;
            }
        }
    }

    static WalkShape ParseShape(bool area, string arg)
    {
        int colon = arg.IndexOf(':');
        var pts = new List<WalkPoint>();
        foreach (string s in (colon < 0 ? arg : arg[..colon]).Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] p = s.Split(',');
            pts.Add(new WalkPoint(Int(p[0]), Int(p[1]), Int(p[2]), Int(p[3])));
        }
        var links = new List<WalkLink>();
        if (colon >= 0)
            foreach (string s in arg[(colon + 1)..].Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] p = s.Split(',');
                links.Add(new WalkLink(Int(p[0]), Int(p[1]), Int(p[2])));
            }
        return new WalkShape(area, pts.ToArray(), links.ToArray());
    }

    static void ParseObjectLine(ObjectDef o, string key, string arg)
    {
        switch (key)
        {
            case "gfx":
            {
                string[] p = arg.Split(':');
                o.Gfx = Int(p[0]);
                if (p.Length > 1)
                {
                    string[] xy = p[1].Split(',');
                    o.GfxX = Int(xy[0]);
                    o.GfxY = Int(xy[1]);
                }
                break;
            }
            case "depth":
                o.Depth = Int(arg);
                break;
            case "size":
                o.Size = double.Parse(arg, CultureInfo.InvariantCulture);
                break;
            case "click":
                o.Click = arg.Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Split(','))
                    .Select(p => (Int(p[0]), Int(p[1])))
                    .ToArray();
                break;
            case "goto":
            {
                string[] p = arg.Split(',');
                o.GotoX = Int(p[0]);
                o.GotoY = Int(p[1]);
                break;
            }
            case "cmd":
            {
                string[] p = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                o.Command = p.Length == 0 ? ObjectCommand.None
                    : p[0].Equals("CMD_CHANGELOC", StringComparison.OrdinalIgnoreCase) ? ObjectCommand.ChangeLocation
                    : ObjectCommand.Examine;
                o.CommandArg = p.Length > 1 ? p[1] : "";
                break;
            }
        }
    }
}
