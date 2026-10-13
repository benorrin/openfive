using OpenFive.Engine;
using OpenFive.Engine.Audio;
using OpenFive.Engine.Graphics;
using OpenFive.Engine.IO;
using OpenFive.Engine.World;
using OpenFive.Platform;

namespace OpenFive;

static class Program
{
    static int Main(string[] args)
    {
        var opt = Options.Parse(args);
        if (opt.ShowHelp)
        {
            Console.WriteLine(Options.Help);
            return 0;
        }
        var settings = Engine.Game.Settings.Load();
        opt.Disc ??= settings.Disc;
        if (opt.Language == null)
            opt.Language = settings.Language.StartsWith("de", StringComparison.OrdinalIgnoreCase) ? Language.German : Language.English;
        if (opt.Disc == null)
        {
            Console.Error.WriteLine("OpenFive needs your Famous Five: Treasure Island CD or a disc image of it.\n" +
                                    "Pass it with --disc PATH (a .cue/.ccd/.img/.bin/.iso file or the mounted CD), " +
                                    "or set OPENFIVE_DISC.\n\n" + Options.Help);
            return 2;
        }
        GameData data;
        try
        {
            data = new GameData(GameFiles.Open(opt.Disc), opt.Language.Value);
        }
        catch (Exception e) when (e is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine("Cannot read the game disc: " + e.Message);
            return 2;
        }
        using (data)
        {
            // Remember where the disc is, so it need not be given again.
            if (settings.Disc != opt.Disc)
            {
                settings.Disc = Path.GetFullPath(opt.Disc);
                settings.Save();
            }
            if (opt.Viewer != null)
                return Viewer.Run(data, opt);
            return new GameApp(data, opt, settings).Run();
        }
    }
}

sealed class Options
{
    public string? Disc = Environment.GetEnvironmentVariable("OPENFIVE_DISC");
    public Language? Language;
    public bool SkipIntro;
    public string? StartLocation, From;
    public bool Fullscreen, Sharp, ShowHelp;
    public string? Viewer;
    public string? Screenshot;
    public string? Track;
    public List<string> Anims = new();
    public List<(string Name, int X, int Y)> Place = new();
    public List<(int X, int Y)> WalkTargets = new();
    public List<(int Speaker, string Scene, int Line)> Say = new();
    public int Quest = 1, Episode = 1;
    public List<int> Items = new();
    public string? Script;
    public int Frames = -1;

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
        "  --walk X,Y         viewer: hero walks to X,Y (repeatable, one after another)\n" +
        "  --track PATH       viewer: play a background track (e.g. wav/atmo/Geheim.WAV)\n" +
        "  --screenshot FILE  with --frames N: render N frames off-screen, save a BMP and exit\n";

    public static Options Parse(string[] args)
    {
        var o = new Options();
        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i];
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"{a} needs a value");
            switch (a)
            {
                case "--disc": o.Disc = Next(); break;
                case "--lang": o.Language = Next().StartsWith("de", StringComparison.OrdinalIgnoreCase) ? Engine.Language.German : Engine.Language.English; break;
                case "--skip-intro": o.SkipIntro = true; break;
                case "--loc": o.StartLocation = Next(); break;
                case "--from": o.From = Next(); break;
                case "--fullscreen": o.Fullscreen = true; break;
                case "--sharp": o.Sharp = true; break;
                case "--view": o.Viewer = Next(); break;
                case "--screenshot": o.Screenshot = Next(); break;
                case "--frames": o.Frames = int.Parse(Next()); break;
                case "--track": o.Track = Next(); break;
                case "--anim": o.Anims.Add(Next()); break;
                case "--place":
                {
                    string[] p = Next().Split(':', ',');
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
                case "--item": o.Items.Add(int.Parse(Next())); break;
                case "--script": o.Script = Next(); break;
                case "--walk":
                {
                    string[] p = Next().Split(',');
                    o.WalkTargets.Add((int.Parse(p[0]), int.Parse(p[1])));
                    break;
                }
                case "-h":
                case "--help": o.ShowHelp = true; break;
                default:
                    if (o.Disc == null && !a.StartsWith('-'))
                        o.Disc = a;
                    else
                        throw new ArgumentException("unknown option " + a);
                    break;
            }
        }
        return o;
    }
}

