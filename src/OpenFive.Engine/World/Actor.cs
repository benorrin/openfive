using OpenFive.Engine.Formats;

namespace OpenFive.Engine.World;

/// <summary>
/// Character states. Each one is an animation named "&lt;prefix&gt;&lt;suffix&gt;" in common.res
/// (for example "aDick_walk_left"); <see cref="Suffix"/> lists the suffixes by state number.
/// </summary>
public static class ActorStates
{
    /// <summary>Walking, in the six directions the original uses (no straight up or down).</summary>
    public const int WalkDownLeft = 1, WalkLeft = 2, WalkUpLeft = 3, WalkUpRight = 4, WalkRight = 5, WalkDownRight = 6;

    /// <summary>Standing, facing one of the six directions.</summary>
    public const int StandDownLeft = 7, StandLeft = 8, StandUpLeft = 9, StandUpRight = 10, StandRight = 11, StandDownRight = 12;

    /// <summary>Raising the binoculars, and holding them to the eyes.</summary>
    public const int BinocularsUpLeft = 13, BinocularsUpRight = 14, BinocularsLeft = 15, BinocularsRight = 16;

    /// <summary>Lowering the binoculars.</summary>
    public const int BinocularsDownLeft = 17, BinocularsDownRight = 18;

    /// <summary>Reaching for something at middle height ("takem").</summary>
    public const int TakeMidLeft = 19, TakeMidUpLeft = 20, TakeMidUpRight = 21, TakeMidRight = 22;

    /// <summary>Reaching for something low down ("takeu").</summary>
    public const int TakeLowLeft = 23, TakeLowUpLeft = 24, TakeLowUpRight = 25, TakeLowRight = 26;

    /// <summary>Talking while standing.</summary>
    public const int SpeakDownLeft = 27, SpeakDownRight = 28;

    /// <summary>Getting up from a seat.</summary>
    public const int RiseDownLeft = 29, RiseLeft = 30, RiseRight = 31, RiseDownRight = 32;

    /// <summary>Sitting.</summary>
    public const int SitDownLeft = 33, SitLeft = 34, SitRight = 35, SitDownRight = 36;

    /// <summary>Sitting down.</summary>
    public const int TakeSeatDownLeft = 37, TakeSeatLeft = 38, TakeSeatRight = 39, TakeSeatDownRight = 40;

    /// <summary>Talking while seated.</summary>
    public const int SitSpeakDownLeft = 41, SitSpeakLeft = 42, SitSpeakRight = 43, SitSpeakDownRight = 44;

    /// <summary>Kneeling down to the ground, and staying down.</summary>
    public const int KneelLeft = 45, KneelRight = 46, KneelingLeft = 47, KneelingRight = 48;

    /// <summary>Getting up from kneeling.</summary>
    public const int GetUpLeft = 49, GetUpRight = 50;

    /// <summary>Cheering.</summary>
    public const int CheerLeft = 51, CheerRight = 52;

    /// <summary>Examining the ground while kneeling.</summary>
    public const int ExamineGroundLeft = 53, ExamineGroundRight = 54, ExaminingGroundLeft = 55, ExaminingGroundRight = 56;

    /// <summary>Idle: putting the hands in the pockets (see <see cref="GameWorld.UpdateIdle"/>).</summary>
    public const int HandsInPocketsLeft = 57, HandsInPocketsRight = 58;

    /// <summary>Idle: standing with the hands in the pockets.</summary>
    public const int HandsInPocketsStillLeft = 59, HandsInPocketsStillRight = 60;

    /// <summary>Jumping.</summary>
    public const int JumpDownLeft = 61, JumpDownRight = 62;

    /// <summary>Celebrating ("jubel"), with or without a jump.</summary>
    public const int CelebrateDownLeft = 63, CelebrateDownRight = 64, CelebrateJumpDownLeft = 65, CelebrateJumpDownRight = 66;

    /// <summary>Knocking, and looking around.</summary>
    public const int KnockLeft = 67, KnockRight = 68, LookAroundLeft = 69, LookAroundRight = 70;

    /// <summary>Shouting, and holding the shouting pose.</summary>
    public const int ShoutLeft = 71, ShoutRight = 72, ShoutingLeft = 73, ShoutingRight = 74;

    /// <summary>Looking up, and holding the pose.</summary>
    public const int LookUpLeft = 75, LookUpRight = 76, LookingUpLeft = 77, LookingUpRight = 78;

    /// <summary>Longer versions of the reaching animations.</summary>
    public const int TakeMidLeftLong = 79, TakeLowLeftLong = 80, TakeMidRightLong = 81;

    /// <summary>Taking the umbrella, taking the tin, opening something below.</summary>
    public const int TakeUmbrella = 82, TakeTin = 83, OpenDown = 84;

