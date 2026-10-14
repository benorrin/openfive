namespace OpenFive;

/// <summary>Scripted input for the viewer: lines of "TICK move X Y | click X Y | shot FILE".</summary>
sealed class InputScript
{
    /// <summary>One command, carried out at <paramref name="Tick"/>.</summary>
    public readonly record struct Command(int Tick, string Op, int X, int Y, string Arg);

    readonly List<Command> _cmds = new();

    /// <summary>Reads a script; lines starting with '#' are comments.</summary>
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

    /// <summary>The commands of one tick, in file order.</summary>
    public IEnumerable<Command> At(int tick) => _cmds.Where(c => c.Tick == tick);
}
