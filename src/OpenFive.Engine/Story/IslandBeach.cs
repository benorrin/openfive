using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The beach of Kirrin Island (loc14), below the castle. The others wait here while the hero
/// explores; what they say depends on the story step. On quest 11 the Five meet here after
/// escaping from the dungeon.
/// </summary>
public sealed class IslandBeach : IslandScript
{
    /// <summary>"Exit" while the talk on the way up to the castle runs.</summary>
    const string GoingUp = "t63be48";

    /// <inheritdoc/>
    public override string Location => "loc14";

    /// <summary>Where everyone is, the boats, the gulls; the pointer stays off for the escape scene.</summary>
    /// <remarks>The original's FUN_004497ba.</remarks>
    protected override void Setup()
    {
        SetVar(Vars.V683260, 1);
        Obj("LOC14Anne", false);
        Obj("LOC14George", false);
        Obj("LOC14Julian", false);
        PlaceHero();
        MoveObj("LOC14Boot1", 160, 361);
        MoveObj("LOC14Boot2", 268, 286);
        MoveObj("LOC14Boot3", 365, 388);
        MoveObj("LOC14Boot4", 502, 363);
        Anim("Moewen", -1);
        if (Quest != 11 || Var(Vars.EscapeSceneSeen) != 0)
            PointerOn();
    }

    /// <summary>Where the hero appears, depending on where they came from; then the others.</summary>
    /// <remarks>The original's FUN_0044988b.</remarks>
    void PlaceHero()
    {
        Depth(@"bmp\obj\fass\fass.brg", 50);
        Depth(@"bmp\obj\fass\fass0.brg", 50);
        Depth(@"bmp\obj\fass\fass9.brg", 50);
        switch (FromId)
        {
            case 13: // the landing place
            case 17: // down from the castle
                Place(Hero, 476, 410, StandDownLeft);
                Show(Hero);
                WalkAndFace(400, 450, StandDownLeft);
                break;
            case 20: // back from the well, everyone together
                Place(Timmy, 550, 387, StandDownLeft);
                Place(Julian, 700, 387, StandDownLeft);
                Place(Anne, 600, 387, StandDownLeft);
                Place(George, 650, 387, StandDownLeft);
                Walk(Timmy, 144, 580);
                State(Timmy, StandUpRight);
                Walk(Julian, 126, 506);
                State(Julian, StandDownRight);
                Walk(Anne, 250, 475);
                State(Anne, StandDownRight);
                Walk(George, 300, 600);
                State(George, StandUpRight);
                Place(Hero, 792, 387, StandDownLeft);
                Show(Hero);
                Walk(Hero, 400, 450);
                Chain = State(Hero, StandDownLeft);
                Obj("LOC14Anne", true);
                Obj("LOC14George", true);
                Obj("LOC14Julian", true);
                break;
            case 50: // from the main menu (a new chapter)
                Place(Hero, 840, 400, StandDownLeft);
                Show(Hero);
                WalkAndFace(750, 400, StandLeft);
                PointerOff();
                break;
            default:
                Place(Hero, 400, 450, StandDownLeft);
                Show(Hero);
                break;
        }
        PlaceTheOthers();
        ShowParty();
    }

    /// <summary>
    /// The others wait on the beach on quests 6 (from step 3) and 7 (steps 1-2), later with Dick
    /// in George's place. On quest 11 they talk about the escape first.
    /// </summary>
    /// <remarks>The original's FUN_00449bf4.</remarks>
    void PlaceTheOthers()
    {
        Obj("LOC14ExitToLoc13", true);
        if (Quest == 6 && Episode >= 3)
            WaitOnTheBeach(George);
        else if (Episode < 3 && Quest == 7)
            WaitOnTheBeach(George);
        else if (Quest == 11)
        {
            Obj("LOC14ExitToLoc13", false);
            if (Var(Vars.EscapeSceneSeen) == 0)
            {
                PointerOff();
                IncAfter(Vars.EscapeSceneSeen, Chain);
                Say(Who.Julian, "STRAND0052", 1);
                Say(Who.Dick, "STRAND0052", 2);
                Say(Who.George, "STRAND0052", 3);
                Say(Who.Anne, "STRAND0052", 4);
                Say(Who.Dick, "STRAND0052", 5);
                Say(Who.Julian, "STRAND0052", 6);
                Say(Who.Julian, "STRAND0052", 7, last: true);
            }
            else
                WaitOnTheBeach(George);
        }
        else if (Quest > 7)
            WaitOnTheBeach(Dick);
    }