    /// <summary>Timmy: panting, scratching with a hind leg, wagging his tail.</summary>
    public const int PantLeft = 85, PantingLeft = 86, HindLegRight = 87, HindLegStillRight = 88, WagTailRight = 89;

    /// <summary>Timmy barking.</summary>
    public const int BarkRight = 90;

    /// <summary>Single scene animations.</summary>
    public const int FrightenedDownRight = 91, CryRight = 92, KeyholeLeft = 93, EatRight = 94, TeapotDownLeft = 95;

    /// <summary>Pushing the boat, pouring from the teapot, using the axe.</summary>
    public const int PushBoatRight = 96, PushBoatLeft = 97, TeapotRight = 98, TeapotLeft = 99, AxeRight = 100;

    /// <summary>Tying the rope, throwing, Timmy in the bucket.</summary>
    public const int TieRight = 101, TieLeft = 102, ThrowRight = 103, InBucketRight = 104, InBucketStillRight = 105;

    /// <summary>Watching, Timmy burrowing and sniffing, gone from view, crawling, throwing.</summary>
    public const int WatchLeft = 106, BurrowRight = 107, Gone = 108, SniffRight = 109, CrawlRight = 110, ThrowLeft = 111;

    /// <summary>The professor on the telephone.</summary>
    public const int PhoneRight = 112, PhoneStillRight = 113, PhoneTalkRight = 114;

    /// <summary>Idle: taking the hands out of the pockets again.</summary>
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
        [BinocularsUpLeft] = BinocularsLeft,
        [BinocularsUpRight] = BinocularsRight,
        [BinocularsDownLeft] = StandLeft,
        [BinocularsDownRight] = StandRight,
        [TakeMidLeft] = StandLeft,
        [TakeMidUpLeft] = StandUpLeft,
        [TakeMidUpRight] = StandUpRight,
        [TakeMidRight] = StandRight,
        [TakeLowLeft] = StandLeft,
        [TakeLowUpLeft] = StandUpLeft,
        [TakeLowUpRight] = StandUpRight,
        [TakeLowRight] = StandRight,
        [RiseDownLeft] = StandDownLeft,
        [RiseLeft] = StandDownLeft,
        [RiseRight] = StandDownLeft,
        [RiseDownRight] = StandDownRight,
        [TakeSeatDownLeft] = SitDownLeft,
        [TakeSeatLeft] = SitLeft,
        [TakeSeatRight] = SitRight,
        [TakeSeatDownRight] = SitDownRight,
        [KneelLeft] = ExamineGroundLeft,
        [KneelRight] = ExamineGroundRight,
        [KneelingLeft] = ExaminingGroundLeft,
        [KneelingRight] = ExaminingGroundRight,
        [GetUpLeft] = StandLeft,
        [GetUpRight] = StandRight,
        [CheerLeft] = StandLeft,
        [CheerRight] = StandRight,
        [ExamineGroundLeft] = GetUpLeft,
        [ExamineGroundRight] = GetUpRight,
        [HandsInPocketsLeft] = HandsInPocketsStillLeft,
        [HandsInPocketsRight] = HandsInPocketsStillRight,
        [JumpDownLeft] = StandDownLeft,
        [JumpDownRight] = StandDownRight,
        [CelebrateDownLeft] = StandDownLeft,
        [CelebrateDownRight] = StandDownRight,
        [CelebrateJumpDownLeft] = StandDownLeft,
        [CelebrateJumpDownRight] = StandDownRight,
        [KnockLeft] = StandLeft,
        [KnockRight] = StandRight,
        [LookAroundLeft] = StandLeft,
        [LookAroundRight] = StandRight,
        [ShoutLeft] = ShoutingLeft,
        [ShoutRight] = ShoutingRight,
        [LookUpLeft] = LookingUpLeft,
        [LookUpRight] = LookingUpRight,
        [TakeMidLeftLong] = StandLeft,
        [TakeLowLeftLong] = StandLeft,
        [TakeMidRightLong] = StandRight,
        [TakeUmbrella] = StandRight,
        [TakeTin] = StandLeft,
        [OpenDown] = StandRight,
        [PantLeft] = PantingLeft,
        [HindLegRight] = StandRight,
        [WagTailRight] = StandRight,
        [BarkRight] = StandRight,
        [FrightenedDownRight] = StandDownRight,
        [CryRight] = StandDownRight,
        [KeyholeLeft] = StandLeft,
        [EatRight] = SitRight,
        [TeapotDownLeft] = StandDownLeft,
        [PushBoatRight] = StandRight,
        [PushBoatLeft] = StandLeft,
        [TeapotRight] = StandRight,
        [TeapotLeft] = StandLeft,
        [AxeRight] = StandUpRight,
        [TieRight] = StandUpRight,
        [TieLeft] = StandUpLeft,
        [ThrowRight] = StandDownRight,
        [InBucketRight] = InBucketStillRight,
        [WatchLeft] = StandUpLeft,
        [BurrowRight] = Gone,
        [SniffRight] = BurrowRight,
        [ThrowLeft] = StandDownLeft,
        [PhoneRight] = PhoneStillRight,
        [HandsOutOfPocketsLeft] = StandDownLeft,
        [HandsOutOfPocketsRight] = StandDownRight,
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
    /// <summary>The step does not start before this is released (null: no wait).</summary>
    public TaskHandle? WaitFor;

