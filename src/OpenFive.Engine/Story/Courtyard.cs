using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The castle courtyard on Kirrin Island by day (loc17). Quest 8: the Five arrive (step 2), Timmy
/// runs into a bush and falls down the well (steps 2-4), and they plan to move the big stone
/// (step 5). Quest 11: the professor telephones from the courtyard; the hero distracts him with
/// stones and calls for help on his radio.
/// </summary>
public sealed class Courtyard : StoryScript
{
    /// <inheritdoc/>
    public override string Location => "loc17";

    // ---- setting up ------------------------------------------------------------------------------

    /// <summary>The rope, the bush and the bucket as the story has them; then the hero and the others.</summary>
    /// <remarks>The original's FUN_0044fb11.</remarks>
    protected override void Setup()
    {
        SetVar(Vars.ChoppingBush, 0);
        SetVar(Vars.TyingRope, 0);
        SetVar(Vars.WellTalkReady, 0);
        SetVar(Vars.ProfessorX, 335);
        Obj("LOC17Anne", false);
        Obj("LOC17George", false);
        Obj("LOC17Julian", false);
        Obj("LOC17Stein", true);
        Obj("LOC17Steinhaufen", true);
        Obj("LOC17CloseUp", true);
        SetUpRope();
        Obj("LOC17BuschObj", false);
        if (Var(Vars.BushCut) == 0)
            Obj("LOC17BuschObj", true);
        SetUpBucket();
        ComeIn();
    }

    /// <summary>The rope tied to the stone by the well, while it is there.</summary>
    /// <remarks>The original's FUN_00451f0f.</remarks>
    void SetUpRope()
    {
        AnimPos("Seil", 498, 438);
        StopAnim("Seil");
        Depth(@"bmp\obj\locs\loc17\seil.brg", 4000);
        AnimScale("Seil", 0.5);
        if (Var(Vars.RopeOnStone) == 1)
            Anim("Seil", -1);
    }

    /// <summary>The bucket, until it has been taken.</summary>
    /// <remarks>The original's FUN_004522cb.</remarks>
    void SetUpBucket()
    {
        SetVar(Vars.TakingCourtyardBucket, 0);
        if (Var(Vars.CourtyardBucketTaken) == 0)
            Obj("LOC17Eimer", true);
        else if (Var(Vars.CourtyardBucketTaken) == 1)
            Obj("LOC17Eimer", false);
    }

    /// <summary>The hero comes in from where they came from; then the scene of the story step.</summary>
    /// <remarks>The original's FUN_0044fbe0.</remarks>
    void ComeIn()
    {
        switch (FromId)
        {
            case 14: // up from the island beach
                Place(Hero, 820, 450, StandDownRight);
                Show(Hero);
                if (!At(11, 1))
                    WalkAndFace(460, 585, StandUpLeft);
                else if (Var(Vars.CourtyardFree) < 2)
                    SetVar(Vars.CourtyardFree, 1);
                break;
            case 17:
                if (At(8, 2))
                    Place(Hero, 460, 585, StandUpLeft);
                else
                    Place(Hero, 633, 440, StandDownRight);
                Show(Hero);
                break;
            case 18: // out of the well
                Place(Hero, 450, 450, StandDownRight);
                Show(Hero);
                break;
            case 117: // the courtyard at night
                PointerOn();
                Place(Hero, 100, 500, StandDownRight);
                Show(Hero);
                break;
        }
        SetUpStoryStep();
        ShowParty();
        // Timmy is in the bush (step 2) or the well (steps 3-4) and away from quest 9 on.
        if (At(8, 2))
        {
            if (Var(Vars.BushState) > 0)
                Hide(Timmy);
        }
        else if (!At(8, 5) && Quest > 7)
            Hide(Timmy);
    }

    /// <summary>The scene of the story step.</summary>
    /// <remarks>The original's FUN_0044fdc9.</remarks>
    void SetUpStoryStep()
    {
        if (At(8, 2))
        {
            if (Var(Vars.CastleArrivalSeen) == 0)
            {
                PointerOff();
                SetVar(Vars.CastleArrivalSeen, 1);
                TheOthersArrive();
                SetText(StoryTexts.CourtyardScene, "start");
                Say(Who.Anne, "RUINE0038", 1, delay: 85);
                Say(Who.Dick, "RUINE0038", 2);
                Say(Who.Julian, "RUINE0038", 3);
                Say(Who.George, "RUINE0038", 4);
                Say(Who.Julian, "RUINE0038", 5);
                Say(Who.George, "RUINE0038", 6);
                Say(Who.Anne, "RUINE0038", 7);
                Say(Who.Dick, "RUINE0038", 8, last: true);
            }
            else
                OthersWhereTheyWere();
            Obj("LOC17Steinhaufen", false);
            Obj("LOC17CloseUp", false);
        }
        else if (At(8, 3))
        {
            OthersByTheWell();
            PointerOff();
            Chain = null;
            Say(Who.George, "RUINE0040", 1, last: true);
        }
        else if (At(8, 4))
        {
            OthersByTheWell();
            if (Var(Vars.TimmyInWellSceneSeen) != 0 || Has(Item.Bucket))
                SetVar(Vars.WellTalkReady, 1);
            else
            {
                SetText(StoryTexts.CourtyardScene, "start");
                PointerOff();
                Chain = null;
                IncAfter(Vars.TimmyInWellSceneSeen, null);
                Say(Who.George, "RUINE0042", 1);
                Say(Who.Dick, "RUINE0042", 2);
                Say(Who.Julian, "RUINE0042", 3, last: true);
            }
        }
        else if (At(8, 5))
        {
            OthersByTheWell();
            Place(Timmy, 480, 589, StandLeft);
            if (Var(Vars.StoneSceneSeen) == 0)
            {
                PointerOff();
                Chain = null;
                IncAfter(Vars.StoneSceneSeen, null);
                Say(Who.George, "RUINE0043", 1);
                Say(Who.Julian, "RUINE0043", 2);
                Say(Who.Dick, "RUINE0043", 3);
                Say(Who.George, "RUINE0043", 4, last: true);
            }
        }
        else if (At(11, 1))
            TheProfessorIsHere();
    }