/// <summary>Developer scene viewer: a location with characters, pointer, labels and the rucksack.</summary>
static class Viewer
{
    public static int Run(GameData data, Options opt)
    {
        var fb = new Framebuffer();
        var mixer = new AudioMixer();
        var audio = new GameAudio(data, mixer);
        var world = new GameWorld(data, audio);
        var session = new Engine.Game.GameSession(world, Engine.Game.Scripts.For);
        world.Quest = opt.Quest;
        world.Episode = opt.Episode;
        session.EnterLocation(opt.Viewer!);
        if (opt.Track != null)
            audio.SetBackgroundTrack(opt.Track);
        foreach (var (name, x, y) in opt.Place)
        {
            var actor = world.Actors.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (actor == null)
            {
                Console.Error.WriteLine($"unknown character {name}");
                continue;
            }
            world.PlaceActor(actor, x, y, Engine.World.ActorStates.StandDownRight);
            if (name == opt.Place[0].Name)
                world.Hero = actor;
        }
        // Further placed characters follow the one before them (the party following the hero).
        for (int i = 1; i < opt.Place.Count; i++)
        {
            var lead = world.Actors.First(a => a.Name.Equals(opt.Place[i - 1].Name, StringComparison.OrdinalIgnoreCase));
            lead.Follower = world.Actors.First(a => a.Name.Equals(opt.Place[i].Name, StringComparison.OrdinalIgnoreCase));
        }
        foreach (int item in opt.Items)
            session.GiveItem(item);
        foreach (var (x, y) in opt.WalkTargets)
            world.WalkToAfter(world.Hero, x, y);
        for (int i = 0; i < opt.Say.Count; i++)
            world.Dialogue.Say(opt.Say[i].Speaker, opt.Say[i].Scene, opt.Say[i].Line, last: i == opt.Say.Count - 1);
        foreach (string a in opt.Anims)
            if (world.PlayAnim(a, -1) == null)
                Console.Error.WriteLine($"unknown animation {a}");
        var scene = world.Scene!;

        void Tick()
        {
            scene.Update(100);
            session.Tick();
            world.Update();
        }

        void Render()
        {
            world.Draw(fb);
            session.DrawOverlay(fb);
        }

        if (opt.Screenshot != null || opt.Script != null)
        {
            var script = opt.Script != null ? InputScript.Load(opt.Script) : new InputScript();
            int frames = Math.Max(1, opt.Frames);
            for (int t = 0; t < frames; t++)
            {
                foreach (var cmd in script.At(t))
                {
                    switch (cmd.Op)
                    {
                        case "move":
                            session.MoveMouse(cmd.X, cmd.Y);
                            break;
                        case "click":
                            session.MoveMouse(cmd.X, cmd.Y);
                            session.Click();
                            break;
                        case "shot":
                            Render();
                            Screenshot.SaveBmp(fb, cmd.Arg);
                            Console.WriteLine($"tick {t}: saved {cmd.Arg}");
                            break;
                    }
                }
                Tick();
            }
            if (opt.Screenshot != null)
            {
                Render();
                Screenshot.SaveBmp(fb, opt.Screenshot);
                Console.WriteLine($"saved {opt.Screenshot}");
            }
            foreach (var a in world.Actors.Where(a => a.Visible))
                Console.WriteLine($"{a.Name}: ({a.X},{a.Y}) state {a.State} shape {a.Shape} scale {a.Scale:F3} depth {a.Depth} queue {a.Queue.Count} sprite ({a.Sprite.X},{a.Sprite.Y}) x{a.Sprite.Scale:F3} frame {a.Sprite.Frame} visible {a.Sprite.Visible} packed {a.Sprite.Packed}");
            return 0;
        }

        using var host = new Host(opt.Fullscreen, !opt.Sharp);
        using var output = new AudioOutput(mixer);
        ulong last = Sdl.GetTicks(), tickClock = 0;
        int frame = 0;
        while (!host.Input.QuitRequested && (opt.Frames < 0 || frame++ < opt.Frames))
        {
            host.PumpEvents();
            while (host.Input.KeysPressed.TryDequeue(out int k))
            {
                if (k == Keys.Escape)
                    host.Input.QuitRequested = true;
                else if (k == Keys.Space && !world.CursorVisible)
                    world.SkipRequested = true;
            }
            session.MoveMouse(host.Input.MouseX, host.Input.MouseY);
            if (host.Input.Clicked)
                session.Click();
            ulong now = Sdl.GetTicks();
            tickClock += now - last;
            last = now;
            while (tickClock >= 100)
            {
                tickClock -= 100;
                Tick();
            }
            Render();
            host.Present(fb, host.Input.MouseInside);
        }
        return 0;
    }
}

/// <summary>Scripted input for automated tests: lines of "TICK move X Y | click X Y | shot FILE".</summary>
sealed class InputScript
{
    public readonly record struct Command(int Tick, string Op, int X, int Y, string Arg);
    readonly List<Command> _cmds = new();

    public static InputScript Load(string path)
    {
        var s = new InputScript();
        foreach (string raw in File.ReadAllLines(path))
        {
            string[] p = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (p.Length < 2 || p[0].StartsWith('#'))
                continue;
            int tick = int.Parse(p[0]);
            if (p[1] == "shot")
                s._cmds.Add(new Command(tick, "shot", 0, 0, p[2]));
            else if (p[1] == "key")
                s._cmds.Add(new Command(tick, "key", int.Parse(p[2]), 0, ""));
            else if (p.Length >= 4)
                s._cmds.Add(new Command(tick, p[1], int.Parse(p[2]), int.Parse(p[3]), ""));
        }
        return s;
    }

    public IEnumerable<Command> At(int tick) => _cmds.Where(c => c.Tick == tick);
}
