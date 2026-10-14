using OpenFive.Engine.Game;
using OpenFive.Engine.World;

namespace OpenFive.Engine.Story;

/// <summary>
/// The top of the well in the castle courtyard (loc18), seen from above with the children's
/// heads around it. On quest 8 Timmy has fallen in: the hero climbs down the rope to fetch him.
/// Climbing is shown by moving the rope: up when climbing out, down when climbing in.
/// </summary>
public sealed class WellTop : StoryScript
{
    /// <summary>The values of <see cref="Vars.WellRopeState"/>.</summary>
    static class Rope
    {
        /// <summary>Nothing moves.</summary>
        public const int Still = 0;
        /// <summary>The hero has just arrived: the rope comes down into view.</summary>
        public const int Arriving = 1;
        /// <summary>The rope is in place; the exits can be used.</summary>
        public const int Ready = 2;
        /// <summary>Climbing out: the rope moves up.</summary>
        public const int ClimbingOut = 3;
        /// <summary>Climbed out: on to the courtyard.</summary>
        public const int Out = 4;
        /// <summary>Climbing down: the rope moves down.</summary>
        public const int ClimbingDown = 5;
        /// <summary>Climbed down: on into the shaft.</summary>
        public const int Down = 6;
    }

    /// <summary>The rope animation being played (the rope is moved while it runs).</summary>
    TaskHandle? _rope;

    /// <inheritdoc/>
    public override string Location => "loc18";

    /// <summary>Shows the scene of the story step: Timmy's cries (quest 8, step 3), or the rope.</summary>
    /// <remarks>The original's FUN_00454687 and FUN_00454705.</remarks>
    protected override void Setup()
    {
        Obj("LOC18BigClick", false);
        Obj("LOC18ExitToLoc17", false);
        Obj("LOC18ExitToLoc19", false);
        StopAnim("4heads");
        StopAnim("RopeDown");
        if (At(8, 3))
            Episode = 4;
        if (Quest == 8 && Episode < 4)
        {
            if (Episode == 2)
            {
                Episode = 3;
                SetVar(Vars.AnneCastleTalk, 0);
                SetVar(Vars.GeorgeCastleTalk, 0);
                SetVar(Vars.JulianCastleTalk, 0);
            }
            if (Episode == 3)
                HearTimmy();
        }
        else if (At(8, 5))
        {
            ShowRope();
            HangRope();
            Obj("LOC18ExitToLoc17", true);
        }
        else if (At(8, 4))
        {
            SetVar(Vars.WellRopeState, Rope.Still);
            ShowRope();
            HangRope();
            Obj("LOC18ExitToLoc17", true);
            Obj("LOC18ExitToLoc19", true);
        }
        else
        {
            ShowRope();
            SetVar(Vars.WellRopeState, Rope.Arriving);
            AnimPos("RopeDown", 200, -300);
            AnimPos("RopeDownStill", 200, Var(Vars.WellRopeY));
            _rope = Anim("RopeDown", 2);
            Anim("RopeDownStill", -1, _rope);
        }
    }

    /// <summary>The children look into the well and hear Timmy whimper below.</summary>
    void HearTimmy()
    {
        Depth(@"bmp\obj\misc\fh\fh.brg", 500);
        Anim("4heads", -1);
        Obj("LOC18BigClick", true);
        Chain = null;
        PointerOff();
        Sound(@"wav\timmy\schmerz.wav");
        Sound(@"wav\timmy\schmerz.wav", delay: 6);
        Sound(@"wav\timmy\schmerz.wav", delay: 12);
        Say(Who.George, "BRUNNEN0039", 1, delay: 4);
        Say(Who.Dick, "BRUNNEN0039", 2, last: true);
    }

