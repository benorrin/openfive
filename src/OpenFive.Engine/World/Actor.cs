using OpenFive.Engine.Formats;

namespace OpenFive.Engine.World;

/// <summary>Character states; each maps to an animation named "&lt;prefix&gt;&lt;suffix&gt;" in common.res.</summary>
public static class ActorStates
{
    public const int WalkDownLeft = 1, WalkLeft = 2, WalkUpLeft = 3, WalkUpRight = 4, WalkRight = 5, WalkDownRight = 6;
    public const int StandDownLeft = 7, StandLeft = 8, StandUpLeft = 9, StandUpRight = 10, StandRight = 11, StandDownRight = 12;

    /// <summary>Animation name suffixes by state number (identifiers used by the game's animation data).</summary>
    public static readonly string[] Suffix =
    {
        "", "walk_downleft", "walk_left", "walk_upleft", "walk_upright", "walk_right", "walk_downright",
        "stand_downleft", "stand_left", "stand_upleft", "stand_upright", "stand_right", "stand_downright",
        "bino_left_up", "bino_right_up", "bino_stand_left", "bino_stand_right", "bino_left_down", "bino_right_down",
        "takem_left", "takem_upleft", "takem_upright", "takem_right", "takeu_left", "takeu_upleft", "takeu_upright", "takeu_right",
        "stand_speak_downleft", "stand_speak_downright", "stand_sitdown_downleft", "stand_sitdown_left", "stand_sitdown_right",
        "stand_sitdown_downright", "sit_downleft", "sit_left", "sit_right", "sit_downright", "stand_sitdown_downleft",
        "stand_sitdown_left", "stand_sitdown_right", "stand_sitdown_downright", "sitspeak_downleft", "sitspeak_left",
        "sitspeak_right", "sitspeak_downright", "taked_left", "taked_right", "takedstill_left", "takedstill_right",
        "taked_goup_left", "taked_goup_right", "cheer_left", "cheer_right", "examinedown_left", "examinedown_right",
        "examinedownstill_left", "examinedownstill_right", "handspocket_left", "handspocket_right", "handspocketstill_left",
        "handspocketstill_right", "jump_downleft", "jump_downright", "jubel_downleft", "jubel_downright", "jubeljump_downleft",
        "jubeljump_downright", "knocking_left", "knocking_right", "lookaround_left", "lookaround_right", "shout_left",
        "shout_right", "shoutstill_left", "shoutstill_right", "lookup_left", "lookup_right", "lookupstill_left",
        "lookupstill_right", "takem_left_long", "takeu_left_long", "takem_right_long", "take_umbr", "take_tin", "open_down",
        "haechle_left", "haechle_stand_left", "hint_leg_right", "hintstill_leg_right", "tail_right", "barknormal_right",
        "frightend_downright", "cry_right", "key_hole_left", "eat_right", "teapot_downleft", "push_boot_right",
        "push_boot_left", "teapot_right", "teapot_left", "with_Axt_right", "tie_right", "tie_left", "throw_right",
        "inbucket_right", "inbucket_right_still", "watch_left", "burrow_right", "gone", "sniff_right", "crawl_right",
        "throw_left", "telefon_right", "telefon_right_still", "telefon_right_talk", "handsoutpocket_left",
        "handsoutpocket_right",
    };

    /// <summary>
    /// The pose a character changes to when an action animation has played to its end (for
    /// example kneel - examine - get up - stand). States not listed loop (talking, sitting...).
    /// </summary>
    public static readonly Dictionary<int, int> FollowUp = BuildFollowUp();

    static Dictionary<int, int> BuildFollowUp()
    {
        var f = new Dictionary<int, int>
        {
            [13] = 15, [14] = 16, [17] = 8, [18] = 11,
            [45] = 53, [46] = 54, [47] = 55, [48] = 56, [49] = 8, [50] = 11, [53] = 49, [54] = 50,
            [29] = 7, [30] = 7, [31] = 7, [32] = 12,
            [51] = 8, [52] = 11, [57] = 59, [58] = 60,
            [61] = 7, [62] = 12, [63] = 7, [64] = 12, [65] = 7, [66] = 12,
            [67] = 8, [68] = 11, [69] = 8, [70] = 11, [71] = 73, [72] = 74, [75] = 77, [76] = 78,
            [79] = 8, [80] = 8, [81] = 11, [82] = 11, [83] = 8, [84] = 11, [85] = 86, [87] = 11,
            [89] = 11, [90] = 11, [91] = 12, [92] = 12, [93] = 8, [94] = 35, [95] = 7, [96] = 11,
            [97] = 8, [98] = 11, [99] = 8, [100] = 10, [101] = 10, [102] = 9, [103] = 12, [104] = 105,
            [106] = 9, [107] = 108, [109] = 107, [111] = 7, [112] = 113, [115] = 7, [116] = 12,
        };
        for (int s = 19; s <= 22; s++) f[s] = s - 11; // take (middle height) -> stand
        for (int s = 23; s <= 26; s++) f[s] = s - 15; // take (low) -> stand
        for (int s = 37; s <= 40; s++) f[s] = s - 4;  // sitting down -> sit
        return f;
    }