    /// <summary>The others come into the courtyard after the hero (quest 8, step 2).</summary>
    /// <remarks>The original's FUN_00451ce3.</remarks>
    void TheOthersArrive()
    {
        Place(Julian, 850, 320, StandLeft);
        Place(Timmy, 880, 474, StandLeft);
        Place(Anne, 830, 456, StandLeft);
        Place(Dick, 840, 462, StandLeft);
        Walk(Timmy, 480, 589);
        State(Timmy, StandUpLeft);
        Walk(Anne, 424, 435);
        Chain = State(Anne, StandDownLeft);
        Walk(Dick, 272, 430);
        State(Dick, StandDownRight);
        Walk(Julian, 348, 432);
        State(Julian, StandDownRight);
    }

    /// <summary>
    /// Quest 8, step 2, coming back: with George as the hero the others are where they went to
    /// look around; otherwise they wait by the well.
    /// </summary>
    /// <remarks>The original's FUN_00451df7.</remarks>
    void OthersWhereTheyWere()
    {
        if (Hero != George)
        {
            OthersByTheWell();
            return;
        }
        Place(Timmy, 480, 589, StandLeft);
        Place(Anne, 830, 456, StandLeft);
        Place(Dick, 331, 363, StandDownRight);
        Place(Julian, 235, 370, StandDownRight);
        if (Var(Vars.JulianAtTheTower) == 1)
            Place(Julian, 173, 397, StandDownRight);
        if (Var(Vars.DickBehindTheHouse) == 1)
            Place(Dick, 368, 412, StandDownRight);
        if (Var(Vars.AnneAtTheGate) == 1)
            Place(Anne, 678, 458, StandDownLeft);
    }

    /// <summary>Julian, Anne and George wait by the well and can be talked to.</summary>
    /// <remarks>The original's FUN_00451ff9.</remarks>
    void OthersByTheWell()
    {
        Place(Julian, 570, 433, StandDownLeft);
        Place(Anne, 540, 433, StandDownLeft);
        Place(George, 476, 440, StandDownLeft);
        MoveObj("LOC17Julian", 560, 342);
        Obj("LOC17George", true);
        Obj("LOC17Anne", true);
        Obj("LOC17Julian", true);
    }

    /// <summary>
    /// Quest 11: the hero comes up out of the dungeon; the professor is on the telephone (or has
    /// gone, leaving his radio); the stairs may have fallen in; the rope lies by the stone.
    /// </summary>
    void TheProfessorIsHere()
    {
        PointerOff();
        Walk(Hero, 633, 440);
        var turned = State(Hero, StandDownLeft);
        PointerAfter(true, turned, 4, 400, 400);
        MoveObj("LOC17bigStone", -60, 400);
        Obj("LOC17ExitToLoc21", true);
        if (Var(Vars.CourtyardFree) < 2)
            SetAfter(Vars.CourtyardFree, 0, turned);
        SetUpProfessor();
        SetUpFallenStones();
        Obj("LOC17SeilClick", !Has(Item.Rope));
    }

    /// <remarks>The original's FUN_00450642.</remarks>
    void SetUpProfessor()
    {
        foreach (string anim in new[] { "ProfHangUp", "ProfTelefon", "Radio", "ProfStandLeft", "ProfSpeakLeft", "ProfPickUp", "ProfWalkRight" })
            StopAnim(anim);
        Depth(@"bmp\obj\prof\prra\prra03.brg", 2990);
        Depth(@"bmp\obj\prof\prsp\prsp01.brg", 2500);
        Depth(@"bmp\obj\prof\prwl\prwl05.brg", 2500);
        foreach (string anim in new[] { "ProfWalkRight", "ProfSpeakLeft", "ProfStandLeft", "ProfPickUp", "ProfHangUp", "ProfTelefon", "Radio" })
            AnimScale(anim, 0.5);
        AnimPos("ProfStandLeft", 350, 340);
        AnimPos("ProfSpeakLeft", 350, 340);
        AnimPos("ProfWalkRight", 335, 340);
        AnimPos("ProfPickUp", 320, 340);
        AnimPos("ProfHangUp", 320, 340);
        AnimPos("Radio", 320, 340);
        AnimPos("ProfTelefon", 320, 340);
        Anim(Var(Vars.CourtyardFree) < 2 ? "ProfTelefon" : "Radio", -1);
    }

