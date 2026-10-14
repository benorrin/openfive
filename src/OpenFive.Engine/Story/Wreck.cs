using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// On the deck of the wreck (loc15), where the children sit and talk. On quest 6 they explore it
/// (step 3) and talk about the cabin (step 4); on quest 7 they come back with the tin.
/// </summary>
public sealed class Wreck : IslandScript
{
    /// <inheritdoc/>
    public override string Location => "loc15";

    /// <summary>Everyone sits on deck; the boats rock. The talk of the story step starts.</summary>
    /// <remarks>The original's FUN_0044b738 and FUN_0044b852.</remarks>
    protected override void Setup()
    {
        SetVar(Vars.V683260, 0);
        SetVar(Vars.WreckTinTalkPending, 0);
        Place(George, 470, 455, SitDownLeft);
        Place(Julian, 200, 455, SitDownRight);
        Place(Hero, 370, 455, SitDownLeft);
        Place(Anne, 275, 475, SitDownRight);
        Place(Timmy, 400, 475, SitRight);
        Show(Hero);
        ShowParty();
        Depth(@"bmp\obj\locs\loc15\boot.brg", 8000);
        Depth(@"bmp\obj\locs\loc15\boot2.brg", 1000);
        Anim("Boot", -1);
        Anim("Boot2", -1);
        if (At(6, 3))
        {
            SetVar(Vars.CabinVisited, 1);
            if (Var(Vars.WreckVisit) == 0)
            {
                Chain = null;
                Say(Who.Dick, "WRACK0023", 1);
                Say(Who.Anne, "WRACK0023", 2);
                Say(Who.Julian, "WRACK0023", 3);
                Say(Who.George, "WRACK0023", 4, last: true);
                SetVar(Vars.WreckVisit, 1);
            }
        }
        else if (At(6, 4))
        {
            if (Var(Vars.WreckVisit) < 2)
                SetVar(Vars.WreckVisit, 2);
            if (Var(Vars.WreckVisit) == 2)
            {
                Chain = null;
                Say(Who.Julian, "ALL0024", 1);
                Say(Who.Dick, "ALL0024", 2);
                Say(Who.Julian, "ALL0024", 3);
                Say(Who.Dick, "ALL0024", 4, last: true);
                SetVar(Vars.WreckVisit, 3);
            }
        }
        else if (At(7, 1) && Has(Item.ClosedTin))
        {
            SetVar(Vars.WreckTinTalkPending, 1);
            Chain = null;
            Say(Who.Anne, "WRACK0025", 1);
            Say(Who.Dick, "WRACK0025", 2);
            Say(Who.Julian, "WRACK0025", 3);
            Say(Who.Dick, "WRACK0025", 4);
            Say(Who.George, "WRACK0025", 5);
            Say(Who.Dick, "WRACK0025", 6);
            Say(Who.Julian, "WRACK0025", 7, last: true);
        }
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>Talking to the others, or back to the boat landing.</summary>
    /// <remarks>The original's FUN_0044a60b.</remarks>
    public override bool Click(string name)
    {
        Chain = null;
        if (Same(name, "LOC15ExitToLoc13"))
        {
            PointerOff();
            ChangeLocation("loc13", delay: 5);
            SetVar(Vars.V683260, 1);
        }
        else if (Same(name, "LOC15Anne"))
            TalkToAnne();
        else if (Same(name, "LOC15George"))
            TalkToGeorge();
        else if (Same(name, "LOC15Julian"))
            TalkToJulian();
        return true;
    }

    /// <summary>What Anne says at each story step.</summary>
    void TalkToAnne()
    {
        int talk = Var(Vars.AnneWreckTalk);
        if (At(6, 4))
        {
            if (talk == 0)
            {
                IncAfter(Vars.AnneWreckTalk, null);
                IncAfter(Vars.AnneBeachTalk, null);
                IncAfter(Vars.AnneIslandTalk, null);
                Say(Who.Anne, "ALL0024", 5, last: true);
            }
            else if (talk == 1)
            {
                Say(Who.Anne, "ALL0024", 6);
                Say(Who.Dick, "ALL0024", 7, last: true);
            }
        }
        else if (At(6, 3))
        {
            if (talk == 0)
            {
                SetVar(Vars.AnneWreckTalk, 1);
                Say(Who.Anne, "WRACK0023", 5);
                Say(Who.Dick, "WRACK0023", 6, last: true);
            }
            else if (talk == 1)
                Say(Who.Anne, "WRACK0023", 7, last: true);
        }
        else if (At(7, 1))
        {
            switch (talk)
            {
                case 0:
                    IncAfter(Vars.AnneWreckTalk, null);
                    Say(Who.Anne, "WRACK0025", 11, last: true);
                    break;
                case 1:
                    Say(Who.Anne, "WRACK0025", 12, last: true);
                    break;
                case 2:
                    IncAfter(Vars.AnneWreckTalk, null);
                    Say(Who.Anne, "WRACK0025", 19);
                    Say(Who.Dick, "WRACK0025", 20, last: true);
                    break;
                case 3:
                    Say(Who.Anne, "WRACK0025", 21, last: true);
                    break;
            }
        }
    }

    /// <summary>What George says at each story step (about the cupboard, the screwdriver and the rope).</summary>
    void TalkToGeorge()
    {
        int talk = Var(Vars.GeorgeWreckTalk);
        if (At(6, 4))
        {
            if (talk == 0)
            {
                IncAfter(Vars.GeorgeWreckTalk, null);
                IncAfter(Vars.GeorgeBeachTalk, null);
                IncAfter(Vars.GeorgeIslandTalk, null);
                TalkAboutTheCupboard();
            }
            else if (talk == 1)
                Say(Who.George, "ALL0024", 20, last: true);
        }
        else if (At(6, 3))
        {
            switch (talk)
            {
                case 0:
                    Say(Who.George, "WRACK0023", 13, last: true);
                    SetVar(Vars.GeorgeWreckTalk, 1);
                    break;
                case 1:
                    Say(Who.George, "WRACK0023", 14, last: true);
                    SetVar(Vars.GeorgeWreckTalk, 2);
                    break;
                case 2:
                    Say(Who.George, "WRACK0023", 15, last: true);
                    if (!Has(Item.Rope))
                        Say(Who.Julian, "WRACK0023", 16, last: true);
                    break;
            }
        }
        else if (At(7, 1))
        {
            if (talk == 0)
                Say(Who.George, "WRACK0025", 14, last: true);
            else if (talk == 2)
                Say(Who.George, "WRACK0025", 18, last: true);
        }
    }

    /// <summary>What Julian says at each story step.</summary>
    void TalkToJulian()
    {
        int talk = Var(Vars.JulianWreckTalk);
        if (At(6, 4))
        {
            if (talk == 0)
            {
                IncAfter(Vars.JulianWreckTalk, null);
                IncAfter(Vars.JulianBeachTalk, null);
                IncAfter(Vars.JulianIslandTalk, null);
                Say(Who.Julian, "ALL0024", 8, last: true);
            }
            else if (talk == 1)
                Say(Who.Julian, "ALL0024", 9, last: true);
        }
        else if (At(6, 3))
        {
            switch (talk)
            {
                case 0:
                    Say(Who.Julian, "WRACK0023", 8);
                    Say(Who.George, "WRACK0023", 9, last: true);
                    SetVar(Vars.JulianWreckTalk, 1);
                    break;
                case 1:
                    Say(Who.Julian, "WRACK0023", 10);
                    Say(Who.George, "WRACK0023", 11, last: true);
                    SetVar(Vars.JulianWreckTalk, 2);
                    break;
                case 2:
                    Say(Who.Julian, "WRACK0023", 12, last: true);
                    break;
            }
        }
        else if (At(7, 1))
        {
            switch (talk)
            {
                case 0:
                    Say(Who.Julian, "WRACK0025", 13, last: true);
                    break;
                case 2:
                    IncAfter(Vars.JulianWreckTalk, null);
                    Say(Who.Julian, "WRACK0025", 16, last: true);
                    break;
                case 3:
                    Say(Who.Julian, "WRACK0025", 17, last: true);
                    break;
            }
        }
    }

    /// <summary>Everyone talks sitting down.</summary>
    protected override TalkPoses? PosesOf(int speaker) => speaker switch
    {
        Who.Anne => new TalkPoses(Anne, SitSpeakDownRight, SitDownRight),
        Who.Julian => new TalkPoses(Julian, SitSpeakDownRight, SitDownRight),
        Who.Dick => new TalkPoses(Hero, SitSpeakDownLeft, SitDownLeft),
        Who.George => new TalkPoses(George, SitSpeakDownLeft, SitDownLeft),
        _ => null,
    };

    /// <summary>The hero stops talking (if still), then the speaker takes their talking pose.</summary>
    /// <remarks>The original's FUN_0044be30.</remarks>
    public override void LineStarting(GameWorld world, int speaker)
    {
        if (Hero.State == SitSpeakDownLeft)
            Hero.State = SitDownLeft;
        StartTalking();
    }

    /// <summary>The speaker sits still again after their line.</summary>
    /// <remarks>The original's FUN_0044bf31.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end) => StopTalking(end);

    /// <summary>The last speaker sits still; after the talk about the tin, Dick is offered the menu about it.</summary>
    /// <remarks>The original's FUN_0044c01b.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        StopTalking(end);
        if (CurrentSpeaker == Who.Julian && At(7, 1) &&
            Var(Vars.WreckTinTalkPending) != 0 && Var(Vars.WreckTinMenuAvailable) != 0)
        {
            Ask(Who.DickAlt, "WRACK0025", 8);
            SetVar(Vars.WreckTinTalkPending, 0);
            SetVar(Vars.WreckTinMenuAvailable, 0);
        }
    }

    /// <summary>The answer to the tin menu, and how far the talks move on after it.</summary>
    /// <remarks>The original's FUN_0044c186.</remarks>
    protected override void AfterChoice(string scene, int line, int option)
    {
        if (Var(ChoicePending) == 0)
            return;
        switch (option)
        {
            case 1:
                Say(Who.Anne, "WRACK0025", 9);
                Say(Who.George, "WRACK0025", 10, last: true);
                SetWreckTalks(0);
                break;
            case 2:
                PointerAfter(true, Chain, x: 100, y: 200);
                SetWreckTalks(2);
                break;
            case 3:
                PointerAfter(true, Chain, x: 100, y: 200);
                SetWreckTalks(4);
                break;
        }
        SetVar(ChoicePending, 0);
    }

    /// <summary>Sets how far the talks with Anne, Julian and George have got, once the current line has ended.</summary>
    void SetWreckTalks(int value)
    {
        SetAfter(Vars.AnneWreckTalk, value, Chain);
        SetAfter(Vars.JulianWreckTalk, value, Chain);
        SetAfter(Vars.GeorgeWreckTalk, value, Chain);
    }
}
