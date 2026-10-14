using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// Inside the well shaft (loc19), where Timmy has fallen (quest 8, step 4). With the bucket the
/// hero can lower it on the rope and pull Timmy up; without it they climb back out.
/// </summary>
public sealed class WellShaft : StoryScript
{
    /// <inheritdoc/>
    public override string Location => "loc19";

    /// <summary>Sets up the rope and Timmy, then plays Timmy's bark.</summary>
    /// <remarks>The original's FUN_00454f6a and FUN_0045500d.</remarks>
    protected override void Setup()
    {
        Depth(@"bmp\obj\timmy\tihl\tihl1902.brg", 1000);
        Depth(@"bmp\obj\dick\dicl\dicl03.brg", 500);
        foreach (string anim in new[] { "RopeExit", "Rope", "Seil", "Wedel", "WedelStop", "TimmyStand" })
            StopAnim(anim);
        SetVar(Vars.WellShaftVisited, 1);
        Obj("SeilKlick", false);
        Obj("LOC19Timmy", false);
        if (At(8, 4))
        {
            if (Has(Item.Bucket))
                ClimbDown();
            else
                LookDown();
        }
        Sound(@"wav\timmy\timmy04.wav");
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>
    /// With the bucket, Dick climbs down the rope to Timmy (who wags his tail), and the rope
    /// can then be clicked to use the bucket on it.
    /// </summary>
    void ClimbDown()
    {
        PointerOff();
        AnimScale("RopeExit", 1.6);
        AnimScale("Seil", 1.6);
        AnimPos("RopeExit", 200, -320);
        AnimPos("Seil", 350, -320);
        foreach (string anim in new[] { "TimmyStand", "WedelStop", "Wedel" })
        {
            AnimScale(anim, 1.3);
            AnimPos(anim, 100, 300);
        }
        Anim("Wedel", -1);
        var down = Anim("RopeExit");
        Anim("Seil", -1, down);
        StopAnimAfter("Wedel", down, 20);
        Anim("WedelStop", after: down, delay: 18);
        Hero = Dick;
        PlaceAfter(Dick, 257, 522, StandDownLeft, null, 12);
        PlaceAfter(Timmy, 257, 200, StandLeft, null, 35);
        PointerAfter(true, null, 40, 400, 400);
        ObjAfter("SeilKlick", true, down, 12);
        Obj("LOC19Timmy", true);
    }

    /// <summary>Without the bucket, Dick only looks down at Timmy and says so; then back up.</summary>
    void LookDown()
    {
        AnimScale("Rope", 1.6);
        AnimPos("Rope", 200, -320);
        Anim("Rope", -1);
        AnimScale("Wedel", 1.3);
        AnimPos("Wedel", 100, 300);
        Anim("Wedel", -1);
        Chain = null;
        PointerOff();
        Say(Who.Dick, "BRUNNEN0042", 4, delay: 3);
        ChangeLocation("loc18", delay: 50);
    }

    /// <summary>Talking to Timmy: Dick calls down to him once; with Timmy in the bucket, he says something else.</summary>
    /// <remarks>The original's FUN_00454dfa.</remarks>
    public override bool Click(string name)
    {
        if (!Same(name, "LOC19Timmy") || !At(8, 4))
            return true;
        bool inBucket = Var(Vars.TimmyInBucket) >= 1;
        if (!inBucket && Var(Vars.TimmyCalledInWell) != 0)
            return true;
        Walk(Hero, 269, 440);
        Chain = State(Hero, StandDownLeft);
        if (inBucket)
            Say(Who.Dick, "BRUNNEN0042", 5, last: true);
        else
        {
            IncAfter(Vars.TimmyCalledInWell, Chain);
            Say(Who.Dick, "BRUNNEN0042", 1, last: true);
        }
        return true;
    }

    /// <inheritdoc/>
    protected override TalkPoses? PosesOf(int speaker) =>
        speaker == Who.Dick ? new TalkPoses(Hero, SpeakDownLeft, StandDownLeft) : null;

    /// <summary>Dick takes his talking pose.</summary>
    /// <remarks>The original's FUN_004553c5.</remarks>
    public override void LineStarting(GameWorld world, int speaker) => StartTalking();

    /// <summary>Dick stands again; once Timmy is in the bucket, they climb back up with him (quest 8, step 5).</summary>
    /// <remarks>The original's FUN_00455415.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        StopTalking(end);
        if (Text(StoryTexts.WellRescue) == "Tim")
        {
            ChangeLocation("loc18", end);
            SetStep(8, 5);
            SetVar(Vars.TimmyRescued, 1);
            SetVar(Vars.AnneCastleTalk, 0);
            SetVar(Vars.GeorgeCastleTalk, 0);
            SetVar(Vars.JulianCastleTalk, 0);
        }
    }
}
