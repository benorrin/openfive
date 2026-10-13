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
            data = new GameData(GameFiles.Open(opt.Disc), opt.Language);
        }
        catch (Exception e) when (e is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine("Cannot read the game disc: " + e.Message);
            return 2;
        }
        using (data)
        {
            if (opt.Viewer != null)
                return Viewer.Run(data, opt);
            Console.Error.WriteLine("The full game is not playable yet in this build; use --view LOCATION to explore scenes.");
            return 1;
        }
    }
}

sealed class Options
{
    public string? Disc = Environment.GetEnvironmentVariable("OPENFIVE_DISC");
    public Language Language = Language.English;
    public bool Fullscreen, Sharp, ShowHelp;
    public string? Viewer;
    public string? Screenshot;
    public string? Track;
    public List<string> Anims = new();
    public int Frames = -1;

    public const string Help =
        "usage: openfive --disc PATH [options]\n" +
        "  --disc PATH        game CD image (.cue, .ccd, .img, .bin, .iso) or mounted CD directory\n" +
        "  --lang en|de       language (default en)\n" +
        "  --fullscreen       start in fullscreen (F11 or Alt+Enter toggles)\n" +
        "  --sharp            sharp pixel scaling instead of smooth (F12 toggles)\n" +
        "  --view LOCATION    developer scene viewer (e.g. loc05)\n" +
        "  --anim NAME        viewer: loop a named animation (repeatable)\n" +
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
                case "--lang": o.Language = Next().StartsWith("de", StringComparison.OrdinalIgnoreCase) ? Language.German : Language.English; break;
                case "--fullscreen": o.Fullscreen = true; break;
                case "--sharp": o.Sharp = true; break;
                case "--view": o.Viewer = Next(); break;
                case "--screenshot": o.Screenshot = Next(); break;
                case "--frames": o.Frames = int.Parse(Next()); break;
                case "--track": o.Track = Next(); break;
                case "--anim": o.Anims.Add(Next()); break;
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

/// <summary>Developer scene viewer: shows a location with its objects and hover labels.</summary>
static class Viewer
{
    public static int Run(GameData data, Options opt)
    {
        var fb = new Framebuffer();
        var font = new BitmapFont(data.Image("bmp/common/misc/char.brg"));
        var mixer = new AudioMixer();
        var audio = new GameAudio(data, mixer);
        var world = new GameWorld(data, audio);
        world.LoadLocation(opt.Viewer!);
        var scene = world.Scene!;
        if (opt.Track != null)
            audio.SetBackgroundTrack(opt.Track);
        foreach (string a in opt.Anims)
            if (world.PlayAnim(a, -1) == null)
                Console.Error.WriteLine($"unknown animation {a}");
        void Render(int mx, int my)
        {
            world.Draw(fb);
            var o = scene.ObjectAt(mx, my);
            string label = o != null ? data.ObjectTexts.Get(o.Name)?.Over ?? "" : "";
            if (label.Length > 0)
            {
                int w = font.MeasureWidth(label);
                font.Draw(fb, label, Math.Clamp(mx - w / 2, 0, Framebuffer.Width - w), Math.Max(0, my - 24));
            }
        }
        if (opt.Screenshot != null)
        {
            if (opt.Track != null)
            {
                var buf = new float[44100 * 2];
                mixer.Mix(buf);
                double rms = Math.Sqrt(buf.Average(x => (double)x * x));
                Console.WriteLine($"audio: {opt.Track} rms over 1s = {rms:F4}");
            }
            int frames = Math.Max(1, opt.Frames);
            for (int i = 0; i < frames; i++)
            {
                scene.Update(100);
                world.Update();
            }
            int mx = 400, my = 300;
            var hover = scene.Script.Objects.FirstOrDefault(o => scene.IsVisible(o) && o.Click.Length >= 3 &&
                                                                 data.ObjectTexts.Get(o.Name)?.Over is { Length: > 0 });
            if (hover != null)
                (mx, my) = ((int)hover.Click.Average(p => p.X), (int)hover.Click.Average(p => p.Y));
            Render(mx, my);
            Screenshot.SaveBmp(fb, opt.Screenshot);
            Console.WriteLine($"saved {opt.Screenshot} (hover at {mx},{my})");
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
                if (k == Keys.Escape)
                    host.Input.QuitRequested = true;
            ulong now = Sdl.GetTicks();
            scene.Update(now - last);
            tickClock += now - last;
            last = now;
            while (tickClock >= 100)
            {
                tickClock -= 100;
                world.Update();
            }
            Render(host.Input.MouseX, host.Input.MouseY);
            host.Present(fb);
        }
        return 0;
    }
}