    /// <summary>Ticks to wait after <see cref="WaitFor"/> is released.</summary>
    public int Delay;

    /// <summary>The step has reached the front of the queue.</summary>
    public bool Started { get; internal set; }

    /// <summary>The step is done (or was dropped).</summary>
    public bool Completed { get; internal set; }

    /// <summary>A queue entry releases what waits for it as soon as it is reached.</summary>
    public override bool Released => Started || Completed;
}

/// <summary>Walk in a straight line to (X, Y), arriving on <see cref="Shape"/>.</summary>
public sealed class WalkStep : ActorStep
{
    /// <summary>The end point.</summary>
    public int X, Y;

    /// <summary>The path or area the end point is on.</summary>
    public int Shape;
}

/// <summary>Change the character's state (stand, face, talk, take...).</summary>
public sealed class StateStep : ActorStep
{
    /// <summary>The new state (see <see cref="ActorStates"/>).</summary>
    public int State;
}

/// <summary>Walk to a point, working out the route only when the step is reached.</summary>
public sealed class RouteStep : ActorStep
{
    /// <summary>Where to walk to.</summary>
    public int X, Y;
}

/// <summary>Run an action (e.g. the click on the object the character walked to).</summary>
public sealed class ActionStep : ActorStep
{
    /// <summary>What to do.</summary>
    public required Action<GameWorld> Action;
}

/// <summary>A character: one of the Five, or another person the scripts move around.</summary>
public sealed class Actor
{
    /// <summary>The character's name ("Dick"); clicking the character clicks this name.</summary>
    public string Name { get; }

    /// <summary>Prefix of the character's state animations ("aDick_").</summary>
    public string AnimPrefix { get; }

    /// <summary>Position of the character's feet.</summary>
    public double X, Y;

    /// <summary>What the character is doing (see <see cref="ActorStates"/>).</summary>
    public int State = ActorStates.StandDownRight;

    /// <summary>The walk path or area the character stands on (-1: none).</summary>
    public int Shape = -1;

    /// <summary>Drawing size (smaller further away).</summary>
    public double Scale = 0.5;

    /// <summary>Drawing depth (larger is further away).</summary>
    public int Depth = 1000;

    /// <summary>Is the character shown (in its <see cref="Location"/>)?</summary>
    public bool Visible;

    /// <summary>Location the character is in.</summary>
    public string Location = "";

    /// <summary>Walking speed in pixels per tick at full size.</summary>
    public int SpeedX = 10, SpeedY = 4;

    /// <summary>What the character will do, in order (walk legs, poses, actions).</summary>
    public readonly LinkedList<ActorStep> Queue = new();

    /// <summary>The sprite that shows the character.</summary>
    public readonly Sprite Sprite = new();

    /// <summary>The character that follows this one around (the rest of the party), or null.</summary>
    public Actor? Follower;

    /// <summary>The animation the sprite shows ("" to restart it on the next tick).</summary>
    internal string AnimName { get; set; } = "";

    /// <summary>The frame of <see cref="AnimName"/> being shown.</summary>
    internal int Frame { get; set; }

    /// <summary>Creates a character whose animations are named <paramref name="animPrefix"/> + state suffix.</summary>
    public Actor(string name, string animPrefix)
    {
        Name = name;
        AnimPrefix = animPrefix;
    }

    /// <summary>X rounded to a pixel.</summary>
    public int IX => (int)Math.Round(X);

    /// <summary>Y rounded to a pixel.</summary>
    public int IY => (int)Math.Round(Y);

    /// <summary>Adds a step to the end of the queue and returns it (it can be waited for).</summary>
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
            if (dx == 0)
                dx = 1;
            sy = uy * sy / ux;
        }
        else
        {
            if (dy == 0)
                dy = 1;
            sx = uy == 0 ? double.PositiveInfinity : ux * sx / uy;
        }
        int nx = dx < 1 ? (int)Math.Round(IX - Math.Min(sx, Math.Abs(dx))) : (int)Math.Round(IX + Math.Min(sx, dx));
        int ny = dy < 1 ? (int)Math.Round(IY - Math.Min(sy, Math.Abs(dy))) : (int)Math.Round(IY + Math.Min(sy, dy));
        // dx/dy were forced to 1 to avoid a division by zero; do not move past the target on that axis.
        if (tx == IX)
            nx = IX;
        if (ty == IY)
            ny = IY;
        X = nx;
        Y = ny;
    }
}