    /// <summary>Prepares the rope animations (double size, at the top) and the click area of the well.</summary>
    void ShowRope()
    {
        Obj("LOC18BigClick", true);
        Depth(@"bmp\obj\dick\dicl\dicl01.brg", 90);
        Depth(@"bmp\obj\dick\dicl\dicl00.brg", 90);
        foreach (string anim in new[] { "RopeUp", "RopeUpStill", "RopeDown", "RopeDownStill" })
            AnimScale(anim, 2.0);
        SetVar(Vars.WellRopeY, -300);
    }

    /// <summary>Shows the rope hanging still.</summary>
    void HangRope()
    {
        AnimPos("RopeDown", 200, -300);
        AnimPos("RopeDownStill", 200, Var(Vars.WellRopeY));
        Anim("RopeDownStill", -1);
    }

    /// <summary>The characters are not shown here.</summary>
    protected override void Arrive() { }

    /// <summary>Back to the courtyard, climbing out or down the rope, or looking at the ladder.</summary>
    /// <remarks>The original's FUN_00454435.</remarks>
    public override bool Click(string name)
    {
        if (Same(name, "LOC18BigClick"))
        {
            if (At(8, 3))
                ChangeLocation("loc17");
        }
        else if (Same(name, "LOC18ExitToLoc17"))
        {
            if (At(8, 4) && Var(Vars.WellShaftVisited) == 0)
            {
                // George will not leave Timmy down there.
                Chain = null;
                Say(Who.George, "BRUNNEN0041", 1, last: true);
            }
            else
                StartClimbing(Rope.ClimbingOut, "RopeUp", "RopeUpStill");
        }
        else if (Same(name, "LOC18ExitToLoc19"))
            StartClimbing(Rope.ClimbingDown, "RopeDown", "RopeDownStill");
        else if (Same(name, "LOC18Leiter") && At(8, 3))
        {
            PointerOff();
            Chain = null;
            Say(Who.Dick, "BRUNNEN0039", 3, last: true);
        }
        return true;
    }

    /// <summary>Starts moving the rope (<paramref name="state"/> says which way) with its animation.</summary>
    void StartClimbing(int state, string anim, string still)
    {
        SetVar(Vars.WellRopeState, state);
        StopAnim("RopeDownStill");
        StopAnim("RopeUpStill");
        _rope = Anim(anim, 4);
        Anim(still, -1, _rope);
    }

    /// <summary>Moves the rope while it is moving, then shows the exits or goes on to the next place.</summary>
    /// <remarks>The original's FUN_00454bfb.</remarks>
    public override void EveryTick()
    {
        switch (Var(Vars.WellRopeState))
        {
            case Rope.Arriving:
                if (!IsActive(_rope))
                    SetVar(Vars.WellRopeState, Rope.Ready);
                else
                    MoveRope(4, "RopeDown", "RopeDownStill");
                break;
            case Rope.Ready:
                Obj("LOC18ExitToLoc17", true);
                if (Var(Vars.TimmyRescued) == 0)
                    Obj("LOC18ExitToLoc19", true);
                break;
            case Rope.ClimbingOut:
                if (!IsRunning(_rope))
                    SetVar(Vars.WellRopeState, Rope.Out);
                else
                    MoveRope(-4, "RopeUp", "RopeUpStill");
                break;
            case Rope.Out:
                SetVar(Vars.WellRopeState, Rope.Still);
                ChangeLocation("loc17");
                break;
            case Rope.ClimbingDown:
                if (!IsRunning(_rope))
                    SetVar(Vars.WellRopeState, Rope.Down);
                else
                    MoveRope(4, "RopeDown", "RopeDownStill");
                break;
            case Rope.Down:
                SetVar(Vars.WellRopeState, Rope.Still);
                ChangeLocation("loc19");
                break;
        }
    }

    /// <summary>Moves the rope (both its animations) by <paramref name="dy"/> pixels.</summary>
    void MoveRope(int dy, string anim, string still)
    {
        SetVar(Vars.WellRopeY, Var(Vars.WellRopeY) + dy);
        AnimPos(anim, 200, Var(Vars.WellRopeY));
        AnimPos(still, 200, Var(Vars.WellRopeY));
    }
}
