using OpenFive.Engine.Formats;

namespace OpenFive.Engine.World;

/// <summary>
/// Character states. Each one is an animation named "&lt;prefix&gt;&lt;suffix&gt;" in common.res
/// (for example "aDick_walk_left"); <see cref="Suffix"/> lists the suffixes by state number.
/// </summary>
public static class ActorStates
{
    // Walking and standing, in the eight-way order the original uses (no straight up or down).
    public const int WalkDownLeft = 1, WalkLeft = 2, WalkUpLeft = 3, WalkUpRight = 4, WalkRight = 5, WalkDownRight = 6;
    public const int StandDownLeft = 7, StandLeft = 8, StandUpLeft = 9, StandUpRight = 10, StandRight = 11, StandDownRight = 12;

    // Looking through the binoculars: raise them, hold them, lower them.
    public const int BinocularsUpLeft = 13, BinocularsUpRight = 14, BinocularsLeft = 15, BinocularsRight = 16;
    public const int BinocularsDownLeft = 17, BinocularsDownRight = 18;

    // Reaching for something at middle height ("takem") or low down ("takeu").
    public const int TakeMidLeft = 19, TakeMidUpLeft = 20, TakeMidUpRight = 21, TakeMidRight = 22;
    public const int TakeLowLeft = 23, TakeLowUpLeft = 24, TakeLowUpRight = 25, TakeLowRight = 26;

    public const int SpeakDownLeft = 27, SpeakDownRight = 28;

    // Sitting: getting up, seated, sitting down, talking while seated.
    public const int RiseDownLeft = 29, RiseLeft = 30, RiseRight = 31, RiseDownRight = 32;
    public const int SitDownLeft = 33, SitLeft = 34, SitRight = 35, SitDownRight = 36;
    public const int TakeSeatDownLeft = 37, TakeSeatLeft = 38, TakeSeatRight = 39, TakeSeatDownRight = 40;
    public const int SitSpeakDownLeft = 41, SitSpeakLeft = 42, SitSpeakRight = 43, SitSpeakDownRight = 44;

    // Kneeling down to the ground: kneel, stay down, get up, and examine what is there.
    public const int KneelLeft = 45, KneelRight = 46, KneelingLeft = 47, KneelingRight = 48;
    public const int GetUpLeft = 49, GetUpRight = 50;
    public const int CheerLeft = 51, CheerRight = 52;
    public const int ExamineGroundLeft = 53, ExamineGroundRight = 54, ExaminingGroundLeft = 55, ExaminingGroundRight = 56;

    // Idle poses: hands in the pockets and out again (see GameWorld.UpdateIdle).
    public const int HandsInPocketsLeft = 57, HandsInPocketsRight = 58;
    public const int HandsInPocketsStillLeft = 59, HandsInPocketsStillRight = 60;

    public const int JumpDownLeft = 61, JumpDownRight = 62;
    public const int CelebrateDownLeft = 63, CelebrateDownRight = 64, CelebrateJumpDownLeft = 65, CelebrateJumpDownRight = 66;
    public const int KnockLeft = 67, KnockRight = 68, LookAroundLeft = 69, LookAroundRight = 70;
    public const int ShoutLeft = 71, ShoutRight = 72, ShoutingLeft = 73, ShoutingRight = 74;
    public const int LookUpLeft = 75, LookUpRight = 76, LookingUpLeft = 77, LookingUpRight = 78;
    public const int TakeMidLeftLong = 79, TakeLowLeftLong = 80, TakeMidRightLong = 81;
    public const int TakeUmbrella = 82, TakeTin = 83, OpenDown = 84;

    // Timmy.
    public const int PantLeft = 85, PantingLeft = 86, HindLegRight = 87, HindLegStillRight = 88, WagTailRight = 89;
    public const int BarkRight = 90;

