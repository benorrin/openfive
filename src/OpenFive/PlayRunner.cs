using OpenFive.Engine.Game;
using OpenFive.Engine.World;

namespace OpenFive;

/// <summary>
/// Plays the game from a list of commands, one at a time, each once the game is idle (pointer
/// shown, hero standing, nothing being said). Speech is skipped (see "speech on"). For automated play-throughs.
///
///   click OBJECT | actor NAME | at X Y | use ITEM TARGET | choose OPTION | wait TICKS | shot FILE
///   expect LOCATION | step Q E | items | save SLOT | load SLOT | until VAR VALUE | # comment
///
/// A command written "!command" runs as soon as the pointer is shown, without waiting for the
/// scene to settle (for moments such as feeding Timmy while Uncle Quentin drinks his tea).
/// </summary>
sealed class PlayRunner
{
    readonly List<string[]> _cmds = new();
    int _next, _idle, _wait, _stuck;
    public bool Done => _next >= _cmds.Count;
    public int Errors { get; private set; }
    /// <summary>Speech is skipped unless "speech on" asks for lines at their real length
    /// (scenes timed against speech, such as pushing the stone at the castle).</summary>
    bool _skipSpeech = true;

    public PlayRunner(string path)
    {
        foreach (string raw in File.ReadAllLines(path))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;
            _cmds.Add(line.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }

    static (int X, int Y)? ObjectPoint(GameSession s, string name)
    {
        var o = s.World.FindObject(name);
        if (o == null || o.Click.Length == 0)
            return null;
        // A point inside the polygon near its centre.
        int cx = (int)o.Click.Average(p => p.X), cy = (int)o.Click.Average(p => p.Y);
        if (Geometry.Contains(o.Click, cx, cy))
            return (cx, cy);
        for (int r = 1; r < 200; r++)
            foreach (var (dx, dy) in new[] { (r, 0), (-r, 0), (0, r), (0, -r), (r, r), (-r, -r), (r, -r), (-r, r) })
                if (Geometry.Contains(o.Click, cx + dx, cy + dy))
                    return (cx + dx, cy + dy);
        return (cx, cy);
    }

    bool Idle(GameSession s)
    {
        var w = s.World;
        return w.CursorVisible && w.Dialogue.OpenMenu == null && w.Hero.Queue.Count == 0 &&
               !w.Tasks.Active.Any(t => t is not AnimTask and not SoundTask);
    }

    /// <summary>Called every tick; returns a message to print, if any.</summary>
    public string? Tick(GameSession s, Action<string> shot)
    {
        var w = s.World;
        if (_skipSpeech && !w.CursorVisible && w.Tick % 3 == 0)
            w.SkipRequested = true;
        if (Done)
            return null;
        var cmd = _cmds[_next];
        // "!command" runs as soon as the pointer is on, without waiting for the scene to settle.
        bool now = cmd[0].StartsWith('!');
        if (now)
            cmd = new[] { cmd[0][1..] }.Concat(cmd.Skip(1)).ToArray();
        string where = $"[{_next + 1}] {string.Join(' ', cmd)} @ {w.Scene?.Name} Q{w.Quest}E{w.Episode}";
        if (cmd[0] == "choose")
        {
            if (w.Dialogue.OpenMenu is not { } menu)
            {
                if (++_stuck > 600)
                {
                    _stuck = 0;
                    _next++;
                    Errors++;
                    return "TIMEOUT waiting for a menu " + where;
                }
                return null;
            }
            int option = int.Parse(cmd[1]);
            var row = menu.Rows.FirstOrDefault(r => r.Option == option);
            _next++;
            _stuck = 0;
            if (row.Y1 == 0)
            {
                Errors++;
                return $"option {option} not on the menu ({string.Join(",", menu.Rows.Select(r => r.Option))}) " + where;
            }
            menu.Click(w, 400, row.Y0 + 10);
            return where;
        }
        if (cmd[0] == "until")
        {
            // until VAR VALUE: waits for a story variable.
            if (s.Var(cmd[1]) == int.Parse(cmd[2]))
            {
                _next++;
                _stuck = 0;
                return where;
            }
            if (++_stuck > 3000)
            {
                _stuck = 0;
                _next++;
                Errors++;
                return "TIMEOUT " + where;
            }
            return null;
        }
        if (cmd[0] == "load")
        {
            _next++;
            if (SaveGame.Load(int.Parse(cmd[1])) is not { } g)
            {
                Errors++;
                return "no save " + where;
            }
            s.Restore(g);
            return where;
        }
        if (cmd[0] == "expect" && w.Dialogue.OpenMenu != null)
        {
            _next++;
            if (string.Equals(w.Scene?.Name, cmd[1], StringComparison.OrdinalIgnoreCase))
                return null;
            Errors++;
            return "EXPECTED " + cmd[1] + " " + where;
        }
        if (_wait > 0)
        {
            _wait--;
            return null;
        }
        if (now ? !w.CursorVisible : !Idle(s))
        {
            _idle = 0;
            if (++_stuck > 1500)
            {
                _stuck = 0;
                Errors++;
                var d = w.Tasks.Active.Select(t => t.GetType().Name + ":" + (t is DialogueTask ? t.ToString() : t.Name) + (t.Started ? "" : $"(waits {(t.WaitFor as GameTask)?.Name ?? t.WaitFor?.GetType().Name}, delay {t.Delay})")).Take(8);
                _next++;
                return $"TIMEOUT (not idle: pointer {w.CursorVisible}, hero queue {w.Hero.Queue.Count}, tasks {string.Join(" ", d)}) " + where;
            }
            return null;
        }
        if (!now && ++_idle < 5)
            return null;
        _idle = 0;
        _stuck = 0;
        _next++;
        switch (cmd[0])
        {
            case "click":
            {
                var p = ObjectPoint(s, cmd[1]);
                if (p == null)
                {
                    Errors++;
                    return "no object " + where;
                }
                if (!w.IsObjectOn(cmd[1]))
                {
                    Errors++;
                    return "object is off " + where;
                }
                s.MoveMouse(p.Value.X, p.Value.Y);
                var t = s.FindTarget(p.Value.X, p.Value.Y);
                s.Click();
                return where + (t?.Name != cmd[1] ? $" (hit {t?.Name})" : "");
            }
            case "actor":
            {
                var a = w.Actors.First(x => x.Name.Equals(cmd[1], StringComparison.OrdinalIgnoreCase));
                if (!s.World.Scene!.Name.Equals(a.Location, StringComparison.OrdinalIgnoreCase) || !a.Visible)
                {
                    Errors++;
                    return "character not here " + where;
                }
                // Clicking a character is handled like clicking its object (names match the characters).
                if (a == w.Hero)
                    s.OpenInventory();
                else
                    s.Script.Click(a.Name);
                return where;
            }
            case "speech":
                _skipSpeech = cmd[1] != "on";
                return where;
            case "lines":
                // Lists every entry of a dialogue scene (all speakers and story steps).
                foreach (var d in w.Data.Dialogs.All
                             .Where(d => d.Scene.Equals(cmd[1], StringComparison.OrdinalIgnoreCase))
                             .OrderBy(d => d.Id))
                    Console.WriteLine($"  {d.Speaker} {(d.IsChoice ? "DLG" : "V")} Q{d.Quest} E{d.Episode} {d.Id}: " +
                                      string.Join(" | ", d.Options.Select(o => o.Text)));
                return where;
            case "at":
                s.MoveMouse(int.Parse(cmd[1]), int.Parse(cmd[2]));
                s.Click();
                return where;
            case "use":
            {
                int item = int.Parse(cmd[1]);
                if (!s.Inventory.Has(item))
                {
                    Errors++;
                    return $"item {item} not in the rucksack [{string.Join(",", s.Inventory.Items)}] " + where;
                }
                bool here = w.IsObjectOn(cmd[2]) || Inventory.ItemOf(cmd[2]) != 0 ||
                            w.Actors.Any(a => a.Visible && a.Name.Equals(cmd[2], StringComparison.OrdinalIgnoreCase) &&
                                              a.Location.Equals(w.Scene?.Name, StringComparison.OrdinalIgnoreCase));
                if (!here)
                {
                    Errors++;
                    return $"target {cmd[2]} not here " + where;
                }
                s.UseItemOn(item, cmd[2]);
                return where;
            }
            case "wait":
                _wait = int.Parse(cmd[1]);
                return null;
            case "shot":
                shot(cmd[1]);
                return where;
            case "expect":
                if (!string.Equals(w.Scene?.Name, cmd[1], StringComparison.OrdinalIgnoreCase))
                {
                    Errors++;
                    return "EXPECTED " + cmd[1] + " " + where;
                }
                return null;
            case "step":
                w.Quest = int.Parse(cmd[1]);
                w.Episode = int.Parse(cmd[2]);
                return where;
            case "save":
                if (!s.Capture("play").Write(int.Parse(cmd[1])))
                    Errors++;
                return where;
            case "items":
                return $"items [{string.Join(", ", s.Inventory.Items.Select(i => $"{i} {w.Data.ObjectTexts.Get(Inventory.ObjectName(i))?.Over}"))}] " + where;
        }
        Errors++;
        return "unknown command " + where;
    }
}
