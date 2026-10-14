using OpenFive.Engine;
using OpenFive.Engine.Audio;
using OpenFive.Engine.Game;
using OpenFive.Engine.Graphics;
using OpenFive.Engine.World;
using OpenFive.Platform;

namespace OpenFive;

/// <summary>
/// Developer scene viewer: a location with characters, pointer, labels and the rucksack. It can
/// run on screen, or off screen from scripted input (saving screenshots).
/// </summary>
sealed class Viewer
{
    readonly Options _opt;
    readonly Framebuffer _fb = new();
    readonly AudioMixer _mixer = new();
    readonly GameAudio _audio;
    readonly GameWorld _world;
    readonly GameSession _session;

    Viewer(GameData data, Options opt)
    {
        _opt = opt;
        _audio = new GameAudio(data, _mixer);
        _world = new GameWorld(data, _audio);
        _session = new GameSession(_world, Scripts.For);
    }

    /// <summary>Shows the location set up from the options; returns the exit code.</summary>
    public static int Run(GameData data, Options opt)
    {
        var viewer = new Viewer(data, opt);
        viewer.SetUp();
        return opt.Screenshot != null || opt.Script != null ? viewer.RunOffScreen() : viewer.RunOnScreen();
    }

    /// <summary>Loads the location and applies the options: characters, items, walks, lines, animations.</summary>
    void SetUp()
    {
        _world.Quest = _opt.Quest;
        _world.Episode = _opt.Episode;
        _session.EnterLocation(_opt.Viewer!);
        if (_opt.Track != null)
            _audio.SetBackgroundTrack(_opt.Track);
        PlaceCharacters();
        foreach (int item in _opt.Items)
            _session.GiveItem(item);
        foreach (var (x, y) in _opt.WalkTargets)
            _world.WalkToAfter(_world.Hero, x, y);
        for (int i = 0; i < _opt.Say.Count; i++)
            _world.Dialogue.Say(_opt.Say[i].Speaker, _opt.Say[i].Scene, _opt.Say[i].Line, last: i == _opt.Say.Count - 1);
        foreach (string a in _opt.Anims)
            if (_world.PlayAnim(a, -1) == null)
                Console.Error.WriteLine($"unknown animation {a}");
    }

    /// <summary>Places the characters of --place; the first is the hero, each further one follows the one before.</summary>
    void PlaceCharacters()
    {
        Actor? previous = null;
        foreach (var (name, x, y) in _opt.Place)
        {
            var actor = _world.Actors.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (actor == null)
            {
                Console.Error.WriteLine($"unknown character {name}");
                continue;
            }
            _world.PlaceActor(actor, x, y, ActorStates.StandDownRight);
            if (previous == null)
                _world.Hero = actor;
            else
                previous.Follower = actor;
            previous = actor;
        }
    }

    void Tick()
    {
        _world.Scene!.Update(100);
        _session.Tick();
        _world.Update();
    }

    void Render()
    {
        _world.Draw(_fb);
        _session.DrawOverlay(_fb);
    }

    /// <summary>Runs --frames ticks with the scripted input, saves the screenshots, and prints the characters' state.</summary>
    int RunOffScreen()
    {
        var script = _opt.Script != null ? InputScript.Load(_opt.Script) : new InputScript();
        int frames = Math.Max(1, _opt.Frames);
        for (int t = 0; t < frames; t++)
        {
            foreach (var cmd in script.At(t))
                Apply(cmd, t);
            Tick();
        }
        if (_opt.Screenshot != null)
        {
            Render();
            Screenshot.SaveBmp(_fb, _opt.Screenshot);
            Console.WriteLine($"saved {_opt.Screenshot}");
        }
        foreach (var a in _world.Actors.Where(a => a.Visible))
            Console.WriteLine($"{a.Name}: ({a.X},{a.Y}) state {a.State} shape {a.Shape} scale {a.Scale:F3} depth {a.Depth} " +
                              $"queue {a.Queue.Count} sprite ({a.Sprite.X},{a.Sprite.Y}) x{a.Sprite.Scale:F3} " +
                              $"frame {a.Sprite.Frame} visible {a.Sprite.Visible} packed {a.Sprite.Packed}");
        return 0;
    }

    /// <summary>Carries out one scripted input command at tick <paramref name="tick"/>.</summary>
    void Apply(InputScript.Command cmd, int tick)
    {
        switch (cmd.Op)
        {
            case "move":
                _session.MoveMouse(cmd.X, cmd.Y);
                break;
            case "click":
                _session.MoveMouse(cmd.X, cmd.Y);
                _session.Click();
                break;
            case "shot":
                Render();
                Screenshot.SaveBmp(_fb, cmd.Arg);
                Console.WriteLine($"tick {tick}: saved {cmd.Arg}");
                break;
        }
    }

    /// <summary>Shows the location in a window until Escape (or --frames frames).</summary>
    int RunOnScreen()
    {
        using var host = new Host(_opt.Fullscreen, !_opt.Sharp);
        using var output = new AudioOutput(_mixer);
        ulong last = Sdl.GetTicks(), tickClock = 0;
        int frame = 0;
        while (!host.Input.QuitRequested && (_opt.Frames < 0 || frame++ < _opt.Frames))
        {
            host.PumpEvents();
            while (host.Input.KeysPressed.TryDequeue(out int k))
            {
                if (k == Keys.Escape)
                    host.Input.QuitRequested = true;
                else if (k == Keys.Space && !_world.CursorVisible)
                    _world.SkipRequested = true;
            }
            _session.MoveMouse(host.Input.MouseX, host.Input.MouseY);
            if (host.Input.Clicked)
                _session.Click();
            ulong now = Sdl.GetTicks();
            tickClock += now - last;
            last = now;
            for (; tickClock >= 100; tickClock -= 100)
                Tick();
            Render();
            host.Present(_fb, host.Input.MouseInside);
        }
        return 0;
    }
}
