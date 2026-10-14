using OpenFive.Engine;

namespace OpenFive;

/// <summary>The command-line options.</summary>
sealed class Options
{
    /// <summary>The game disc (image or folder); $OPENFIVE_DISC by default.</summary>
    public string? Disc = Environment.GetEnvironmentVariable("OPENFIVE_DISC");

    /// <summary>The language (null: from the settings).</summary>
    public Language? Language;

    public bool SkipIntro, Fullscreen, Sharp, ShowHelp;

    /// <summary>Start a game directly in this location (coming from <see cref="From"/>).</summary>
    public string? StartLocation, From;

    /// <summary>Developer viewer: the location to show.</summary>
    public string? Viewer;

    /// <summary>Off-screen run: the screenshot to save after <see cref="Frames"/> ticks.</summary>
    public string? Screenshot;

    /// <summary>Viewer: a background track to play.</summary>
    public string? Track;

    /// <summary>Viewer: animations to loop.</summary>
    public List<string> Anims = new();

    /// <summary>Viewer: characters to place (the first is the hero).</summary>
    public List<(string Name, int X, int Y)> Place = new();

    /// <summary>Viewer: points the hero walks to, one after another.</summary>
    public List<(int X, int Y)> WalkTargets = new();

    /// <summary>Viewer: lines of dialogue to say, in order.</summary>
    public List<(int Speaker, string Scene, int Line)> Say = new();

    /// <summary>The story step to start at.</summary>
    public int Quest = 1, Episode = 1;

    /// <summary>Items to put in the rucksack.</summary>
    public List<int> Items = new();

    /// <summary>Viewer: scripted input file.</summary>
    public string? Script;

    /// <summary>Automated play-through command file.</summary>
    public string? Play;

    /// <summary>Ticks to run (-1: until quit).</summary>
    public int Frames = -1;

    /// <summary>The usage text.</summary>
    public const string Help =
        "usage: openfive --disc PATH [options]\n" +
        "  --disc PATH        game CD image (.cue, .ccd, .img, .bin, .iso) or mounted CD directory\n" +
        "  --lang en|de       language (default en)\n" +
        "  --fullscreen       start in fullscreen (F11 or Alt+Enter toggles)\n" +
        "  --sharp            sharp pixel scaling instead of smooth (F12 toggles)\n" +
        "  --skip-intro       start at the main menu\n" +
        "developer options:\n" +
        "  --loc NAME         start a game directly in a location (with --from PREVIOUS, --quest Q,E, --item N)\n" +
        "  --view LOCATION    developer scene viewer (e.g. loc05)\n" +
        "  --anim NAME        viewer: loop a named animation (repeatable)\n" +
        "  --place NAME:X,Y   viewer: put a character (Dick, George, ...) at X,Y; the first one is the hero\n" +
        "  --quest Q,E        viewer: story step for dialogue lookups\n" +
        "  --say S,SCENE,N    viewer: speaker code S says line N of SCENE (repeatable, in order)\n" +
        "  --item N           viewer: put item N (1-32) in the rucksack (repeatable)\n" +
        "  --script FILE      viewer: scripted input (\"TICK move X Y | click X Y | shot FILE\"), run --frames ticks\n" +
        "  --play FILE        play through the game off-screen from a command list (see PlayRunner.cs)\n" +
        "  --walk X,Y         viewer: hero walks to X,Y (repeatable, one after another)\n" +
        "  --track PATH       viewer: play a background track (e.g. wav/atmo/Geheim.WAV)\n" +
        "  --screenshot FILE  with --frames N: render N frames off-screen, save a BMP and exit\n";

    /// <summary>Parses the command line; a bare argument is the disc.</summary>
    /// <exception cref="ArgumentException">An unknown option, or an option without its value.</exception>
    public static Options Parse(string[] args)
    {
        var o = new Options();
        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i];
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"{a} needs a value");
            string[] NextList() => Next().Split(':', ',');
            switch (a)
            {
                case "--disc":
                    o.Disc = Next();
                    break;
                case "--lang":
                    o.Language = Next().StartsWith("de", StringComparison.OrdinalIgnoreCase)
                        ? Engine.Language.German
                        : Engine.Language.English;
                    break;
                case "--skip-intro":
                    o.SkipIntro = true;
                    break;
                case "--loc":
                    o.StartLocation = Next();
                    break;
                case "--from":
                    o.From = Next();
                    break;
                case "--fullscreen":
                    o.Fullscreen = true;
                    break;
                case "--sharp":
                    o.Sharp = true;
                    break;
                case "--view":
                    o.Viewer = Next();
                    break;
                case "--screenshot":
                    o.Screenshot = Next();
                    break;
                case "--frames":
                    o.Frames = int.Parse(Next());
                    break;
                case "--track":
                    o.Track = Next();
                    break;
                case "--anim":
                    o.Anims.Add(Next());
                    break;
                case "--place":
                {
                    string[] p = NextList();
                    o.Place.Add((p[0], int.Parse(p[1]), int.Parse(p[2])));
                    break;
                }
                case "--say":
                {
                    string[] p = Next().Split(',');
                    o.Say.Add((int.Parse(p[0]), p[1], int.Parse(p[2])));
                    break;
                }
                case "--quest":
                {
                    string[] p = Next().Split(',');
                    (o.Quest, o.Episode) = (int.Parse(p[0]), int.Parse(p[1]));
                    break;
                }
                case "--item":
                    o.Items.Add(int.Parse(Next()));
                    break;
                case "--script":
                    o.Script = Next();
                    break;
                case "--play":
                    o.Play = Next();
                    break;
                case "--walk":
                {
                    string[] p = Next().Split(',');
                    o.WalkTargets.Add((int.Parse(p[0]), int.Parse(p[1])));
                    break;
                }
                case "-h" or "--help":
                    o.ShowHelp = true;
                    break;
                default:
                    if (o.Disc != null || a.StartsWith('-'))
                        throw new ArgumentException("unknown option " + a);
                    o.Disc = a;
                    break;
            }
        }
        return o;
    }
}