    /// <summary>Anne, Julian, Timmy and <paramref name="fourth"/> (George or Dick) wait on the beach and can be talked to.</summary>
    /// <remarks>The original's FUN_00449ec0 and FUN_00449f70.</remarks>
    void WaitOnTheBeach(Actor fourth)
    {
        Obj("LOC14Anne", true);
        Obj("LOC14George", true);
        Obj("LOC14Julian", true);
        Place(fourth, 300, 600, StandUpRight);
        Place(Timmy, 144, 580, StandUpRight);
        Place(Anne, 250, 475, StandDownRight);
        Place(Julian, 126, 506, StandDownRight);
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>The ways up to the castle and back to the landing, the barrel, and talking to the others.</summary>
    /// <remarks>The original's FUN_004476f9.</remarks>
    public override bool Click(string name)
    {
        Chain = null;
        if (Same(name, "LOC14ExitToLoc17"))
            GoUpToTheCastle();
        else if (Same(name, "LOC14ExitToLoc13"))
            ChangeLocation("loc13");
        else if (Same(name, "LOC14Fass0"))
        {
            Walk(Hero, 260, 575);
            var opened = ObjAfter("LOC14Fass0", false, null);
            ObjAfter("LOC14Fass9", true, opened);
            SetVar(Vars.IslandBarrelOpen, 1);
            PointerOff();
            // (The original then reads a dialogue file the disc does not have: nothing is said.)
        }
        else if (Same(name, "LOC14Fass9"))
        {
            SetVar(Vars.IslandBarrelOpen, 0);
            // The original switched the language here; in OpenFive the language cannot be set
            // as a story variable, so this does nothing.
            SetVar(Vars.LanguageNumber, German ? 2 : 1);
        }
        else if (Same(name, "LOC14Anne"))
        {
            Walk(Hero, 336, 455);
            Chain = State(Hero, StandDownLeft);
            TalkToAnne();
        }
        else if (Same(name, "LOC14George"))
        {
            Walk(Hero, 350, 500);
            Chain = State(Hero, StandDownLeft);
            TalkToGeorge();
        }
        else if (Same(name, "LOC14Julian"))
        {
            Walk(Hero, 203, 507);
            Chain = State(Hero, StandDownLeft);
            TalkToJulian();
        }
        return true;
    }

    /// <summary>
    /// The path up to the castle: before the cabin has been seen (quest 6) the others talk the
    /// hero out of it; on quest 7, step 2 they talk about going; from quest 8 it is open.
    /// </summary>
    void GoUpToTheCastle()
    {
        Walk(Hero, 790, 400);
        Chain = State(Hero, StandRight);
        SetText(GoingUp, "Exit");
        if (BeforeTheCabin)
        {
            Say(Who.Julian, "INSELSTRAND0022", 6);
            Say(Who.George, "INSELSTRAND0022", 7);
            Say(Who.Julian, "INSELSTRAND0022", 8, last: true);
        }
        else if (At(7, 2))
        {
            SetText(GoingUp, "");
            SetAfter(Vars.AnneIslandTalk, 2, null);
            SetAfter(Vars.GeorgeIslandTalk, 2, null);
            SetAfter(Vars.JulianIslandTalk, 2, null);
            Say(Who.Julian, "INSELSTRAND0025", 9);
            Say(Who.Dick, "INSELSTRAND0025", 10);
            Say(Who.Julian, "INSELSTRAND0025", 11);
            Say(Who.George, "INSELSTRAND0025", 12, last: true);
        }
        else if (Quest > 7)
            ChangeLocation("loc17");
    }

    /// <summary>Quest 6, step 3, before the wreck's cabin has been visited.</summary>
    bool BeforeTheCabin => At(6, 3) && Var(Vars.CabinVisited) == 0;

    /// <summary>
    /// Quest 6, step 3, back from the wreck: a remark that depends on whether the rope has been
    /// found — or, if the children chose the mainland first, <paramref name="mainland"/>.
    /// </summary>
    void AfterTheWreck(int speaker, string talk, (int First, int Then) withRope, (int First, int Then) withoutRope,
        Action mainland)
    {
        if (Var(Vars.WreckVisit) != 2)
            return;
        if (Var(Vars.ChoseMainland) != 0)
        {
            mainland();
            return;
        }
        var (first, then) = Has(Item.Rope) ? withRope : withoutRope;
        if (Var(talk) == 0)
            Say(speaker, "ALL0023", first, last: true);
        else if (Var(talk) == 1)
            Say(speaker, "ALL0023", then, last: true);
    }

    /// <summary>What Anne says at each story step.</summary>
    void TalkToAnne()
    {
        int talk = Var(Vars.AnneIslandTalk);
        if (BeforeTheCabin)
        {
            if (talk == 0)
                Say(Who.Dick, "INSELSTRAND0022", 2, last: true);
            else if (talk == 1)
                Say(Who.Anne, "INSELSTRAND0022", 3, last: true);
            else if (talk == 2)
                Say(Who.Anne, "INSELSTRAND0022", 9, last: true);
        }
        else if (At(6, 3))
            AfterTheWreck(Who.Anne, Vars.AnneIslandTalk, (1, 2), (7, 8), () => Say(Who.Anne, "ALL0023", 13, last: true));
        else if (At(6, 4))
        {
            if (talk == 0)
            {
                IncAfter(Vars.AnneWreckTalk, Chain);
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
        else if (At(7, 2))
        {
            if (talk == 0)
            {
                IncAfter(Vars.AnneIslandTalk, Chain);
                Say(Who.Anne, "INSELSTRAND0025", 1, last: true);
            }
            else if (talk == 1)
                Say(Who.Anne, "INSELSTRAND0025", 2, last: true);
            else if (talk == 2)
                Say(Who.Anne, "INSELSTRAND0025", 13, last: true);
        }
        else if (At(8, 2))
        {
            if (talk == 0)
            {
                IncAfter(Vars.AnneIslandTalk, Chain);
                Say(Who.Anne, "STRAND0037", 3);
                Say(Who.George, "STRAND0037", 4, last: true);
            }
            else if (talk == 1)
                Say(Who.Anne, "STRAND0037", 5, last: true);
        }
        else if (At(11, 1))
        {
            if (talk == 0)
            {
                IncAfter(Vars.AnneIslandTalk, Chain);
                Say(Who.Anne, "STRAND0052", 7, last: true);
            }
            else if (talk == 1)
                Say(Who.Anne, "STRAND0052", 8, last: true);
        }
    }

    /// <summary>What George says at each story step (on quest 8, Dick's remarks).</summary>
    void TalkToGeorge()
    {
        int talk = Var(Vars.GeorgeIslandTalk);
        if (BeforeTheCabin)
        {
            if (talk == 0)
                Say(Who.George, "INSELSTRAND0022", 4, last: true);
            else if (talk == 2)
                Say(Who.George, "INSELSTRAND0022", 10, last: true);
        }
        else if (At(6, 3))
        {
            AfterTheWreck(Who.George, Vars.GeorgeIslandTalk, (5, 6), (11, 12), () =>
            {
                Say(Who.George, "ALL0023", 15);
                Say(Who.Dick, "ALL0023", 16, last: true);
            });
        }
        else if (At(6, 4))
        {
            if (talk == 0)
            {
                IncAfter(Vars.GeorgeWreckTalk, Chain);
                IncAfter(Vars.GeorgeBeachTalk, null);
                IncAfter(Vars.GeorgeIslandTalk, null);
                TalkAboutTheCupboard();
            }
            else if (talk == 1)
                Say(Who.George, "ALL0024", 20, last: true);
        }
        else if (At(7, 2))
        {
            if (talk == 0)
            {
                IncAfter(Vars.GeorgeIslandTalk, Chain);
                Say(Who.George, "INSELSTRAND0025", 3);
                Say(Who.Dick, "INSELSTRAND0025", 4, last: true);
            }
            else if (talk == 1)
                Say(Who.George, "INSELSTRAND0025", 5, last: true);
            else if (talk == 2)
                Say(Who.George, "INSELSTRAND0025", 14, last: true);
        }
        else if (At(8, 2))
        {
            if (Var(Vars.DickIslandTalk) == 0)
            {
                IncAfter(Vars.DickIslandTalk, Chain);
                Say(Who.Dick, "STRAND0037", 2, last: true);
            }
            else if (Var(Vars.DickIslandTalk) == 1)
                Say(Who.Dick, "STRAND0037", 6, last: true);
        }
        else if (At(11, 1) && talk == 0)
        {
            IncAfter(Vars.GeorgeIslandTalk, Chain);
            Say(Who.George, "STRAND0052", 9, last: true);
        }
    }

    /// <summary>What Julian says at each story step.</summary>
    void TalkToJulian()
    {
        int talk = Var(Vars.JulianIslandTalk);
        if (BeforeTheCabin)
        {
            if (talk == 0)
                Say(Who.Julian, "INSELSTRAND0022", 5, last: true);
            else if (talk == 2)
                Say(Who.Julian, "INSELSTRAND0022", 11, last: true);
        }
        else if (At(6, 3))
            AfterTheWreck(Who.Julian, Vars.JulianIslandTalk, (3, 4), (9, 10), () => Say(Who.Julian, "ALL0023", 14, last: true));
        else if (At(6, 4))
        {
            if (talk == 0)
            {
                IncAfter(Vars.JulianWreckTalk, Chain);
                IncAfter(Vars.JulianBeachTalk, null);
                IncAfter(Vars.JulianIslandTalk, null);
                Say(Who.Julian, "ALL0024", 8, last: true);
            }
            else if (talk == 1)
                Say(Who.Julian, "ALL0024", 9, last: true);
        }
        else if (At(7, 2))
        {
            if (talk == 0)
            {
                IncAfter(Vars.JulianIslandTalk, Chain);
                Say(Who.Julian, "INSELSTRAND0025", 6);
                Say(Who.Dick, "INSELSTRAND0025", 7, last: true);
            }
            else if (talk == 1)
                Say(Who.Julian, "INSELSTRAND0025", 8, last: true);
            else if (talk == 2)
                Say(Who.Julian, "INSELSTRAND0025", 15, last: true);
        }
        else if (At(8, 2))
        {
            if (talk == 0)
            {
                IncAfter(Vars.JulianIslandTalk, Chain);
                Say(Who.Julian, "STRAND0037", 1, last: true);
            }
            else if (talk == 1)
            {
                IncAfter(Vars.JulianIslandTalk, Chain);
                Say(Who.Julian, "STRAND0037", 7, last: true);
            }
            else if (talk == 2)
                Say(Who.Julian, "STRAND0037", 8, last: true);
        }
        else if (At(11, 1) && talk == 0)
            Say(Who.Julian, "STRAND0052", 10, last: true);
    }

    /// <summary>Anne and Julian face down-right; the hero (under their own speaker code) down-left.</summary>
    protected override TalkPoses? PosesOf(int speaker)
    {
        if (speaker == Who.Anne)
            return new TalkPoses(Anne, SpeakDownRight, StandDownRight);
        if (speaker == Who.Julian)
            return new TalkPoses(Julian, SpeakDownRight, StandDownRight);
        if (speaker == HeroSpeaker)
            return new TalkPoses(Hero, SpeakDownLeft, StandDownLeft);
        return null;
    }

    /// <summary>The speaker takes their talking pose.</summary>
    /// <remarks>The original's FUN_0044a05e.</remarks>
    public override void LineStarting(GameWorld world, int speaker) => StartTalking();

    /// <summary>The speaker stands again after their line.</summary>
    /// <remarks>The original's FUN_0044a17b.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end) => StopTalking(end);

    /// <summary>
    /// The last speaker stands again; the first remark of each character on quest 6 counts as
    /// heard. After the talk on the way up (quest 6), everyone has said their piece.
    /// </summary>
    /// <remarks>The original's FUN_0044a298.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (Text(GoingUp) == "Exit")
        {
            SetVar(Vars.AnneIslandTalk, 2);
            SetVar(Vars.GeorgeIslandTalk, 2);
            SetVar(Vars.JulianIslandTalk, 2);
        }
        if (At(6, 3))
        {
            if (CurrentSpeaker != Who.George)
                AnneJulianOrDickStands(end);
            string? heard = CurrentSpeaker switch
            {
                Who.Anne => Vars.AnneIslandTalk,
                Who.Julian => Vars.JulianIslandTalk,
                Who.George => Vars.GeorgeIslandTalk,
                _ => null,
            };
            if (heard != null && Var(heard) == 0)
                SetVar(heard, 1);
        }
        else if (Quest == 6 && Episode >= 3)
            AnneJulianOrDickStands(end);
        else if (Quest < 7)
        {
            AnneJulianOrDickStands(end);
            if (CurrentSpeaker == Who.Dick && Var(Vars.AnneIslandTalk) == 0)
                SetVar(Vars.AnneIslandTalk, 1);
        }
        else
            StopTalking(end);
        SetText(GoingUp, "");
    }

    /// <summary>Anne, Julian or (as Dick) the hero stands again after the line.</summary>
    void AnneJulianOrDickStands(TaskHandle end)
    {
        if (CurrentSpeaker == Who.Anne)
            StateAfter(Anne, StandDownRight, end);
        else if (CurrentSpeaker == Who.Julian)
            StateAfter(Julian, StandDownRight, end);
        else if (CurrentSpeaker == Who.Dick)
            StateAfter(Hero, StandDownLeft, end);
    }
}
