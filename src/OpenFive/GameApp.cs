using OpenFive.Engine;
using OpenFive.Engine.Audio;
using OpenFive.Engine.Game;
using OpenFive.Engine.Graphics;
using OpenFive.Engine.World;
using OpenFive.Platform;

namespace OpenFive;

/// <summary>The game: intro movie, menus and play, on a window (or off-screen for tests).</summary>
sealed class GameApp
{
    readonly GameData _data;
    readonly Options _opt;
    readonly Framebuffer _fb = new();
    readonly AudioMixer _mixer = new();
    readonly GameAudio _audio;
    readonly GameWorld _world;
    readonly GameSession _session;
    readonly Settings _settings;
    MoviePlayer? _movie;
    readonly float[] _silence = new float[Engine.Formats.Wav.OutputRate / 10 * 2];
    int _movieMusic;

    public GameApp(GameData data, Options opt, Settings settings)
    {
        _data = data;
        _opt = opt;
        _settings = settings;
        _audio = new GameAudio(data, _mixer);
        _audio.SetVolumes(settings.MusicVolume, 100, settings.SpeechVolume);
        _world = new GameWorld(data, _audio);
        _session = new GameSession(_world, Scripts.For, settings);
        _session.MovieRequested += (path, music) => StartMovie(path, music);
    }

    void StartMovie(string path, string music)
    {
        try
        {
            _movie = new MoviePlayer(_data, path);
            _audio.StopBackgroundTrack();
            _movieMusic = music.Length > 0 ? _audio.PlayEffect(music) : 0;
        }
        catch (Exception e) when (e is IOException or InvalidDataException)
        {
            Console.Error.WriteLine("movie: " + e.Message);
            _movie = null;
        }
    }

    void EndMovie()
    {
        _movie = null;
        if (_movieMusic != 0)
            _audio.Stop(_movieMusic);
        _movieMusic = 0;
    }

    void Start()
    {
        if (!_opt.SkipIntro)
        {
            bool de = _data.Language == Language.German;
            StartMovie(de ? "smk/introd.smk" : "smk/introe.smk", de ? "wav/atmo/titelsong.wav" : "wav/atmo/track2.wav");
        }
        if (_opt.StartLocation != null)
        {
            _session.Reset();
            _session.InGame = true;
            _world.Quest = _opt.Quest;
            _world.Episode = _opt.Episode;
            foreach (int i in _opt.Items)
                _session.GiveItem(i);
            _session.EnterLocation(_opt.StartLocation, _opt.From ?? "");
        }
        else
            _session.EnterLocation("menue");
    }

    void Tick()
    {
        if (_movie != null)
            return;
        _world.DialogueBox = _settings.DialogueBox;
        if (_settings.AnimatedBackgrounds)
            _world.Scene?.Update(100);
        _session.Tick();
        _world.Update();
    }

    void Render()
    {
        if (_movie != null)
        {
            _movie.Draw(_fb);
            return;
        }
        _world.Draw(_fb);
        _session.DrawOverlay(_fb);
    }

    void Click(int x, int y)
    {
        if (_movie != null)
        {
            _movie.Skip();
            return;
        }
        _session.MoveMouse(x, y);
        _session.Click();
    }

    void Key(int scancode)
    {
        if (_movie != null)
        {
            if (scancode is Keys.Space or Keys.Escape or Keys.Return)
                _movie.Skip();
            return;
        }
        char c = ScancodeToChar(scancode);
        if (c != 0 && _session.KeyTyped(c))
            return;
        if (scancode == Keys.F1 && _session.InGame && !GameSession.IsMenu(_world.Scene?.Name ?? ""))
            _session.ChangeLocation("menue");
        else if (scancode == Keys.Space && !_world.CursorVisible)
            _world.SkipRequested = true;
        else if (scancode == Keys.Escape && GameSession.IsMenu(_world.Scene?.Name ?? "") && _session.InGame)
            _session.ChangeLocation(_session.GameLocation);
    }

    static char ScancodeToChar(int sc) => sc switch
    {
        >= 4 and <= 29 => (char)('A' + sc - 4),
        >= 30 and <= 38 => (char)('1' + sc - 30),
        39 => '0',
        Keys.Space => ' ',
        Keys.Backspace => '\b',
        Keys.Return => '\r',
        Keys.Escape => '\u001b',
        _ => '\0',
    };

    /// <summary>Off-screen run driven by an input script (for automated tests).</summary>
    int RunScripted()
    {
        var script = _opt.Script != null ? InputScript.Load(_opt.Script) : new InputScript();
        Start();
        int frames = Math.Max(1, _opt.Frames);
        for (int t = 0; t < frames && !_session.QuitRequested; t++)
        {
            foreach (var cmd in script.At(t))
            {
                switch (cmd.Op)
                {
                    case "move":
                        _session.MoveMouse(cmd.X, cmd.Y);
                        break;
                    case "click":
                        Click(cmd.X, cmd.Y);
                        break;
                    case "key":
                        Key(cmd.X);
                        break;
                    case "shot":
                        Render();
                        Screenshot.SaveBmp(_fb, cmd.Arg);
                        Console.WriteLine($"tick {t}: {_world.Scene?.Name} Q{_world.Quest} E{_world.Episode} -> {cmd.Arg}");
                        break;
                }
            }
            _movie?.Update(100);
            if (_movie is { Finished: true })
                EndMovie();
            // No audio device off-screen: consume a tick's worth of sound so recordings end.
            _mixer.Mix(_silence);
            Tick();
        }
        if (_opt.Screenshot != null)
        {
            Render();
            Screenshot.SaveBmp(_fb, _opt.Screenshot);
        }
        Console.WriteLine($"end: {_world.Scene?.Name} Q{_world.Quest} E{_world.Episode} items [{string.Join(",", _session.Inventory.Items)}]");
        return 0;
    }

    public int Run()
    {
        if (_opt.Script != null || _opt.Screenshot != null)
            return RunScripted();
        using var host = new Host(_opt.Fullscreen || _settings.Fullscreen, _settings.SmoothScaling && !_opt.Sharp);
        using var output = new AudioOutput(_mixer);
        Start();
        ulong last = Sdl.GetTicks(), clock = 0;
        bool wasDown = false;
        while (!host.Input.QuitRequested && !_session.QuitRequested)
        {
            host.PumpEvents();
            host.SetSmooth(_settings.SmoothScaling);
            while (host.Input.KeysPressed.TryDequeue(out int k))
                Key(k);
            _session.MoveMouse(host.Input.MouseX, host.Input.MouseY);
            if (host.Input.Clicked)
                Click(host.Input.MouseX, host.Input.MouseY);
            if (wasDown && !host.Input.ButtonDown)
                _session.ButtonReleased();
            wasDown = host.Input.ButtonDown;
            ulong now = Sdl.GetTicks();
            ulong dt = now - last;
            last = now;
            if (_movie != null)
            {
                _movie.Update(dt);
                if (_movie.Finished)
                    EndMovie();
            }
            clock += dt;
            if (clock > 1000)
                clock = 1000;
            while (clock >= 100)
            {
                clock -= 100;
                Tick();
            }
            Render();
            host.Present(_fb, host.Input.MouseInside && _movie == null);
            if (_world.CursorWarp is { } w)
            {
                _world.CursorWarp = null;
                host.WarpMouse(w.X, w.Y);
            }
        }
        return 0;
    }
}
