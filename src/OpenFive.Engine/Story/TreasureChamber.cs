using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The treasure chamber (loc27). On quest 9 the children find the gold, and the professor and
/// his accomplice catch them; afterwards they are locked into the cell (loc28, quest 10).
/// </summary>
public sealed class TreasureChamber : StoryScript
{
    /// <summary>Which conversation is running: "start" (finding the gold), then "Anne" (being caught).</summary>
    const string Scene = "t63bedc";

    /// <inheritdoc/>
    public override string Location => "loc27";

    /// <summary>The hero comes in through the broken door; on quest 9 the others are here and the finding scene starts.</summary>
    /// <remarks>The original's FUN_004591db and FUN_0045926e.</remarks>
    protected override void Setup()
    {
        Hero.Follower = null;
        SetVar(Vars.ProfessorArrived, 0);
        foreach (string anim in new[] { "ProfSpeak", "ProfStand", "KomplizeStand", "KomplizeSpeak" })
            StopAnim(anim);
        Obj("LOC27ExitToLoc24", true);
        Place(Hero, 296, 582, StandUpRight);
        Show(Hero);
        if (At(9, 1))
            FindTheGold();
        ShowParty();
    }

    /// <summary>
    /// Quest 10 begins: the four of them stand around the gold and talk; the professor and his
    /// accomplice are made ready at the door (they come in after the last line).
    /// </summary>
    void FindTheGold()
    {
        SetStep(10, 1);
        Obj("LOC27ExitToLoc24", false);
        Place(George, 443, 500, StandDownLeft);
        Place(Julian, 620, 493, StandDownLeft);
        Place(Anne, 171, 544, StandUpRight);
        Chain = null;
        SetText(Scene, "start");
        PointerOff();
        Depth(@"bmp\obj\prof\prsp\prsp01.brg", 5000);
        Depth(@"bmp\obj\prof\prst\prst.brg", 5000);
        Depth(@"bmp\obj\komplize\kogu06.brg", 6000);
        Depth(@"bmp\obj\komplize\kosp\kosp06.brg", 6000);
        AnimPos("ProfStand", 330, 150);
        AnimPos("ProfSpeak", 355, 152);
        AnimPos("KomplizeStand", 270, 152);
        AnimPos("KomplizeSpeak", 230, 146);
        AnimScale("ProfSpeak", 1.66);
        AnimScale("ProfStand", 0.9);
        AnimScale("KomplizeStand", 1.66);
        AnimScale("KomplizeSpeak", 1.66);
        Say(Who.George, "SCHATZ0047", 1, delay: 4);
        Say(Who.Anne, "SCHATZ0047", 2);
        Say(Who.Julian, "SCHATZ0047", 3);
        Say(Who.Dick, "SCHATZ0047", 4, last: true);
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>The way back out to the treasure door.</summary>
    /// <remarks>The original's FUN_0045915b.</remarks>
    public override bool Click(string name)
    {
        if (Same(name, "LOC27ExitToLoc24"))
            ChangeLocation("loc24", WalkAndFace(351, 423, StandUpLeft));
        return true;
    }

    /// <summary>The speaker takes their talking pose (the professor and accomplice switch animations).</summary>
    /// <remarks>The original's FUN_0045958b.</remarks>
    public override void LineStarting(GameWorld world, int speaker)
    {
        switch (CurrentSpeaker)
        {
            case Who.Julian:
                StateAfter(Julian, SpeakDownLeft, null);
                break;
            case Who.Anne:
                StateAfter(Anne, StandUpRight, null);
                break;
            case Who.George:
                StateAfter(George, Var(Vars.ProfessorArrived) == 0 ? SpeakDownLeft : StandUpLeft, null);
                break;
            case Who.Professor:
                StopAnimAfter("ProfStand", null);
                Anim("ProfSpeak", -1);
                break;
            case Who.Accomplice:
                StopAnimAfter("KomplizeStand", null);
                Anim("KomplizeSpeak", -1);
                break;
        }
    }

    /// <summary>The speaker stands again after their line.</summary>
    /// <remarks>The original's FUN_004596cb.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end)
    {
        switch (CurrentSpeaker)
        {
            case Who.Julian:
            case Who.George:
            case Who.Professor:
                StandAfterLine(end);
                break;
            case Who.Accomplice:
                StopAnimAfter("KomplizeSpeak", end);
                Anim("KomplizeStand", -1, end);
                break;
        }
    }

    /// <summary>
    /// After the finding scene the professor and accomplice appear and the children are caught;
    /// after Anne's last line they are taken to the cell (quest 10, step 2).
    /// </summary>
    /// <remarks>The original's FUN_004597e3.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (At(10, 1))
        {
            switch (CurrentSpeaker)
            {
                case Who.Julian:
                case Who.George:
                case Who.Professor:
                    StandAfterLine(end);
                    break;
                case Who.Dick when Text(Scene) == "start":
                    Caught(end);
                    return;
                case Who.Anne when Text(Scene) == "Anne":
                    SetAfter(Vars.QuestNumber, 10, end);
                    SetAfter(Vars.EpisodeNumber, 2, end);
                    PointerAfter(false, end, x: 400, y: 400);
                    ChangeLocation("loc28", end, 4);
                    SetVar(Vars.GeorgeDungeonTalk, 0);
                    SetText(Scene, "");
                    break;
            }
        }
        SetText(Scene, "");
    }

    /// <summary>Julian, George or the professor stands again after a line.</summary>
    void StandAfterLine(TaskHandle end)
    {
        switch (CurrentSpeaker)
        {
            case Who.Julian:
                StateAfter(Julian, StandDownLeft, end);
                break;
            case Who.George:
                StateAfter(George, Var(Vars.ProfessorArrived) == 0 ? StandDownLeft : StandUpLeft, end);
                break;
            case Who.Professor:
                StopAnimAfter("ProfSpeak", end);
                Anim("ProfStand", -1, end);
                break;
        }
    }

    /// <summary>The professor and his accomplice appear; Anne cries; the conversation of being caught.</summary>
    void Caught(TaskHandle end)
    {
        StateAfter(Anne, CryRight, end);
        StateAfter(George, StandUpLeft, end);
        Anim("ProfStand", -1, end);
        Anim("KomplizeStand", -1, end);
        PointerAfter(false, end, x: 400, y: 400);
        Chain = end;
        SetVar(Vars.ProfessorArrived, 1);
        Say(Who.Accomplice, "SCHATZ0047", 5, delay: 2);
        Say(Who.Professor, "SCHATZ0047", 6);
        Say(Who.George, "SCHATZ0047", 7);
        Say(Who.Professor, "SCHATZ0047", 8);
        Say(Who.Dick, "SCHATZ0047", 9);
        Say(Who.Professor, "SCHATZ0047", 10);
        Say(Who.Professor, "SCHATZ0047", 15);
        Say(Who.Professor, "SCHATZ0047", 16);
        Say(Who.Accomplice, "SCHATZ0047", 11);
        Say(Who.Professor, "SCHATZ0047", 12);
        Say(Who.Accomplice, "SCHATZ0047", 13);
        Say(Who.Anne, "SCHATZ0047", 14, last: true);
        SetText(Scene, "Anne");
    }
}