    public const int FrightenedDownRight = 91, CryRight = 92, KeyholeLeft = 93, EatRight = 94, TeapotDownLeft = 95;
    public const int PushBoatRight = 96, PushBoatLeft = 97, TeapotRight = 98, TeapotLeft = 99, AxeRight = 100;
    public const int TieRight = 101, TieLeft = 102, ThrowRight = 103, InBucketRight = 104, InBucketStillRight = 105;
    public const int WatchLeft = 106, BurrowRight = 107, Gone = 108, SniffRight = 109, CrawlRight = 110, ThrowLeft = 111;
    public const int PhoneRight = 112, PhoneStillRight = 113, PhoneTalkRight = 114;
    public const int HandsOutOfPocketsLeft = 115, HandsOutOfPocketsRight = 116;

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
    public static readonly IReadOnlyDictionary<int, int> FollowUp = new Dictionary<int, int>
    {
        [BinocularsUpLeft] = BinocularsLeft, [BinocularsUpRight] = BinocularsRight,
        [BinocularsDownLeft] = StandLeft, [BinocularsDownRight] = StandRight,
        [TakeMidLeft] = StandLeft, [TakeMidUpLeft] = StandUpLeft, [TakeMidUpRight] = StandUpRight, [TakeMidRight] = StandRight,
        [TakeLowLeft] = StandLeft, [TakeLowUpLeft] = StandUpLeft, [TakeLowUpRight] = StandUpRight, [TakeLowRight] = StandRight,
        [RiseDownLeft] = StandDownLeft, [RiseLeft] = StandDownLeft, [RiseRight] = StandDownLeft, [RiseDownRight] = StandDownRight,
        [TakeSeatDownLeft] = SitDownLeft, [TakeSeatLeft] = SitLeft, [TakeSeatRight] = SitRight, [TakeSeatDownRight] = SitDownRight,
        [KneelLeft] = ExamineGroundLeft, [KneelRight] = ExamineGroundRight,
        [KneelingLeft] = ExaminingGroundLeft, [KneelingRight] = ExaminingGroundRight,
        [GetUpLeft] = StandLeft, [GetUpRight] = StandRight,
        [CheerLeft] = StandLeft, [CheerRight] = StandRight,
        [ExamineGroundLeft] = GetUpLeft, [ExamineGroundRight] = GetUpRight,
        [HandsInPocketsLeft] = HandsInPocketsStillLeft, [HandsInPocketsRight] = HandsInPocketsStillRight,
        [JumpDownLeft] = StandDownLeft, [JumpDownRight] = StandDownRight,
        [CelebrateDownLeft] = StandDownLeft, [CelebrateDownRight] = StandDownRight,
        [CelebrateJumpDownLeft] = StandDownLeft, [CelebrateJumpDownRight] = StandDownRight,
        [KnockLeft] = StandLeft, [KnockRight] = StandRight, [LookAroundLeft] = StandLeft, [LookAroundRight] = StandRight,
        [ShoutLeft] = ShoutingLeft, [ShoutRight] = ShoutingRight, [LookUpLeft] = LookingUpLeft, [LookUpRight] = LookingUpRight,
        [TakeMidLeftLong] = StandLeft, [TakeLowLeftLong] = StandLeft, [TakeMidRightLong] = StandRight,
        [TakeUmbrella] = StandRight, [TakeTin] = StandLeft, [OpenDown] = StandRight, [PantLeft] = PantingLeft,
        [HindLegRight] = StandRight, [WagTailRight] = StandRight, [BarkRight] = StandRight,
        [FrightenedDownRight] = StandDownRight, [CryRight] = StandDownRight, [KeyholeLeft] = StandLeft, [EatRight] = SitRight,
        [TeapotDownLeft] = StandDownLeft, [PushBoatRight] = StandRight, [PushBoatLeft] = StandLeft,
        [TeapotRight] = StandRight, [TeapotLeft] = StandLeft, [AxeRight] = StandUpRight,
        [TieRight] = StandUpRight, [TieLeft] = StandUpLeft, [ThrowRight] = StandDownRight,
        [InBucketRight] = InBucketStillRight, [WatchLeft] = StandUpLeft, [BurrowRight] = Gone,
        [SniffRight] = BurrowRight, [ThrowLeft] = StandDownLeft, [PhoneRight] = PhoneStillRight,
        [HandsOutOfPocketsLeft] = StandDownLeft, [HandsOutOfPocketsRight] = StandDownRight,
    };

    /// <summary>True for the six walking directions.</summary>
    public static bool IsWalk(int s) => s is >= WalkDownLeft and <= WalkDownRight;
    /// <summary>True for the six standing directions.</summary>
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
    public bool Started { get; internal set; }
    public bool Completed { get; internal set; }
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
