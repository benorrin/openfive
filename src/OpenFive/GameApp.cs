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
    /// <summary>One tick's worth of sound, mixed and thrown away when there is no audio device.</summary>
    readonly float[] _silence = new float[Engine.Formats.Wav.OutputRate / 10 * 2];

    MoviePlayer? _movie;
    int _movieMusic;

    /// <summary>Sets up the world and session with the player's settings.</summary>
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

    /// <summary>Plays a movie; its music is a sound of its own, or (the intro's) the background
    /// track, which goes on playing afterwards.</summary>
    void StartMovie(string path, string music, bool musicAsTrack = false)
    {
        try
        {
            _movie = new MoviePlayer(_data, path);
            _audio.StopBackgroundTrack();
            _movieMusic = 0;
            if (musicAsTrack)
                _audio.SetBackgroundTrack(music);
            else if (music.Length > 0)
                _movieMusic = _audio.PlayEffect(music);
        }
        catch (Exception e) when (e is IOException or InvalidDataException)
        {
            Console.Error.WriteLine("movie: " + e.Message);
            _movie = null;
        }
    }

    /// <summary>Stops the movie and its own music.</summary>
    void EndMovie()
    {
        _movie = null;
        if (_movieMusic != 0)
            _audio.Stop(_movieMusic);
        _movieMusic = 0;
    }

    /// <summary>
    /// Starts the intro (unless skipped), then either the logo/menu or, with --loc, a game in that
    /// location at the given story step.
    /// </summary>
    void Start()
    {
        if (!_opt.SkipIntro)
        {
            bool de = _data.Language == Language.German;
            StartMovie(de ? "smk/introd.smk" : "smk/introe.smk", de ? "wav/atmo/titelsong.wav" : "wav/atmo/track2.wav",
                musicAsTrack: true);
        }
        if (_opt.StartLocation != null)
        {
            _session.Reset();
            StoryScript.NewGameMenus(_session);
            _session.InGame = true;
            _world.Quest = _opt.Quest;
            _world.Episode = _opt.Episode;
            foreach (int i in _opt.Items)
                _session.GiveItem(i);
            _session.EnterLocation(_opt.StartLocation, _opt.From ?? "");
        }
        else
            _session.EnterLocation(_opt.SkipIntro ? "menue" : "logo");
    }

    /// <summary>One game tick (nothing happens while a movie plays).</summary>
    void Tick()
    {
        if (_movie != null)
            return;
        _world.DialogueBox = _settings.DialogueBox;
        _world.Shadows = _settings.Shadows;
        if (_settings.AnimatedBackgrounds)
            _world.Scene?.Update(100);
        _session.Tick();
        _world.Update();
    }

    /// <summary>Draws the movie, or the game and the pointer.</summary>
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

    /// <summary>A click skips a movie, or goes to the game.</summary>
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

    /// <summary>
    /// A key: skips a movie, types into a save name, or F1 (menu), Space (skip the line) and
    /// Escape (back to the game from a menu).
    /// </summary>
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

    /// <summary>The character a key types (letters, digits, space and editing keys), or '\0'.</summary>
    static char ScancodeToChar(int sc) => sc switch
    {
        >= Keys.A and <= Keys.Z => (char)('A' + sc - Keys.A),
        >= Keys.Digit1 and <= Keys.Digit9 => (char)('1' + sc - Keys.Digit1),
        Keys.Digit0 => '0',
        Keys.Space => ' ',
        Keys.Backspace => '\b',
        Keys.Return => '\r',
        Keys.Escape => '\u001b',
        _ => '\0',
    };

    /// <summary>Off-screen play-through from a command list (see PlayRunner).</summary>
    int RunPlay()
    {
        var runner = new PlayRunner(_opt.Play!);
        Start();
        if (_movie != null)
            EndMovie();
        if (_opt.StartLocation == null)
            _session.NewGame();
        int maxTicks = _opt.Frames > 0 ? _opt.Frames : 200000;
        for (int t = 0; t < maxTicks && !runner.Done && !_session.QuitRequested; t++)
        {
            string? msg = runner.Tick(_session, SaveScreenshot);
            if (msg != null)
                Console.WriteLine($"{t,6} {msg}");
            _mixer.Mix(_silence);
            Tick();
        }
        Console.WriteLine($"end: {Summary()} errors {runner.Errors}{(runner.Done ? "" : " (not finished)")}");
        return runner.Errors == 0 && runner.Done ? 0 : 1;
    }

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
                        SaveScreenshot(cmd.Arg);
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
            SaveScreenshot(_opt.Screenshot);
        Console.WriteLine($"end: {Summary()}");
        return 0;
    }

    void SaveScreenshot(string file)
    {
        Render();
        Screenshot.SaveBmp(_fb, file);
    }

    /// <summary>Where the game is: location, story step and rucksack.</summary>
    string Summary() =>
        $"{_world.Scene?.Name} Q{_world.Quest} E{_world.Episode} items [{string.Join(",", _session.Inventory.Items)}]";

    /// <summary>Runs the game (or an off-screen run, for --play, --script and --screenshot); returns the exit code.</summary>
    public int Run()
    {
        if (_opt.Play != null)
            return RunPlay();
        if (_opt.Script != null || _opt.Screenshot != null)
            return RunScripted();
        return RunWindowed();
    }

    /// <summary>
    /// The game in a window: input, movies, ten game ticks per second (at most a second's worth
    /// caught up after a stall), and a frame drawn every time round.
    /// </summary>
    int RunWindowed()
    {
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
            for (clock = Math.Min(clock + dt, 1000); clock >= 100; clock -= 100)
                Tick();
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