    /// <summary>With the rope on the ivy, the stairs have been buried by falling stones.</summary>
    /// <remarks>The original's FUN_0045058c.</remarks>
    void SetUpFallenStones()
    {
        StopAnim("FallendeSteine");
        StopAnim("begrabendeSteine");
        Depth(@"bmp\obj\locs\loc17\fstein.brg", 2990);
        AnimPos("FallendeSteine", -100, -100);
        AnimPos("begrabendeSteine", -100, -100);
        if (Var(Vars.RopeOnIvy) == 1)
        {
            Anim("begrabendeSteine", -1);
            Obj("LOC17ExitToLoc21", false);
            Obj("LOC17Funkgeraet", true);
        }
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    // ---- clicks --------------------------------------------------------------------------------

    /// <summary>The ways out, the others, and the things in the courtyard.</summary>
    /// <remarks>The original's FUN_0044c983.</remarks>
    public override bool Click(string name)
    {
        switch (name.ToLowerInvariant())
        {
            case "loc17exittoloc14":
                WayDown();
                break;
            case "loc17seilclick":
                if (At(11, 1))
                    TakeBackTheRope();
                break;
            case "loc17george":
                TalkToGeorge();
                break;
            case "loc17anne":
                TalkToAnne();
                break;
            case "loc17julian":
                TalkToJulian();
                break;
            case "loc17tuerclick":
                TheTowerDoor();
                break;
            case "loc17efeu":
                TheIvy();
                break;
            case "loc17busch":
                TheBush();
                break;
            case "loc17zwischenturmundhaus":
                BetweenTowerAndHouse();
                break;
            case "loc17stein":
                Walk(Hero, 490, 458);
                Chain = State(Hero, StandUpRight);
                break;
            case "loc17funkgeraet":
                if (At(11, 1))
                    UseTheRadio();
                break;
            case "loc17bigstone":
                if (At(11, 1) && Var(Vars.RopeOnIvy) != 1)
                    Remark(1, 455, PushBoatRight, 13);
                break;
            case "loc17exittoloc21":
                if (At(11, 1))
                    Remark(100, 455, StandDownLeft, 18);
                break;
            case "loc17steinhaufen":
                TheStonePile();
                break;
            case "loc17closeup":
                Walk(Hero, 215, 476);
                Chain = State(Hero, StandUpLeft);
                break;
            case "loc17eimer":
            {
                Walk(Hero, 148, 420);
                var kneeling = State(Hero, KneelLeft);
                IncAfter(Vars.CourtyardBucketTaken, kneeling);
                SetVar(Vars.TakingCourtyardBucket, 1);
                ObjAfter("LOC17Eimer", false, kneeling, 5);
                break;
            }
        }
        return true;
    }

    /// <summary>Quest 11: the hero walks somewhere and Dick remarks on it (a line of "RUINE0053").</summary>
    void Remark(int x, int y, int pose, int line)
    {
        Walk(Hero, x, y);
        Chain = State(Hero, pose);
        PointerAfter(false, Chain, x: 400, y: 400);
        Say(Who.Dick, "RUINE0053", line, last: true, delay: 10);
    }

    /// <summary>
    /// Says lines of one scene that each start once <paramref name="after"/> is released, not one
    /// after the other (the original queues these conversations so); the last ends the conversation.
    /// </summary>
    void SayAllAfter(TaskHandle after, string scene, params (int Speaker, int Line)[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
            Say(lines[i].Speaker, scene, lines[i].Line, last: i == lines.Length - 1, after: after);
    }

    /// <summary>
    /// The way down to the island beach: on quest 8 someone stops the hero (Anne on step 2, George
    /// later); on quest 11 Dick will not leave while the professor is there.
    /// </summary>
    void WayDown()
    {
        if (At(8, 2))
        {
            Walk(Hero, 586, 490);
            Chain = State(Hero, StandUpRight);
            if (Var(Vars.AnneAtTheGate) == 0)
            {
                // Anne comes over to stop the hero.
                var turned = Chain;
                PointerAfter(false, turned, x: 300, y: 400);
                IncAfter(Vars.AnneAtTheGate, turned);
                PlaceAfter(Anne, 830, 456, StandDownRight, turned);
                WalkAfter(Anne, 678, 458, turned);
                Chain = State(Anne, StandDownLeft);
                Say(Who.Anne, "RUINE0038", 13, last: true, delay: 50);
            }
            else if (Var(Vars.AnneAtTheGate) == 1)
            {
                PointerAfter(false, Chain, x: 300, y: 400);
                Say(Who.Anne, "RUINE0038", 14, last: true);
            }
        }
        else if (At(8, 3))
            GeorgeStopsTheHero("RUINE0040", 9);
        else if (At(8, 4))
        {
            Walk(Hero, 586, 490);
            Chain = State(Hero, StandUpRight);
        }
        else if (At(8, 5))
            GeorgeStopsTheHero("RUINE0043", 5);
        else if (At(11, 1))
        {
            Walk(Hero, 586, 490);
            Chain = State(Hero, StandUpRight);
            if (Var(Vars.CourtyardFree) > 1)
            {
                PointerAfter(false, Chain, x: 400, y: 400);
                Say(Who.Dick, "RUINE0053", 12, delay: 10);
                Say(Who.Dick, "RUINE0053", 88, last: true);
            }
        }
        else
        {
            Walk(Hero, 850, 446);
            ChangeLocation("loc14");
        }
    }

    /// <summary>The hero walks to the way down and George says they cannot go yet.</summary>
    void GeorgeStopsTheHero(string scene, int line)
    {
        Walk(Hero, 586, 490);
        Chain = State(Hero, StandUpRight);
        PointerAfter(false, Chain, x: 400, y: 400);
        Say(Who.George, scene, line, last: true, delay: 10);
    }

    /// <summary>Quest 11: the hero unties the rope from the stone and takes it back.</summary>
    void TakeBackTheRope()
    {
        Walk(Hero, 565, 450);
        var kneeling = State(Hero, KneelLeft);
        PointerOff();
        PointerAfter(true, kneeling, 8, 400, 400);
        SetVar(Vars.RopeOnStone, 0);
        StopAnimAfter("Seil", kneeling, 6);
        Give(Item.Rope);
        Obj("LOC17SeilClick", false);
    }

    /// <summary>The hero walks up to the others by the well and turns to them; returns that turn.</summary>
    TaskHandle TurnToTheOthers()
    {
        WalkAfter(Hero, 429, 450, null, 5);
        return Chain = State(Hero, StandDownRight);
    }

    /// <summary>What George says on quest 8 (steps 2, 3 and 5).</summary>
    void TalkToGeorge()
    {
        int talk = Var(Vars.GeorgeCastleTalk);
        if (At(8, 2))
        {
            var turned = TurnToTheOthers();
            if (talk == 1)
            {
                SayAllAfter(turned, "RUINE0038", (Who.George, 25), (Who.Dick, 26));
                IncAfter(Vars.GeorgeCastleTalk, turned);
            }
            else if (talk == 2)
            {
                SayAllAfter(turned, "RUINE0038", (Who.George, 29));
                IncAfter(Vars.GeorgeCastleTalk, turned);
            }
            else if (talk == 3)
            {
                SayAllAfter(turned, "RUINE0038", (Who.George, 32));
                Sound(@"wav\timmy\timmy01.wav", turned, 8);
            }
        }
        else if (At(8, 3))
        {
            var turned = TurnToTheOthers();
            if (Var(Vars.RopeTiedOnce) == 0)
                RopeTalk(turned, Who.George, Vars.GeorgeCastleTalk, (15, 16), 19, 20);
            else if (Var(Vars.StonesAtBush) == 0)
                StoneTalk(turned, Who.George);
        }
        else if (At(8, 5))
        {
            var turned = TurnToTheOthers();
            if (talk == 0)
            {
                SayAllAfter(turned, "RUINE0043", (Who.George, 16), (Who.Dick, 17));
                IncAfter(Vars.GeorgeCastleTalk, turned);
            }
            else if (talk == 1)
                SayAllAfter(turned, "RUINE0043", (Who.George, 18), (Who.Dick, 19));
        }
    }

    /// <summary>What Anne says on quest 8; on step 5 she leads the way to the night scene.</summary>
    void TalkToAnne()
    {
        int talk = Var(Vars.AnneCastleTalk);
        if (At(8, 2))
        {
            var turned = TurnToTheOthers();
            if (talk == 1)
            {
                SayAllAfter(turned, "RUINE0038", (Who.Anne, 27));
                IncAfter(Vars.AnneCastleTalk, turned);
            }
            else if (talk == 2)
            {
                SayAllAfter(turned, "RUINE0038", (Who.Anne, 30));
                IncAfter(Vars.AnneCastleTalk, turned);
            }
            else if (talk == 3)
                SayAllAfter(turned, "RUINE0038", (Who.Anne, 33));
        }
        else if (At(8, 3))
        {
            var turned = TurnToTheOthers();
            if (Var(Vars.RopeTiedOnce) == 0)
                RopeTalk(turned, Who.Anne, Vars.AnneCastleTalk, (15, 16), 17, 18);
            else if (Var(Vars.StonesAtBush) == 0)
                StoneTalk(turned, Who.Anne);
        }
        else if (At(8, 5))
            ChangeLocation("loc17b");
    }

    /// <summary>What Julian says on quest 8 (steps 2, 3 and 5).</summary>
    void TalkToJulian()
    {
        int talk = Var(Vars.JulianCastleTalk);
        if (At(8, 2))
        {
            var turned = TurnToTheOthers();
            if (talk == 1)
            {
                SayAllAfter(turned, "RUINE0038", (Who.Julian, 28));
                IncAfter(Vars.JulianCastleTalk, turned);
            }
            else if (talk == 2)
            {
                SayAllAfter(turned, "RUINE0038", (Who.Julian, 31));
                IncAfter(Vars.JulianCastleTalk, turned);
            }
            else if (talk == 3)
            {
                SetText(StoryTexts.CourtyardScene, "exit");
                SayAllAfter(turned, "RUINE0038", (Who.Julian, 34));
            }
        }
        else if (At(8, 3))
        {
            var turned = TurnToTheOthers();
            if (Var(Vars.RopeTiedOnce) == 0)
            {
                if (talk == 0)
                {
                    SayAllAfter(turned, "RUINE0040", (Who.Julian, 15), (Who.Dick, 16));
                    IncAfter(Vars.JulianCastleTalk, turned);
                }
                else if (talk == 1)
                    SayAllAfter(turned, "RUINE0040", (Who.Julian, 21), (Who.Dick, 22), (Who.Julian, 23));
            }
            else if (Var(Vars.StonesAtBush) == 0)
                StoneTalk(turned, Who.Julian);
        }
        else if (At(8, 5))
        {
            var turned = TurnToTheOthers();
            if (talk == 0)
            {
                SayAllAfter(turned, "RUINE0043", (Who.Julian, 6), (Who.Dick, 7), (Who.Anne, 8), (Who.George, 9),
                    (Who.Julian, 10), (Who.Anne, 11));
                IncAfter(Vars.JulianCastleTalk, turned);
            }
            else if (talk == 1)
                SayAllAfter(turned, "RUINE0043", (Who.Julian, 12), (Who.Dick, 13));
        }
    }

    /// <summary>
    /// Quest 8, step 3, before the rope is tied: the first remark (with Dick's answer), a second,
    /// then a last one.
    /// </summary>
    void RopeTalk(TaskHandle turned, int speaker, string talk, (int First, int Answer) first, int second, int last)
    {
        if (Var(talk) == 0)
        {
            SayAllAfter(turned, "RUINE0040", (speaker, first.First), (Who.Dick, first.Answer));
            IncAfter(talk, turned);
        }
        else if (Var(talk) < 2)
        {
            SayAllAfter(turned, "RUINE0040", (speaker, second));
            IncAfter(talk, turned);
        }
        else if (Var(talk) == 2)
            SayAllAfter(turned, "RUINE0040", (speaker, last));
    }

    /// <summary>Quest 8, step 3, with the rope tied: the talk about trying a stone on the bush.</summary>
    void StoneTalk(TaskHandle turned, int speaker) =>
        SayAllAfter(turned, "RUINE0040", (speaker, 10), (Who.Julian, 11), (Who.Dick, 12), (Who.Anne, 13), (Who.Julian, 14));

    /// <summary>The tower door: on quest 8, step 2 Julian comes over (twice).</summary>
    void TheTowerDoor()
    {
        Walk(Hero, 232, 497);
        Chain = State(Hero, StandUpLeft);
        if (!At(8, 2))
            return;
        if (Var(Vars.JulianAtTheTower) == 0)
        {
            var turned = Chain;
            IncAfter(Vars.JulianAtTheTower, turned);
            PointerAfter(false, turned, x: 300, y: 400);
            PlaceAfter(Julian, 235, 370, StandDownRight, turned);
            WalkAfter(Julian, 173, 397, turned);
            Chain = State(Julian, StandDownRight);
            Say(Who.Julian, "RUINE0038", 9, last: true);
        }
        else if (Var(Vars.JulianAtTheTower) == 1)
        {
            PointerAfter(false, Chain, x: 300, y: 400);
            Say(Who.Julian, "RUINE0038", 10, last: true);
        }
    }

    /// <summary>Between the tower and the house: on quest 8, step 2 Dick comes out (twice).</summary>
    void BetweenTowerAndHouse()
    {
        Walk(Hero, 368, 470);
        Chain = State(Hero, StandUpLeft);
        if (!At(8, 2))
            return;
        if (Var(Vars.DickBehindTheHouse) == 0)
        {
            var turned = Chain;
            IncAfter(Vars.DickBehindTheHouse, turned);
            PointerAfter(false, turned, x: 300, y: 400);
            PlaceAfter(Dick, 331, 363, StandDownRight, turned);
            WalkAfter(Dick, 368, 412, turned);
            Chain = State(Dick, StandDownRight);
            Say(Who.Dick, "RUINE0038", 11, last: true, delay: 20);
        }
        else if (Var(Vars.DickBehindTheHouse) == 1)
        {
            PointerAfter(false, Chain, x: 300, y: 400);
            Say(Who.Dick, "RUINE0038", 12, last: true);
        }
    }

    /// <summary>The ivy on the wall: the hero gets a fright (George especially); on quest 11 Dick remarks on it.</summary>
    void TheIvy()
    {
        Walk(Hero, 64, 419);
        var reached = State(Hero, TakeMidLeft);
        Sound(@"wav\fx\raschel.wav", reached, 3);
        Sound(@"wav\fx\mauerrie.wav", reached, 8);
        if (Hero == George)
            reached = State(Hero, FrightenedDownRight);
        Chain = reached;
        if (At(8, 2))
            Say(HeroSpeaker, "RUINE0038", 15, last: true, delay: 15);
        else if (At(11, 1))
            Say(Who.Dick, "RUINE0053", 14, last: true);
    }

    /// <summary>
    /// The bush by the well. Quest 8, step 2: Timmy runs into it the first time; once cut down,
    /// everyone goes to the well. Step 4: back down the well (with the bucket, after a word).
    /// </summary>
    void TheBush()
    {
        if (At(8, 2))
        {
            switch (Var(Vars.BushState))
            {
                case 0:
                    TimmyRunsIntoTheBush();
                    break;
                case 2:
                    StateAfter(Anne, StandUpLeft, null);
                    StateAfter(Julian, StandUpLeft, null);
                    StateAfter(George, StandUpLeft, null);
                    StateAfter(Hero, StandUpRight, null);
                    PointerAfter(false, null, x: 400, y: 400);
                    ChangeLocation("loc18", null, 2);
                    break;
                default:
                    WalkAndFace(431, 442, StandUpRight);
                    break;
            }
        }
        else if (At(8, 4))
        {
            Walk(Hero, 431, 442);
            var turned = Chain = State(Hero, StandUpRight);
            if (Has(Item.Bucket))
            {
                SetText(StoryTexts.CourtyardScene, "wiederhinein");
                SayAllAfter(turned, "RUINE0042", (Who.Dick, 8), (Who.George, 9));
            }
            else
                EveryoneToTheWell(turned);
        }
        else if (At(8, 5))
            GeorgeStopsTheHero("RUINE0043", 5);
        else if (At(11, 1))
        {
            Walk(Hero, 431, 442);
            Chain = State(Hero, StandUpRight);
            if (Var(Vars.CourtyardFree) > 1)
            {
                PointerAfter(false, Chain, x: 400, y: 400);
                Say(Who.Dick, "RUINE0053", 11, last: true, delay: 10);
            }
        }
        else
        {
            var turned = WalkAndFace(431, 442, StandUpRight);
            if (Var(Vars.RopeTiedOnce) > 0)
                EveryoneToTheWell(turned);
        }
    }

    /// <summary>The others turn to the well and they all go down to look into it.</summary>
    void EveryoneToTheWell(TaskHandle after)
    {
        StateAfter(Anne, StandUpLeft, after);
        StateAfter(Julian, StandUpLeft, after);
        StateAfter(George, StandUpLeft, after);
        PointerAfter(false, after, x: 400, y: 400);
        ChangeLocation("loc18", after, 2);
    }

    /// <summary>Timmy runs into the bush and disappears; George calls after him (the bush can now be cut).</summary>
    void TimmyRunsIntoTheBush()
    {
        PointerOff();
        SetText(StoryTexts.CourtyardScene, "Busch");
        Walk(Timmy, 456, 433);
        WalkAfter(George, 476, 450, null, 5);
        var timmy = State(Timmy, StandLeft);
        Sound(@"wav\fx\raschel.wav", timmy);
        Sound(@"wav\timmy\schmerz.wav", timmy, 10);
        HideAfter(Timmy, timmy);
        PointerAfter(false, timmy, x: 300, y: 400);
        Chain = State(Hero, StandUpRight);
        IncAfter(Vars.BushState, Chain);
        SetAfter(Vars.DickBehindTheHouse, 2, Chain);
        SetAfter(Vars.AnneAtTheGate, 2, Chain);
        SetAfter(Vars.JulianAtTheTower, 2, Chain);
        Say(Who.George, "RUINE0038", 16, delay: 10);
        Say(Who.George, "RUINE0038", 17, last: true);
    }

    /// <summary>Quest 11: the professor's radio; Dick calls the police (then home to the kitchen, quest 12).</summary>
    void UseTheRadio()
    {
        Walk(Hero, 310, 430);
        Chain = State(Hero, PhoneRight);
        StopAnimAfter("Radio", Chain, 1);
        PointerAfter(false, Chain, x: 400, y: 400);
        Say(Who.Dick, "RUINE0053", 17, delay: 14);
        Say(Who.Dick, "RUINE0053", 20, last: true);
    }

    /// <summary>
    /// The pile of stones: the hero picks one up (three in all). On quest 8, step 3 Dick says
    /// something about each; on quest 11 the professor may notice (see <see cref="EveryTick"/>).
    /// </summary>
    void TheStonePile()
    {
        if (At(11, 1) && Var(Vars.CourtyardFree) < 2)
            SetVar(Vars.CourtyardFree, 1);
        Walk(Hero, 633, 436);
        var reached = Chain = State(Hero, Var(Vars.StonesTaken) < 3 ? KneelLeft : StandDownLeft);
        if (At(8, 3))
        {
            int taken = Var(Vars.StonesTaken);
            if (taken == 2)
                IncAfter(Vars.StonesTaken, reached);
            if (taken is >= 0 and <= 3)
            {
                PointerAfter(false, reached, x: 300, y: 400);
                Say(Who.Dick, "RUINE0040", 3 + taken, last: true, delay: 14);
            }
            if (taken is 0 or 1)
                IncAfter(Vars.StonesTaken, reached);
        }
        else if (Quest == 8 && Episode >= 4)
            IncAfter(Vars.StonesTaken, reached);
        else if (Quest > 8)
        {
            if (At(11, 1) && Var(Vars.CourtyardFree) < 2)
                SetAfter(Vars.CourtyardFree, 0, reached);
            IncAfter(Vars.StonesTaken, reached);
        }
    }

    // ---- every tick --------------------------------------------------------------------------

    /// <summary>
    /// Waits for actions to finish: after chopping at the bush Dick remarks; once the rope is tied
    /// it is shown; by the well the talk starts once the hero moves; on quest 11 the hero is held
    /// back while the professor watches, and the professor walks off. The stones picked up are
    /// kept in the rucksack as one item (the first, second or third stone).
    /// </summary>
    /// <remarks>The original's FUN_00450800.</remarks>
    public override void EveryTick()
    {
        if (At(8, 2))
        {
            if (Hero.State == AxeRight)
                SetVar(Vars.ChoppingBush, 1);
            else if (Var(Vars.ChoppingBush) == 0 || IsBusy(Dick))
            {
                if (Anne.X == 740)
                    Place(Anne, 740, 435, Anne.State);
            }
            else
            {
                SetVar(Vars.ChoppingBush, 0);
                Chain = null;
                Say(Who.Dick, "RUINE0038", 24, last: true);
            }
        }
        else if (At(8, 3))
        {
            if (Hero.State == TieRight)
                SetVar(Vars.TyingRope, 1);
            else if (Var(Vars.TyingRope) != 0 && !IsBusy(Hero))
            {
                SetVar(Vars.TyingRope, 0);
                Anim("Seil", -1);
            }
        }
        else if (At(8, 4))
        {
            if (IsBusy(Hero) && Var(Vars.WellTalkDone) == 0 && Var(Vars.WellTalkReady) != 0)
            {
                if (Var(Vars.TakingCourtyardBucket) != 0)
                    return;
                TalkAboutTheWell();
            }
        }
        else if (At(11, 1))
        {
            if (IsBusy(Hero) && Var(Vars.CourtyardFree) == 0)
                HeldBackByTheProfessor();
            else if (IsAnimShown("ProfWalkRight"))
            {
                SetVar(Vars.ProfessorX, Var(Vars.ProfessorX) + 2);
                if (Var(Vars.ProfessorX) > 419)
                {
                    StopAnim("ProfWalkRight");
                    return;
                }
                AnimPos("ProfWalkRight", Var(Vars.ProfessorX), 340);
            }
        }
        KeepTheStonesInTheRucksack();
    }

    /// <summary>Quest 8, step 4: Anne and George talk about Timmy in the well; Dick (with or without the bucket) answers.</summary>
    void TalkAboutTheWell()
    {
        SetVar(Vars.WellTalkDone, 1);
        Chain = null;
        Say(Who.Anne, "RUINE0042", 4);
        Say(Who.George, "RUINE0042", 5);
        Hero.State = FacingGeorge;
        Say(Who.Dick, "RUINE0042", Has(Item.Bucket) ? 7 : 6, last: true);
    }

    /// <summary>Quest 11: the professor would see the hero: they are put back, and Dick says so.</summary>
    void HeldBackByTheProfessor()
    {
        ClearQueue(Hero);
        Place(Hero, 633, 440, StandDownLeft);
        Chain = null;
        int talk = Var(Vars.HidingFromProfessorTalk);
        if (talk is < 0 or > 3)
            return;
        if (talk < 3)
            IncAfter(Vars.HidingFromProfessorTalk, null);
        Say(Who.Dick, "RUINE0053", 1 + talk, last: true);
    }

    /// <summary>The stone item matches the number of stones taken from the pile (until the third).</summary>
    void KeepTheStonesInTheRucksack()
    {
        if (Has(Item.Stone3))
            return;
        int taken = Var(Vars.StonesTaken);
        if (taken == 1)
        {
            if (!Has(Item.Stone1))
                Give(Item.Stone1);
        }
        else if (taken == 2)
        {
            Take(Item.Stone1);
            Give(Item.Stone2);
        }
        else if (taken > 2)
        {
            Take(Item.Stone2);
            Give(Item.Stone3);
        }
    }

    // ---- talking ---------------------------------------------------------------------------------

    /// <summary>
    /// The usual poses: Anne, Julian and George talk facing down-left, Dick (the hero) down-right.
    /// With George as the hero (quest 8, step 2), Julian and Dick face down-right.
    /// </summary>
    protected override TalkPoses? PosesOf(int speaker)
    {
        if (Hero == George)
        {
            return speaker switch
            {
                Who.Anne => new TalkPoses(Anne, SpeakDownLeft, StandDownLeft),
                Who.Julian => new TalkPoses(Julian, SpeakDownRight, StandDownRight),
                Who.Dick => new TalkPoses(Dick, SpeakDownRight, StandDownRight),
                _ => null,
            };
        }
        return UsualPoses(speaker);
    }

    TalkPoses? UsualPoses(int speaker) => speaker switch
    {
        Who.Anne => new TalkPoses(Anne, SpeakDownLeft, StandDownLeft),
        Who.Julian => new TalkPoses(Julian, SpeakDownLeft, StandDownLeft),
        Who.George => new TalkPoses(George, SpeakDownLeft, StandDownLeft),
        Who.Dick => new TalkPoses(Hero, SpeakDownRight, StandDownRight),
        _ => null,
    };

    /// <summary>
    /// The speaker takes their talking pose. On quest 11 the professor switches animations, and
    /// Dick on the radio talks into it (remembered as "Kueche": help is called).
    /// </summary>
    /// <remarks>The original's FUN_00450fd2.</remarks>
    public override void LineStarting(GameWorld world, int speaker)
    {
        if (At(11, 1))
        {
            if (CurrentSpeaker == Who.Professor)
            {
                if (Var(Vars.CourtyardFree) < 2)
                {
                    StopAnimAfter("ProfStandLeft", null);
                    Anim("ProfSpeakLeft", -1);
                }
            }
            else if (CurrentSpeaker == Who.Dick)
            {
                if (Hero.State == PhoneStillRight)
                {
                    StateAfter(Hero, PhoneTalkRight, null);
                    SetText(StoryTexts.CourtyardScene, "Kueche");
                }
                else if (Var(Vars.CourtyardFree) > 1)
                    StateAfter(Hero, SpeakDownLeft, null);
            }
            return;
        }
        if (Hero == George && Dick.State == SpeakDownRight)
            Dick.State = StandDownRight;
        StartTalking();
    }

    /// <summary>The speaker stands again after their line (not on quest 11).</summary>
    /// <remarks>The original's FUN_0045121d.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end)
    {
        if (!At(11, 1))
            StopTalking(end);
    }

    /// <summary>What follows a conversation, for each story step.</summary>
    /// <remarks>The original's FUN_004513a9.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (At(11, 1))
            AfterTalkOnQuest11(end);
        else if (At(8, 2))
            AfterTalkOnStep2(end);
        else if (At(8, 3) || At(8, 5))
        {
            StandAfterLine(end);
            SetText(StoryTexts.CourtyardScene, "");
        }
        else if (At(8, 4))
        {
            StandAfterLine(end);
            string scene = Text(StoryTexts.CourtyardScene);
            if (scene == "wiederhinein")
            {
                PointerAfter(false, end, x: 400, y: 400);
                ChangeLocation("loc18", end, 2);
            }
            else if (scene == "start")
                ToggleAfter(Vars.WellTalkReady, end);
            SetText(StoryTexts.CourtyardScene, "");
        }
    }

    /// <summary>The last speaker stands again (the usual poses).</summary>
    void StandAfterLine(TaskHandle end)
    {
        if (UsualPoses(CurrentSpeaker) is { } p)
            StateAfter(p.Actor, p.Standing, end);
    }

    /// <summary>
    /// Quest 11: the professor picks up the telephone again; after Dick's call for help, quest 12
    /// begins in the kitchen; with the rope on the ivy, down into the courtyard at night.
    /// </summary>
    void AfterTalkOnQuest11(TaskHandle end)
    {
        if (CurrentSpeaker == Who.Professor)
        {
            if (Var(Vars.CourtyardFree) < 2)
            {
                StopAnimAfter("ProfSpeakLeft", end);
                StopAnimAfter("Radio", end);
                var pickedUp = Anim("ProfPickUp", after: end);
                Anim("ProfTelefon", -1, pickedUp);
            }
        }
        else if (CurrentSpeaker == Who.Dick)
        {
            if (Text(StoryTexts.CourtyardScene) != "Kueche")
            {
                if (Var(Vars.CourtyardFree) > 1)
                    StateAfter(Hero, StandDownLeft, end);
            }
            else
            {
                StateAfter(Hero, PhoneStillRight, end);
                PointerAfter(false, end, x: 400, y: 400);
                ChangeLocation("loc05", end);
                SetAfter(Vars.EpisodeNumber, 1, end);
                SetAfter(Vars.QuestNumber, 12, end);
            }
            if (Text(StoryTexts.CourtyardScene) == "EfeuMitSeil")
            {
                PointerAfter(false, end, x: 400, y: 400);
                ChangeLocation("loc17b", end, 6);
                SetText(StoryTexts.CourtyardScene, "");
            }
        }
    }

    /// <summary>
    /// Quest 8, step 2: after the arrival everyone spreads out to look around; after Timmy runs
    /// into the bush Dick comes over (and the bush menu follows); after chopping the hero turns.
    /// </summary>
    void AfterTalkOnStep2(TaskHandle end)
    {
        switch (Text(StoryTexts.CourtyardScene))
        {
            case "start":
                SpreadOut(end);
                break;
            case "Busch":
                DickComesOver(end);
                break;
            case "fr":
                SetText(StoryTexts.CourtyardScene, "");
                PointerAfter(false, end, x: 400, y: 400);
                Ask(Who.DickAlt, "RUINE0038", 90, after: end);
                break;
            case "Axt":
                SetText(StoryTexts.CourtyardScene, "");
                StateAfter(Hero, StandDownRight, end);
                return;
        }
        StopTalking(end);
        if (Text(StoryTexts.CourtyardScene) != "fr")
            SetText(StoryTexts.CourtyardScene, "");
    }

    /// <summary>Dick goes behind the house, Julian to the tower, Anne to the gate.</summary>
    void SpreadOut(TaskHandle end)
    {
        PointerAfter(false, end, x: 300, y: 400);
        WalkAfter(Dick, 329, 362, end);
        var dickThere = State(Dick, StandDownRight);
        PointerAfter(true, dickThere, x: 300, y: 400);
        WalkAfter(Julian, 235, 370, end, 6);
        State(Julian, StandDownRight);
        WalkAfter(Anne, 830, 456, end);
        State(Anne, StandDownRight);
    }

    /// <summary>Dick comes over to the bush and says what to do; Julian and Anne come too.</summary>
    void DickComesOver(TaskHandle end)
    {
        PointerAfter(false, end, x: 300, y: 400);
        if (Var(Vars.DickBehindTheHouse) != 0)
            PlaceAfter(Dick, 331, 363, StandDownRight, end);
        WalkAfter(Dick, 431, 442, end, 10);
        var arrived = State(Dick, StandDownRight);
        Say(Who.Dick, "RUINE0038", 18, last: true, after: arrived, delay: 20);
        SetText(StoryTexts.CourtyardScene, "fr");
        WalkAfter(Julian, 520, 433, end);
        State(Julian, StandUpLeft);
        WalkAfter(Anne, 540, 433, end);
        State(Anne, StandUpLeft);
        Obj("LOC17Anne", true);
        Obj("LOC17George", true);
        Obj("LOC17Julian", true);
    }

    /// <summary>
    /// The bush menu (quest 8, step 2): option 1 leaves it to Dick (the menu closes and Dick
    /// becomes the hero); option 2, George and Dick talk. The menu comes back while options remain.
    /// </summary>
    /// <remarks>The original's FUN_004520a4.</remarks>
    protected override void AfterChoice(string scene, int line, int option)
    {
        if (Var(ChoicePending) == 0)
            return;
        if (option == 1)
        {
            // Every option off the menu (in the original's order: the third first).
            SetVar(Vars.V64efac, Var(Vars.V64efb4));
            SetVar(Vars.V64efa8, Var(Vars.V64efb4));
            SetVar(Vars.V64efa4, Var(Vars.V64efb4));
            Hero = Dick;
            PointerAfter(true, Chain, 1, 400, 400);
        }
        else if (option == 2)
        {
            StopAnimAfter("DickHeadAnimStill", Chain);
            Say(Who.George, "RUINE0038", 21);
            Say(Who.Dick, "RUINE0038", 22);
            Say(Who.George, "RUINE0038", 23, last: true);
        }
        RemoveChosenOption();
        if (!MenuExhausted())
            Ask(Who.DickAlt, "RUINE0038", 90);
        SetVar(ChoicePending, 0);
    }

    /// <summary>Taking the bucket (its object switched off) puts it into the rucksack.</summary>
    /// <remarks>The original's FUN_0045232e.</remarks>
    public override void ObjectChanged(GameWorld world, string name, bool on)
    {
        if (!on && Same(name, "LOC17Eimer") && Var(Vars.TakingCourtyardBucket) != 0)
        {
            SetVar(Vars.TakingCourtyardBucket, 0);
            Give(Item.Bucket);
            SetVar(Vars.CourtyardBucketTaken, 1);
        }
    }
}