    public static bool IsWalk(int s) => s is >= WalkDownLeft and <= WalkDownRight;
    public static bool IsStand(int s) => s is >= StandDownLeft and <= StandDownRight;

    /// <summary>The standing state that matches a walking direction.</summary>
    public static int StandFor(int walk) => IsWalk(walk) ? walk + 6 : walk;

    /// <summary>
    /// The walking direction for a move, from its slope (shallower than 1:4 counts as sideways).
    /// </summary>
    public static int Direction(int x0, int y0, int x1, int y1, int current)
    {
        if (x0 == x1 && y0 == y1)
            return current;
        int dx = x1 - x0, dy = y1 - y0;
        float slope = (float)dy / dx;
        if (slope < 0)
            return dx < 0 ? (-slope <= 0.25f ? WalkLeft : WalkDownLeft) : (-slope <= 0.25f ? WalkRight : WalkUpRight);
        return dx < 0 ? (slope <= 0.25f ? WalkLeft : WalkUpLeft) : (slope <= 0.25f ? WalkRight : WalkDownRight);
    }
}

/// <summary>An entry in a character's action queue. Tasks may wait for it to be reached.</summary>
public abstract class ActorStep : TaskHandle
{
    public TaskHandle? WaitFor;
    public int Delay;
    internal bool Started, Completed;
    public override bool Released => Started || Completed;
}

/// <summary>Walk in a straight line to (X, Y), arriving on <see cref="Shape"/>.</summary>
public sealed class WalkStep : ActorStep
{
    public int X, Y, Shape;
}

/// <summary>Change the character's state (stand, face, talk, take...).</summary>
public sealed class StateStep : ActorStep
{
    public int State;
}

/// <summary>Walk to a point, working out the route only when the step is reached.</summary>
public sealed class RouteStep : ActorStep
{
    public int X, Y;
}

/// <summary>Run an action (e.g. the click on the object the character walked to).</summary>
public sealed class ActionStep : ActorStep
{
    public required Action<GameWorld> Action;
}

/// <summary>A character: one of the Five, or another person the scripts move around.</summary>
public sealed class Actor
{
    public string Name { get; }
    /// <summary>Prefix of the character's state animations ("aDick_").</summary>
    public string AnimPrefix { get; }
    public double X, Y;
    public int State = ActorStates.StandDownRight;
    public int Shape = -1;
    public double Scale = 0.5;
    public int Depth = 1000;
    public bool Visible;
    /// <summary>Location the character is in.</summary>
    public string Location = "";
    public int SpeedX = 10, SpeedY = 4;
    public readonly LinkedList<ActorStep> Queue = new();
    public readonly Sprite Sprite = new();
    /// <summary>The character that follows this one around (the rest of the party), or null.</summary>
    public Actor? Follower;

    // Display state
    internal string _animName = "";
    internal int _frame;

    public Actor(string name, string animPrefix)
    {
        Name = name;
        AnimPrefix = animPrefix;
    }

    public int IX => (int)Math.Round(X);
    public int IY => (int)Math.Round(Y);

    public ActorStep? Current => Queue.First?.Value;

    public T Enqueue<T>(T step) where T : ActorStep
    {
        Queue.AddLast(step);
        return step;
    }

    /// <summary>Drops all queued steps (a new order replaces the old one).</summary>
    public void ClearQueue()
    {
        foreach (var s in Queue)
            s.Completed = true;
        Queue.Clear();
    }

    /// <summary>
    /// Moves one tick towards (tx, ty): up to SpeedX/SpeedY pixels (scaled by size) per axis, the
    /// shorter axis slowed in proportion. Counting is in whole speed units; when the longer axis is
    /// under one unit away the rest of the distance is covered at once (as the original does).
    /// </summary>
    internal void StepTowards(int tx, int ty)
    {
        double sx = SpeedX * Scale, sy = SpeedY * Scale;
        int dx = tx - IX, dy = ty - IY;
        int ux = Math.Abs(dx) / SpeedX, uy = Math.Abs(dy) / SpeedY;
        if (uy < ux)
        {
            if (dx == 0) dx = 1;
            sy = uy * sy / ux;
        }
        else
        {
            if (dy == 0) dy = 1;
            sx = uy == 0 ? double.PositiveInfinity : ux * sx / uy;
        }
        int nx = dx < 1 ? (int)Math.Round(IX - Math.Min(sx, Math.Abs(dx))) : (int)Math.Round(IX + Math.Min(sx, dx));
        int ny = dy < 1 ? (int)Math.Round(IY - Math.Min(sy, Math.Abs(dy))) : (int)Math.Round(IY + Math.Min(sy, dy));
        // dx/dy were forced to 1 to avoid a division by zero; do not move past the target on that axis.
        if (tx == IX) nx = IX;
        if (ty == IY) ny = IY;
        X = nx;
        Y = ny;
    }
}
