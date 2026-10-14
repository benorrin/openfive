using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The beach below Kirrin Cottage (loc09): Alf mending his boat, the fisherman's hut, the rowing
/// boat to the island, and (behind a crate) the way into the hideout. Quest 2: footprints and the
/// crate. Quest 6: the fisherman's questions; then off to the island. Quests 7-8: the others wait here.
/// </summary>
public sealed class Shore : IslandScript
{
    /// <summary>The conversation running: "Boot" (the boat), "Fischer" (the fisherman), "heroWechsel" (George takes over).</summary>
    const string BeachScene = "t63be28";

    /// <summary>Things that are only looked at: where the hero stands, which way they face, and the flag it sets (if any).</summary>
    static readonly Dictionary<string, (int X, int Y, int Facing, string? Seen)> LookAt = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LOC09KistenLinks"] = (637, 383, StandUpRight, null),
        ["LOC09KistenMitte"] = (660, 398, StandUpRight, null),
        ["LOC09Huette"] = (320, 380, StandUpRight, Vars.HutSeen),
        ["LOC09Window"] = (320, 380, StandUpRight, Vars.HutWindowSeen),
        ["LOC09Bank"] = (320, 380, StandUpRight, Vars.BenchSeen),
        ["LOC09Netz"] = (320, 380, StandUpLeft, Vars.NetSeen),
        ["LOC09Meer"] = (188, 420, StandUpLeft, Vars.SeaSeen),
        ["LOC09Felseninsel"] = (188, 420, StandUpLeft, Vars.IslandSeen),
    };

    /// <summary>Where the hero comes in from each neighbouring location, and which way they turn (null: they do not).</summary>
    static readonly Dictionary<int, (int X, int Y, int? Facing)> Entrances = new()
    {
        [5] = (602, 273, StandDownLeft),
        [6] = (602, 273, StandDownRight),
        [8] = (636, 404, null),
        [13] = (200, 404, StandDownRight),
    };

    /// <inheritdoc/>
    public override string Location => "loc09";

    // ---- setting up ------------------------------------------------------------------------------

    /// <summary>The crate, the others, the fisherman, Alf, the boat and the footprints, as the story has them.</summary>
    /// <remarks>The original's FUN_00444e95.</remarks>
    protected override void Setup()
    {
        Obj("LOC09KisteRechtsClick", false);
        Obj("LOC09ExitToLOC08", false);
        if (FromId == 8)
        {
            foreach (var a in new[] { Hero, Dick, Anne, Julian, George, Timmy })
                ClearQueue(a);
        }
        Obj("LOC09Anne", false);
        Obj("LOC09Dick", false);
        Obj("LOC09Julian", false);
        Obj("LOC09KisteRechts", false);
        SetVar(Vars.CrateX, 686);
        SetVar(Vars.BoatX, 0);
        SetVar(Vars.CrateJustPushed, 0);
        SetText(BeachScene, "");
        ComeIn();
        ShowParty();
        SetUpFisherman();
        SetUpAlf();
        SetUpBoat();
        SetUpFootprints();
        AnimPos("loc09DoBirds", 160, 119);
        AnimScale("loc09DoBirds", 1.0);
        Anim("loc09DoBirds", -1);
        Depth(@"bmp\obj\locs\loc09\tuer.brg", 3000);
        AnimPos("CloseDoor", 505, 267);
        AnimScale("CloseDoor", 1.0);
        Anim("CloseDoor", -1);
        if (Quest > 2)
            Obj("LOC09ExitToLOC08", true);
    }

    /// <summary>The hero comes in from where they came from; then the scene of the story step.</summary>
    /// <remarks>The original's FUN_004455fb.</remarks>
    void ComeIn()
    {
        if (Entrances.TryGetValue(FromId, out var entrance))
        {
            Place(Hero, entrance.X, entrance.Y, StandDownRight);
            Show(Hero);
            Walk(Hero, 400, 426);
            if (entrance.Facing is { } facing)
                State(Hero, facing);
            if (FromId == 5)
            {
                SetVar(Vars.FishermanTalk, 1);
                SetVar(Vars.BeachBoat, 3);
            }
        }
        else
        {
            Place(Hero, 400, 426, StandDownRight);
            Show(Hero);
            State(Hero, StandDownRight);
        }
        SetUpStoryStep();
    }

    /// <summary>The crate (quest 2), and where the others are on quests 6-8.</summary>
    /// <remarks>The original's FUN_004457e1.</remarks>
    void SetUpStoryStep()
    {
        if (At(2, 2))
        {
            SetStep(2, 3);
            Obj("LOC09KisteRechts", true);
            if (Var(Vars.V64f4fc) > 0)
                SetVar(Vars.V64f4fc, 3);
            SetVar(Vars.FannyTalk, 0);
            SetVar(Vars.JulianTalk, 0);
            SetVar(Vars.AnneTalk, 0);
        }
        else if (At(2, 3))
        {
            if (Var(Vars.CratePushed) == 0)
            {
                Obj("LOC09KisteRechts", true);
                // Once the footprints have been seen, the crate can be pushed.
                if (Var(Vars.FootprintsFound) > 0)
                    Obj("LOC09KisteRechtsClick", true);
            }
        }
        else if (At(6, 2))
            OthersOnTheBeach(Dick, alfTalk: 4, resetFisherman: false);
        else if (At(7, 2) || (Quest == 6 && Episode >= 3))
            OthersOnTheBeach(George, alfTalk: 4, resetFisherman: true);
        else if (Episode < 3 && Quest == 8)
            BackFromTheHideout();
        if (Var(Vars.CratePushed) > 0)
            Obj("LOC09ExitToLOC08", true);
    }

    /// <summary>Anne, Julian, Timmy and <paramref name="fourth"/> (Dick or George) wait on the beach and can be talked to.</summary>
    void OthersOnTheBeach(Actor fourth, int alfTalk, bool resetFisherman)
    {
        Place(Julian, 250, 300, StandDownRight);
        Place(Anne, 150, 200, StandDownRight);
        Place(fourth, 300, 375, StandDownLeft);
        Place(Timmy, 580, 506, StandDownLeft);
        SetVar(Vars.AlfTalk, alfTalk);
        if (resetFisherman)
            SetVar(Vars.FishermanTalk, 0);
        Obj("LOC09Anne", true);
        Obj("LOC09Dick", true);
        Obj("LOC09Julian", true);
    }

    /// <summary>
    /// Quest 8: the Five come down from the hideout (or the garden) and talk about Julian's
    /// question; the story moves to step 2.
    /// </summary>
    void BackFromTheHideout()
    {
        if (FromId == 6)
            SetVar(Vars.HideoutLeaving, 1);
        if (Var(Vars.HideoutLeaving) <= 0)
            return;
        Episode = 2;
        OthersOnTheBeach(Dick, alfTalk: 9, resetFisherman: false);
        Chain = State(Hero, StandUpLeft);
        if (Var(Vars.HideoutLeaving) == 1 && Var(Vars.HideoutAnswer) != 0)
        {
            PointerOff();
            if (Var(Vars.HideoutAnswer) < 2)
            {
                Say(Who.Julian, "STRAND0036", 1);
                Say(Who.George, "STRAND0036", 2, last: true);
            }
            else if (Var(Vars.HideoutAnswer) == 2)
            {
                Say(Who.Julian, "STRAND0036", 1);
                Say(Who.George, "STRAND0036", 3);
                Say(Who.Julian, "STRAND0036", 4, last: true);
            }
        }
        SetVar(Vars.HideoutLeaving, 2);
    }

    /// <summary>The fisherman by his hut, once he has been met.</summary>
    /// <remarks>The original's FUN_0044506c.</remarks>
    void SetUpFisherman()
    {
        StopAnim("FischerStand");
        Obj("LOC09Fischer", false);
        Depth(@"bmp\obj\fischer\fisp\fisp06an.brg", 300);
        if (Var(Vars.FishermanTalk) <= 0)
            return;
        foreach (string anim in new[] { "FischerStand", "FischerStandSpeak" })
        {
            AnimPos(anim, 20, 330);
            AnimScale(anim, 1.0);
        }
        Anim("FischerStand", -1);
        Obj("LOC09Fischer", true);
    }

    /// <summary>Alf: sanding his boat (quests 1-2), or with the radio (quest 8).</summary>
    /// <remarks>The original's FUN_0044512a.</remarks>
    void SetUpAlf()
    {
        foreach (string anim in new[] { "AlfmitFunkSpeak", "AlfmitFunk", "AlfSchleife", "AlfSchleifeArm", "AlfSchleifeSpeak", "AlfSchleifeArmSpeak" })
            StopAnim(anim);
        Depth(@"bmp\obj\alf\alph\alph01.brg", 1010);
        Depth(@"bmp\obj\alf\alph\alpa01.brg", 900);
        Depth(@"bmp\obj\alf\alra\alra.brg", 1200);
        Obj("LOC09Alf", false);
        if (Quest < 3)
        {
            AnimPos("AlfSchleife", 90, 300);
            AnimScale("AlfSchleife", 1.0);
            Anim("AlfSchleife", -1);
            Sound(@"wav\fx\schmierg.wav");
            AnimPos("AlfSchleifeArm", 83, 315);
            AnimScale("AlfSchleifeArm", 1.0);
            Anim("AlfSchleifeArm", -1);
            AnimPos("AlfSchleifeSpeak", 90, 300);
            AnimScale("AlfSchleifeSpeak", 1.0);
            AnimPos("AlfSchleifeArmSpeak", 83, 315);
            AnimScale("AlfSchleifeArmSpeak", 1.0);
            Obj("LOC09Alf", true);
        }
        else if (Var(Vars.AlfTalk) == 9)
        {
            Obj("LOC09Alf", true);
            MoveObj("LOC09Alf", 410, 290);
            foreach (string anim in new[] { "AlfmitFunk", "AlfmitFunkSpeak" })
            {
                AnimPos(anim, 400, 290);
                AnimScale(anim, 0.4);
            }
            Anim("AlfmitFunk", -1);
        }
    }

    /// <summary>The rowing boat on the sand (not while it is out, state 2).</summary>
    /// <remarks>The original's FUN_0044549a.</remarks>
    void SetUpBoat()
    {
        AnimPos("Boot", 0, 335);
        StopAnim("Boot");
        Depth(@"bmp\obj\locs\loc09\boot.brg", 1000);
        if (Var(Vars.BeachBoat) is 0 or 1 or 3 or 4 or 5)
            Anim("Boot", -1);
    }

    /// <summary>The footprints, until they have been examined.</summary>
    /// <remarks>The original's FUN_00445333.</remarks>
    void SetUpFootprints()
    {
        SetVar(Vars.ExaminingFootprints, 0);
        Obj("LOC09Fusspuren", false);
        if (Var(Vars.FootprintsFound) == 0)
            Obj("LOC09Fusspuren", true);
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    // ---- clicks --------------------------------------------------------------------------------

    /// <summary>The ways out, the crate and footprints, the boat, Alf, the others and the fisherman.</summary>
    /// <remarks>The original's FUN_00441d3e.</remarks>
    public override bool Click(string name)
    {
        if (LookAt.TryGetValue(name, out var spot))
        {
            WalkAndFace(spot.X, spot.Y, spot.Facing);
            if (spot.Seen != null && Var(spot.Seen) == 0)
                SetVar(spot.Seen, 1);
        }
        else if (Same(name, "LOC09ExitToLOC08"))
        {
            TaskHandle? reached = null;
            if (Var(Vars.CrateJustPushed) == 0)
                reached = WalkAndFace(784, 428, StandUpRight);
            else
                SetVar(Vars.CrateJustPushed, 0);
            ChangeLocation("loc08", reached);
        }
        else if (Same(name, "LOC09ExitToLOC06"))
        {
            Walk(Hero, 602, 273);
            ChangeLocation("loc06");
        }
        else if (Same(name, "LOC09KisteRechtsClick"))
        {
            // Pushing the crate aside uncovers the way into the hideout (see EveryTick).
            var pushing = WalkAndFace(654, 428, PushBoatRight);
            ObjAfter("LOC09KisteRechts", false, pushing, 20);
            ObjAfter("LOC09KisteRechtsClick", false, pushing);
            ObjAfter("LOC09ExitToLOC08", true, pushing);
            Sound(@"wav\fx\kistesand.wav", pushing);
        }
        else if (Same(name, "LOC09Fusspuren"))
        {
            Walk(Hero, Var(Vars.RucksackPointerX) - 50, Var(Vars.RucksackPointerY));
            var kneeling = State(Hero, KneelRight);
            SetVar(Vars.ExaminingFootprints, 1);
            ObjAfter("LOC09Fusspuren", false, kneeling, 6);
            ObjAfter("LOC09KisteRechtsClick", true, kneeling, 6);
        }
        else if (Same(name, "LOC09Boot"))
            TheBoat();
        else if (Same(name, "LOC09Alf"))
            TalkToAlf();
        else if (Same(name, "LOC09Anne"))
            TalkToAnne(TurnToTheOthers());
        else if (Same(name, "LOC09Julian"))
            TalkToJulian(TurnToTheOthers());
        else if (Same(name, "LOC09Dick"))
            TalkToDickOrGeorge(TurnToTheOthers());
        else if (Same(name, "LOC09Fischer"))
            TalkToTheFisherman();
        return true;
    }

    /// <summary>
    /// The rowing boat: looked at; on quest 6, step 2 the others talk about it; from quest 6 on
    /// (with the rope, the axe and the torch on quest 8) the Five push off to the island.
    /// </summary>
    void TheBoat()
    {
        SetText(BeachScene, "Boot");
        if (Var(Vars.BeachBoat) == 0)
        {
            WalkAndFace(160, 412, StandLeft);
            SetVar(Vars.BeachBoat, 1);
        }
        else if (Var(Vars.BeachBoat) == 1)
            WalkAndFace(160, 412, StandLeft);
        if (At(6, 2))
        {
            var reached = WalkAndFace(160, 412, StandLeft);
            if (Var(Vars.BeachBoat) == 3)
            {
                Say(Who.Julian, "STRAND0018-09", 13, after: reached);
                Say(Who.George, "STRAND0018-09", 14);
                Say(Who.Julian, "STRAND0018-09", 15);
                Say(Who.George, "STRAND0018-09", 16, last: true);
            }
            else if (Var(Vars.BeachBoat) == 4)
            {
                Say(Who.Dick, "STRAND0018-09", 17, after: reached);
                Say(Who.George, "STRAND0018-09", 18);
                Say(Who.Julian, "STRAND0018-09", 19);
                Say(Who.Anne, "STRAND0018-09", 20, last: true);
            }
        }
        else if (!(Quest == 7 && Episode >= 3) && !(Quest == 8 && Episode <= 1))
        {
            if (At(8, 2))
            {
                if (Has(Item.Rope) && Has(Item.Axe) && Has(Item.Torch))
                    PushOff();
            }
            else if (Quest > 5)
                PushOff();
        }
    }

    /// <summary>The hero pushes the boat into the water (see EveryTick); everyone gets in, and off to the island.</summary>
    void PushOff()
    {
        var pushing = WalkAndFace(210, 412, PushBoatLeft);
        PointerAfter(false, pushing, x: 100, y: 200);
        Sound(@"wav\fx\bootsand.wav", pushing);
        WalkAfter(Anne, 150, 400, pushing, 2);
        WalkAfter(Julian, 200, 412, pushing, 4);
        WalkAfter(Hero == Dick ? George : Dick, 250, 500, pushing, 6);
        WalkAfter(Timmy, 250, 500, pushing, 6);
        ChangeLocation("loc13", pushing, 14);
    }

    /// <summary>Alf: three things to say on quests 1-2 (each line waits for the hero only); on quest 8 about the radio.</summary>
    void TalkToAlf()
    {
        if (Quest < 3)
        {
            int talk = Var(Vars.AlfTalk);
            if (talk > 2)
                return;
            var reached = WalkAndFace(44, 430, StandDownRight);
            Depth(@"bmp\obj\quentin\quwr\quwr11.brg", 2500);
            Depth(@"bmp\obj\quentin\qusp\qusp11.brg", 2500);
            Chain = null;
            if (talk == 0)
            {
                IncAfter(Vars.AlfTalk, reached);
                Say(Who.Alf, "STRAND0003-09", 1, after: reached);
                Say(Who.Dick, "STRAND0003-09", 1, after: reached);
                Say(Who.Alf, "STRAND0003-09", 11, after: reached);
                Say(Who.Dick, "STRAND0003-09", 11, last: true, after: reached);
            }
            else if (talk < 2)
            {
                IncAfter(Vars.AlfTalk, reached);
                Say(Who.Alf, "STRAND0003-09", 2, after: reached);
                Say(Who.Dick, "STRAND0003-09", 2, after: reached);
                Say(Who.Alf, "STRAND0003-09", 21, last: true, after: reached);
            }
            else
                Say(Who.Alf, "STRAND0003-09", 3, last: true, after: reached);
        }
        else if (At(8, 2))
        {
            Walk(Hero, 376, 392);
            var turned = Chain = State(Hero, StandUpRight);
            Say(Who.Alf, "STRAND0036", 5, after: turned);
            Say(Who.George, "STRAND0036", 6, after: turned);
            Say(Who.Alf, "STRAND0036", 7, last: true, after: turned);
        }
    }

    /// <summary>The hero walks over to the others and turns to them; returns that turn.</summary>
    TaskHandle TurnToTheOthers()
    {
        Walk(Hero, 170, 470);
        return Chain = State(Hero, StandUpRight);
    }

    /// <summary>What Anne says at each story step.</summary>
    void TalkToAnne(TaskHandle turned)
    {
        int talk = Var(Vars.AnneBeachTalk);
        if (At(6, 2))
        {
            switch (talk)
            {
                case 0:
                    Say(Who.Anne, "STRAND0018-09", 1, last: true);
                    Say(Who.George, "STRAND0018-09", 2, last: true);
                    break;
                case 1:
                    Say(Who.Anne, "STRAND0018-09", 3, last: true);
                    break;
                case 2:
                    Say(Who.Anne, "STRAND0018-09", 4, last: true);
                    break;
                case 3:
                    Say(Who.Anne, "STRAND0018-09", 38, last: true);
                    break;
                case 4:
                    Say(Who.Anne, "STRAND0018-09", 42, last: true);
                    break;
            }
        }
        else if (At(6, 3))
            AfterTheWreck(Vars.ChoseIslandBeach, Who.Anne, Vars.AnneBeachTalk, (1, 2), (7, 8), () => Say(Who.Anne, "ALL0023", 13, last: true));
        else if (At(6, 4))
        {
            if (talk == 0)
            {
                IncAfter(Vars.AnneWreckTalk, turned);
                IncAfter(Vars.AnneBeachTalk, turned);
                IncAfter(Vars.AnneIslandTalk, turned);
                Say(Who.Anne, "ALL0024", 5, last: true);
            }
            else if (talk == 1)
            {
                Say(Who.Anne, "ALL0024", 6);
                Say(Who.Dick, "ALL0024", 7, last: true);
            }
        }
        else if (At(7, 2))
            Say(Who.Anne, "STRAND0026", 1, last: true);
        else if (At(8, 2))
        {
            if (talk == 0)
            {
                IncAfter(Vars.AnneBeachTalk, turned);
                Say(Who.Anne, "STRAND0036", 16);
                Say(Who.George, "STRAND0036", 17, last: true);
            }
            else if (talk == 1)
            {
                Say(Who.Anne, "STRAND0036", 18);
                Say(Who.George, "STRAND0036", 19, last: true);
            }
        }
    }

    /// <summary>What Julian says at each story step.</summary>
    void TalkToJulian(TaskHandle turned)
    {
        int talk = Var(Vars.JulianBeachTalk);
        if (At(6, 2))
        {
            switch (talk)
            {
                case 0:
                    Say(Who.Julian, "STRAND0018-09", 9);
                    Say(Who.George, "STRAND0018-09", 10);
                    Say(Who.Julian, "STRAND0018-09", 11, last: true);
                    break;
                case 1:
                    Say(Who.Julian, "STRAND0018-09", 12, last: true);
                    break;
                case 2:
                    Say(Who.Julian, "STRAND0018-09", 40, last: true);
                    break;
                case 3:
                    Say(Who.Julian, "STRAND0018-09", 44, last: true);
                    break;
            }
        }
        else if (At(6, 3))
            AfterTheWreck(Vars.ChoseIslandBeach, Who.Julian, Vars.JulianBeachTalk, (3, 4), (9, 10), () => Say(Who.Julian, "ALL0023", 14, last: true));
        else if (At(6, 4))
        {
            if (talk == 0)
            {
                IncAfter(Vars.JulianWreckTalk, turned);
                IncAfter(Vars.JulianBeachTalk, turned);
                IncAfter(Vars.JulianIslandTalk, turned);
                Say(Who.Julian, "ALL0024", 8, last: true);
            }
            else if (talk == 1)
                Say(Who.Julian, "ALL0024", 9, last: true);
        }
        else if (At(7, 2))
        {
            if (talk == 0)
            {
                IncAfter(Vars.JulianBeachTalk, turned);
                // As in the original, both lines wait for the hero to turn.
                Say(Who.Julian, "STRAND0026", 3);
                Say(Who.Dick, "STRAND0026", 4, last: true, after: turned);
            }
            else if (talk == 1)
                Say(Who.Julian, "STRAND0026", 5, last: true);
        }
        else if (At(8, 2))
            Say(Who.Julian, "STRAND0036", 15, last: true);
    }

    /// <summary>What Dick (on quests 6-7 and in the cabin talk, George) says at each story step.</summary>
    void TalkToDickOrGeorge(TaskHandle turned)
    {
        int talk = Var(Vars.DickBeachTalk);
        if (At(6, 2))
        {
            switch (talk)
            {
                case 0:
                    Say(Who.Dick, "STRAND0018-09", 5, last: true);
                    Say(Who.George, "STRAND0018-09", 6, last: true);
                    break;
                case 1:
                    Say(Who.Dick, "STRAND0018-09", 7, last: true);
                    Say(Who.George, "STRAND0018-09", 8, last: true);
                    break;
                case 2:
                    Say(Who.Dick, "STRAND0018-09", 39, last: true);
                    break;
                case 3:
                    Say(Who.Dick, "STRAND0018-09", 43, last: true);
                    break;
            }
        }
        else if (At(6, 3))
        {
            AfterTheWreck(Vars.ChoseIslandBeach, Who.George, Vars.GeorgeBeachTalk, (5, 6), (11, 12), () =>
            {
                // As in the original, both lines wait for the hero to turn.
                Say(Who.George, "ALL0023", 15, after: turned);
                Say(Who.Dick, "ALL0023", 16, last: true, after: turned);
            });
        }
        else if (At(6, 4))
        {
            if (Var(Vars.GeorgeBeachTalk) == 0)
            {
                IncAfter(Vars.GeorgeWreckTalk, turned);
                IncAfter(Vars.GeorgeBeachTalk, turned);
                IncAfter(Vars.GeorgeIslandTalk, turned);
                TalkAboutTheCupboard();
            }
            else if (Var(Vars.GeorgeBeachTalk) == 1)
                Say(Who.George, "ALL0024", 20, last: true);
        }
        else if (At(7, 2))
            Say(Who.George, "STRAND0026", 2, last: true);
        else if (At(8, 2))
        {
            if (talk == 0)
            {
                IncAfter(Vars.DickBeachTalk, turned);
                // As in the original, every line waits for the hero to turn.
                Say(Who.Dick, "STRAND0036", 8, after: turned);
                Say(Who.Alf, "STRAND0036", 9, after: turned);
                Say(Who.George, "STRAND0036", 10, after: turned);
                Say(Who.Alf, "STRAND0036", 11, after: turned);
                Say(Who.Dick, "STRAND0036", 12, after: turned);
                Say(Who.Alf, "STRAND0036", 13, last: true, after: turned);
            }
            else if (talk == 1)
                Say(Who.Dick, "STRAND0036", 14, last: true);
        }
    }

    /// <summary>
    /// The fisherman (quest 6, step 2): first what he knows about the island, then George's
    /// questions, and at the end George takes over as the hero.
    /// </summary>
    void TalkToTheFisherman()
    {
        Walk(Hero, 213, 500);
        Chain = State(Hero, StandDownLeft);
        PointerOff();
        SetText(BeachScene, "Fischer");
        if (!At(6, 2))
        {
            PointerOn();
            return;
        }
        switch (Var(Vars.FishermanTalk))
        {
            case 1:
                Say(Who.Fisherman, "STRAND0018-09", 33);
                Say(Who.George, "STRAND0018-09", 34);
                Say(Who.Fisherman, "STRAND0018-09", 35, last: true);
                break;
            case 2:
                Ask(Who.GeorgeAlt, "STRAND0018-09", 36);
                break;
            case 3:
                SetText(BeachScene, "heroWechsel");
                Say(Who.George, "STRAND0018-09", 48);
                Say(Who.Fisherman, "STRAND0018-09", 49);
                Say(Who.Fisherman, "STRAND0018-09", 55);
                Say(Who.Anne, "STRAND0018-09", 50);
                Say(Who.Dick, "STRAND0018-09", 51);
                Say(Who.Fisherman, "STRAND0018-09", 52, last: true);
                break;
        }
    }

    // ---- talking poses ---------------------------------------------------------------------------

    /// <summary>Anne and Julian talk facing down-right; Dick or George (not the hero) down-left.</summary>
    TalkPoses? OthersPoses(int speaker) => speaker switch
    {
        Who.Anne => new TalkPoses(Anne, SpeakDownRight, StandDownRight),
        Who.Julian => new TalkPoses(Julian, SpeakDownRight, StandDownRight),
        Who.Dick => new TalkPoses(Dick, SpeakDownLeft, StandDownLeft),
        Who.George => new TalkPoses(George, SpeakDownLeft, StandDownLeft),
        _ => null,
    };

    /// <summary>George (the hero) talks to the fisherman: facing down-left.</summary>
    bool GeorgeAtTheFisherman => Text(BeachScene) is "Fischer" or "heroWechsel";

    /// <summary>The speakers of the story step: Alf and the fisherman switch animations, the others take poses.</summary>
    /// <remarks>The original's FUN_00445de1.</remarks>
    public override void LineStarting(GameWorld world, int speaker)
    {
        if (Quest < 3)
        {
            if (CurrentSpeaker == Who.Dick)
                StateAfter(Hero, SpeakDownRight, null);
            else if (CurrentSpeaker == Who.Alf)
                AlfSanding(talking: true, null);
        }
        else if (At(6, 2))
        {
            if (Hero.State == SpeakDownLeft)
                Hero.State = StandDownLeft;
            if (CurrentSpeaker is Who.Anne or Who.Julian or Who.Dick)
                OthersTalk();
            else if (CurrentSpeaker == Who.George)
            {
                if (GeorgeAtTheFisherman)
                    StateAfter(Hero, SpeakDownLeft, null);
            }
            else if (CurrentSpeaker == Who.Fisherman)
                FishermanTalking(true, null);
        }
        else if (At(7, 2) || (Quest == 6 && Episode >= 3))
        {
            if (CurrentSpeaker is Who.Anne or Who.Julian or Who.George)
                OthersTalk();
        }
        else if (At(8, 2))
        {
            if (CurrentSpeaker is Who.Anne or Who.Julian or Who.Dick)
                OthersTalk();
            else if (CurrentSpeaker == Who.Alf)
                AlfWithTheRadio(true, null);
        }
    }

    /// <summary>The speakers stand still again (Alf and the fisherman go back to their animations).</summary>
    /// <remarks>The original's FUN_004461e1.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end)
    {
        if (Quest < 3)
        {
            if (CurrentSpeaker == Who.Dick)
                StateAfter(Hero, StandDownRight, end);
            else if (CurrentSpeaker == Who.Alf)
                AlfSanding(talking: false, end);
        }
        else if (At(6, 2))
        {
            if (CurrentSpeaker is Who.Anne or Who.Julian or Who.Dick)
                OthersStand(end);
            else if (CurrentSpeaker == Who.George)
            {
                if (GeorgeAtTheFisherman)
                    StateAfter(Hero, StandDownLeft, end);
            }
            else if (CurrentSpeaker == Who.Fisherman)
                FishermanTalking(false, end);
        }
        else if (At(7, 2) || (Quest == 6 && Episode >= 3))
        {
            if (CurrentSpeaker is Who.Anne or Who.Julian or Who.George)
                OthersStand(end);
        }
        else if (At(8, 2))
        {
            if (CurrentSpeaker is Who.Anne or Who.Julian or Who.Dick)
                OthersStand(end);
            else if (CurrentSpeaker == Who.Alf)
                AlfWithTheRadio(false, end);
        }
    }

    /// <summary>The current speaker (one of the others) takes their talking pose.</summary>
    void OthersTalk()
    {
        if (OthersPoses(CurrentSpeaker) is { } p)
            StateAfter(p.Actor, p.Talking, null);
    }

    /// <summary>The current speaker (one of the others) stands still again.</summary>
    void OthersStand(TaskHandle end)
    {
        if (OthersPoses(CurrentSpeaker) is { } p)
            StateAfter(p.Actor, p.Standing, end);
    }

    /// <summary>Alf sanding his boat: switches between his talking and working animations.</summary>
    void AlfSanding(bool talking, TaskHandle? after)
    {
        string from = talking ? "" : "Speak", to = talking ? "Speak" : "";
        StopAnimAfter("AlfSchleifeArm" + from, after);
        StopAnimAfter("AlfSchleife" + from, after);
        Anim("AlfSchleifeArm" + to, -1, after);
        Anim("AlfSchleife" + to, -1, after);
    }

    /// <summary>Alf with the radio: switches between talking and standing.</summary>
    void AlfWithTheRadio(bool talking, TaskHandle? after)
    {
        StopAnimAfter(talking ? "AlfmitFunk" : "AlfmitFunkSpeak", after);
        Anim(talking ? "AlfmitFunkSpeak" : "AlfmitFunk", -1, after);
    }

    /// <summary>The fisherman: switches between talking and standing.</summary>
    void FishermanTalking(bool talking, TaskHandle? after)
    {
        StopAnimAfter(talking ? "FischerStand" : "FischerStandSpeak", after);
        Anim(talking ? "FischerStandSpeak" : "FischerStand", -1, after);
    }

    // ---- after a conversation ----------------------------------------------------------------

    /// <summary>
    /// The last speaker stands still; talk counters move on. After the fisherman's first talk
    /// George's questions are offered; after the last, George becomes the hero (step 3).
    /// </summary>
    /// <remarks>The original's FUN_004465ca.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (Quest < 3)
        {
            if (CurrentSpeaker == Who.Alf)
                AlfSanding(talking: false, end);
            StateAfter(Hero, StandDownRight, end);
        }
        else if (CurrentSpeaker == Who.George)
            StateAfter(Hero, StandDownLeft, end);
        else if (At(6, 2))
            AfterTalkOnStep2(end);
        else if (At(6, 3))
        {
            if (CurrentSpeaker is Who.Anne or Who.Julian)
            {
                OthersStand(end);
                string talk = CurrentSpeaker == Who.Anne ? Vars.AnneBeachTalk : Vars.JulianBeachTalk;
                if (Var(talk) == 0)
                    SetVar(talk, 1);
            }
            SetText(BeachScene, "");
        }
        else if (At(7, 2) || (Quest == 6 && Episode >= 4))
        {
            if (CurrentSpeaker is Who.Anne or Who.Julian)
                OthersStand(end);
        }
        else if (At(8, 2))
        {
            if (CurrentSpeaker is Who.Anne or Who.Julian or Who.Dick)
                OthersStand(end);
            else if (CurrentSpeaker == Who.Alf)
                AlfWithTheRadio(false, end);
        }
    }

    /// <summary>Quest 6, step 2: after the boat talk, a character's remark, or the fisherman.</summary>
    void AfterTalkOnStep2(TaskHandle end)
    {
        if (Text(BeachScene) == "Boot" && Var(Vars.BeachBoat) == 3)
            SetVar(Vars.BeachBoat, 4);
        switch (CurrentSpeaker)
        {
            case Who.Anne:
                OthersStand(end);
                if (Var(Vars.AnneBeachTalk) == 0)
                    SetVar(Vars.AnneBeachTalk, 1);
                else if (Var(Vars.AnneBeachTalk) == 1)
                    SetVar(Vars.AnneBeachTalk, 2);
                break;
            case Who.Julian:
                OthersStand(end);
                if (Var(Vars.JulianBeachTalk) == 0)
                    SetVar(Vars.JulianBeachTalk, 1);
                break;
            case Who.Dick:
                OthersStand(end);
                if (Var(Vars.DickBeachTalk) == 0)
                    SetVar(Vars.DickBeachTalk, 1);
                break;
            case Who.Fisherman:
                FishermanTalking(false, end);
                if (Text(BeachScene) == "heroWechsel")
                {
                    Episode = 3;
                    Hero = Dick;
                }
                if (Var(Vars.FishermanTalk) == 1)
                {
                    Ask(Who.GeorgeAlt, "STRAND0018-09", 36, after: end);
                    SetVar(Vars.FishermanTalk, 2);
                }
                break;
        }
        SetText(BeachScene, "");
    }

    /// <summary>
    /// The fisherman answers George's question (and the others' talks move on); when every
    /// question has been asked, he has a last thing to say.
    /// </summary>
    /// <remarks>The original's FUN_00446bea.</remarks>
    protected override void AfterChoice(string scene, int line, int option)
    {
        if (Var(ChoicePending) == 0)
            return;
        if (At(6, 2))
        {
            switch (option)
            {
                case 1:
                    Say(Who.Fisherman, "STRAND0018-09", 37, last: true);
                    SetVar(Vars.AnneBeachTalk, 3);
                    SetVar(Vars.DickBeachTalk, 2);
                    SetVar(Vars.JulianBeachTalk, 2);
                    break;
                case 2:
                    Say(Who.Fisherman, "STRAND0018-09", 41, last: true);
                    SetVar(Vars.AnneBeachTalk, 4);
                    SetVar(Vars.DickBeachTalk, 3);
                    SetVar(Vars.JulianBeachTalk, 3);
                    break;
                case 3:
                    Say(Who.Fisherman, "STRAND0018-09", 45);
                    Say(Who.George, "STRAND0018-09", 46);
                    Say(Who.Fisherman, "STRAND0018-09", 47);
                    Say(Who.Fisherman, "STRAND0018-09", 54, last: true);
                    SetVar(Vars.AnneBeachTalk, 5);
                    SetVar(Vars.DickBeachTalk, 4);
                    SetVar(Vars.JulianBeachTalk, 4);
                    SetVar(Vars.BeachBoat, 5);
                    break;
            }
            RemoveChosenOption();
            if (MenuExhausted())
                SetVar(Vars.FishermanTalk, 3);
        }
        SetVar(ChoicePending, 0);
    }

    /// <summary>Examining the footprints (quest 2): the hero kneels a while; the others' talks move on.</summary>
    /// <remarks>The original's FUN_00446ecb.</remarks>
    public override void ObjectChanged(GameWorld world, string name, bool on)
    {
        if (on)
            return;
        if (Same(name, "LOC09Fusspuren") && Var(Vars.ExaminingFootprints) != 0 && Quest < 3)
        {
            SetVar(Vars.FootprintsFound, 1);
            if (Var(Vars.AnneTalk) != 3)
                SetVar(Vars.AnneTalk, 2);
            if (Var(Vars.JulianTalk) != 3)
                SetVar(Vars.JulianTalk, 2);
            if (Var(Vars.FannyTalk) != 3)
                SetVar(Vars.FannyTalk, 2);
            WalkAfter(Hero, 635, 420, null, 10);
            State(Hero, KneelRight);
        }
        SetVar(Vars.ExaminingFootprints, 0);
    }

    /// <summary>While the hero pushes, the crate (quest 2, step 3) or the boat moves ten pixels a tick.</summary>
    /// <remarks>The original's FUN_00446fa0.</remarks>
    public override void EveryTick()
    {
        if (At(2, 3))
        {
            if (Hero.State == PushBoatRight)
            {
                Hero.X += 10;
                SetVar(Vars.CrateX, Var(Vars.CrateX) + 10);
                MoveObj("LOC09KisteRechts", Var(Vars.CrateX), 279);
                SetVar(Vars.CratePushed, 1);
                SetVar(Vars.CrateJustPushed, 1);
            }
        }
        else if (Hero.State == PushBoatLeft)
        {
            Hero.X -= 10;
            SetVar(Vars.BoatX, Var(Vars.BoatX) - 10);
            AnimPos("Boot", Var(Vars.BoatX), 335);
        }
    }
}
