using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The secret hideout in the cliffs (loc08). On quest 3 Dick finds George and Timmy here and asks
/// her questions; on quest 8 the Five meet here, pick up the torch and lantern, and set off.
/// </summary>
public sealed class Cave : StoryScript
{
    /// <summary>"Dialog" while Julian's question about leaving is being asked (quest 8).</summary>
    const string JulianAsks = "t63be0c";

    /// <inheritdoc/>
    public override string Location => "loc08";

    /// <summary>The torch, lantern and binoculars while not taken; the scene of the story step; the candle and tins.</summary>
    /// <remarks>The original's FUN_00440545.</remarks>
    protected override void Setup()
    {
        SetVar(Vars.HideoutAnswer, 0);
        Obj("LOC08Anne", false);
        Obj("LOC08Dick", false);
        Obj("LOC08Julian", false);
        PlaceCharacters();
        StopAnim("Kerze");
        StopAnim("Dosen");
        Depth(@"bmp\obj\locs\loc08\kerze.brg", 1000);
        Depth(@"bmp\obj\locs\loc08\dosen.brg", 5000);
        if (!Has(Item.LanternWithCandle))
            Anim("Kerze", -1);
        Anim("Dosen", -1);
        if (At(3, 4))
        {
            if (IsShown(Dick))
                Hide(Dick);
            if (IsShown(Timmy))
                Hide(Timmy);
        }
    }

    /// <summary>The things lying here, and the characters for the story step (with its scene).</summary>
    /// <remarks>The original's FUN_0044068b.</remarks>
    void PlaceCharacters()
    {
        Obj("LOC08Lampe", Var(Vars.TorchTaken) == 0);
        Obj("LOC08Fernglas", Var(Vars.BinocularsTaken) == 0);
        if (At(2, 3))
            FindGeorge();
        else if (At(8, 1))
            MeetTheOthers();
        else if (At(3, 3))
        {
            Place(Hero, 680, 500, StandLeft);
            Show(Hero);
            Place(George, 350, 560, SitDownRight);
            Place(Timmy, 390, 570, StandDownRight);
            ShowParty();
        }
        else if (Quest > 2)
        {
            if (LocationId == FromId)
            {
                Place(Hero, 680, 500, StandLeft);
                Show(Hero);
                ShowParty();
            }
            else
            {
                Place(Hero, 788, 346, StandLeft);
                Show(Hero);
                WalkAndFace(680, 500, StandDownLeft);
                ShowParty();
            }
        }
    }

    /// <summary>Quest 3 begins: Dick comes in and finds George sitting here; Timmy scratches himself.</summary>
    void FindGeorge()
    {
        SetStep(3, 1);
        Place(Hero, 788, 346, StandLeft);
        Show(Hero);
        Walk(Hero, 680, 500);
        var arrived = Chain = State(Hero, StandDownLeft);
        Place(George, 350, 560, SitDownRight);
        Place(Timmy, 450, 570, StandDownRight);
        WalkAfter(Timmy, 640, 500, null, 45);
        var scratching = State(Timmy, HindLegRight);
        Sound(@"wav\timmy\timmy01.wav", scratching);
        Sound(@"wav\timmy\timmy02.wav", scratching, 10);
        Sound(@"wav\timmy\timmy03.wav", scratching, 20);
        StateAfter(Timmy, HindLegRight, arrived, 30);
        StateAfter(Timmy, HindLegRight, arrived, 90);
        ShowParty();
        PointerOff();
        Say(Who.Dick, "GEHEIM0004-08", 1);
        Say(Who.George, "GEHEIM0004-08", 1);
        Say(Who.Dick, "GEHEIM0004-08", 2);
        Say(Who.George, "GEHEIM0004-08", 2);
        Say(Who.Dick, "GEHEIM0004-08", 3);
        Say(Who.George, "GEHEIM0004-08", 3, last: true);
    }

    /// <summary>
    /// Quest 8: the others are waiting in the hideout; the first time they talk, and Julian's
    /// question is put back on the menu. With the torch and the axe, they are ready to go.
    /// </summary>
    void MeetTheOthers()
    {
        Place(Hero, 788, 346, StandLeft);
        Show(Hero);
        Walk(Hero, 606, 580);
        var arrived = State(Hero, StandUpLeft);
        Obj("LOC08Anne", true);
        Obj("LOC08Dick", true);
        Obj("LOC08Julian", true);
        Place(Timmy, 450, 570, StandDownRight);
        Place(Dick, 500, 346, StandDownRight);
        Place(Anne, 400, 346, StandDownRight);
        Place(Julian, 300, 346, StandDownRight);
        ShowParty();
        SetVar(Vars.AlfTalk, 0);
        SetVar(Vars.AnneBeachTalk, 0);
        SetVar(Vars.DickBeachTalk, 0);
        SetVar(Vars.JulianBeachTalk, 0);
        SetVar(Vars.AnneIslandTalk, 0);
        SetVar(Vars.DickIslandTalk, 0);
        SetVar(Vars.JulianIslandTalk, 0);
        if (Var(Vars.HideoutSceneSeen) == 0)
        {
            ReopenMenu();
            SetVar(Vars.HideoutSceneSeen, 1);
            PointerOff();
            Say(Who.Anne, "VERSTECK0035", 1, after: arrived, delay: 20);
            Say(Who.George, "VERSTECK0035", 2);
            Say(Who.Dick, "VERSTECK0035", 3);
            Say(Who.George, "VERSTECK0035", 4);
            Say(Who.Anne, "VERSTECK0035", 5);
            Say(Who.Julian, "VERSTECK0035", 6);
            Say(Who.Dick, "VERSTECK0035", 7, last: true);
        }
        if (Has(Item.Torch) && Has(Item.Axe))
        {
            SetVar(Vars.AnneHideoutTalk, 2);
            SetVar(Vars.DickHideoutTalk, 2);
            SetVar(Vars.JulianHideoutTalk, 3);
            SetVar(Vars.HideoutLeaving, 1);
        }
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>The way out, talking to the others (quest 8), the torch and the lantern.</summary>
    /// <remarks>The original's FUN_0043fd7d.</remarks>
    public override bool Click(string name)
    {
        if (Same(name, "LOC08ExitToLOC09"))
            GoOut();
        else if (Same(name, "LOC08Anne"))
        {
            TurnToTheOthers();
            if (!At(8, 1))
                return true;
            if (Var(Vars.AnneHideoutTalk) == 0)
                Say(Who.Anne, "VERSTECK0035", 8, last: true);
            else if (Var(Vars.AnneHideoutTalk) == 2)
                Say(Who.Anne, "VERSTECK0035", 19, last: true);
        }
        else if (Same(name, "LOC08Dick"))
        {
            TurnToTheOthers();
            if (!At(8, 1))
                return true;
            if (Var(Vars.DickHideoutTalk) == 0)
            {
                Say(Who.Dick, "VERSTECK0035", 11);
                Say(Who.George, "VERSTECK0035", 12, last: true);
            }
            else if (Var(Vars.DickHideoutTalk) == 2)
                Say(Who.Dick, "VERSTECK0035", 18, last: true);
        }
        else if (Same(name, "LOC08Julian"))
        {
            TurnToTheOthers();
            if (At(8, 1))
                TalkToJulian();
        }
        else if (Same(name, "LOC08Lampe"))
        {
            PointerOff();
            Walk(Hero, 536, 557);
            IncAfter(Vars.TorchTaken, null);
            var taken = ObjAfter("LOC08Lampe", false, null, 5);
            PointerAfter(true, taken, x: 469, y: 538);
        }
        else if (Same(name, "LOC08LaterneKlick"))
        {
            Walk(Hero, 494, 544);
            ObjAfter("LOC08LaterneKlick", false, null, 5);
            StopAnimAfter("Kerze", null);
            IncAfter(Vars.LanternTaken, null);
        }
        return true;
    }

    /// <summary>
    /// Going out to the beach: after the talk with George (quest 3) Dick, George and Timmy go home
    /// to the kitchen; ready to set off (quest 8) everyone goes; otherwise the hero goes alone.
    /// </summary>
    void GoOut()
    {
        if (At(3, 3))
        {
            PointerOff();
            ClearQueue(Timmy);
            Walk(George, 850, 362);
            WalkAfter(Hero, 785, 362, null, 50);
            WalkAfter(Timmy, 765, 362, null, 55);
            ChangeLocation("loc05", null, 6);
            SetVar(Vars.AnneTalk, 0);
            SetVar(Vars.JulianTalk, 0);
            SetVar(Vars.DickTalk, 0);
            SetVar(Vars.QuentinKitchenTalk, 0);
            SetVar(Vars.FannyTalk, 0);
        }
        else if (At(8, 1) && Var(Vars.HideoutLeaving) == 1)
        {
            LeaveTogether();
            ChangeLocation("loc09", Chain, 12);
        }
        else
        {
            Walk(Hero, Var(Vars.RucksackPointerX), 362);
            ChangeLocation("loc09");
        }
    }

    /// <summary>Everyone walks out of the hideout one after the other.</summary>
    void LeaveTogether()
    {
        PointerAfter(false, Chain, x: 300, y: 400);
        WalkAfter(Hero, 760, 366, Chain);
        WalkAfter(Dick, 760, 366, Chain, 2);
        WalkAfter(Anne, 760, 366, Chain, 6);
        WalkAfter(Timmy, 760, 366, Chain, 9);
        WalkAfter(Julian, 760, 366, Chain, 12);
    }

    /// <summary>The hero walks over to the others and turns to them.</summary>
    void TurnToTheOthers()
    {
        Walk(Hero, 606, 580);
        Chain = State(Hero, StandUpLeft);
    }

    /// <summary>Julian (quest 8): first a remark, then his question whether to leave, then a last remark.</summary>
    void TalkToJulian()
    {
        switch (Var(Vars.JulianHideoutTalk))
        {
            case 0:
                IncAfter(Vars.JulianHideoutTalk, Chain);
                Say(Who.Julian, "VERSTECK0035", 9);
                Say(Who.George, "VERSTECK0035", 10, last: true);
                break;
            case 1:
                SetText(JulianAsks, "Dialog");
                IncAfter(Vars.JulianHideoutTalk, null);
                Say(Who.Julian, "VERSTECK0035", 13, last: true);
                break;
            case 3:
                Say(Who.Julian, "VERSTECK0035", 20, last: true);
                break;
        }
    }

    /// <summary>The speaker takes their talking pose (it depends on the story step).</summary>
    /// <remarks>The original's FUN_00440fc5.</remarks>
    public override void LineStarting(GameWorld world, int speaker)
    {
        if (At(3, 1))
        {
            if (CurrentSpeaker == Who.Dick)
                StateAfter(Hero, SpeakDownLeft, null);
            else if (CurrentSpeaker == Who.George)
                StateAfter(George, SitSpeakDownRight, null);
        }
        else if (At(3, 2))
        {
            // Once the hero has sat down beside George, the questions begin (step 3).
            if (Hero.State == SitDownLeft)
            {
                SetStep(3, 3);
                StateAfter(Hero, SitSpeakDownLeft, null);
            }
            else if (CurrentSpeaker == Who.George)
                StateAfter(George, SitSpeakDownRight, null);
            else if (CurrentSpeaker == Who.Dick)
                StateAfter(Hero, SpeakDownLeft, null);
        }
        else if (At(3, 3))
        {
            if (Hero.State == SitSpeakDownLeft)
                Hero.State = SitDownLeft;
            StateAfter(CurrentSpeaker == Who.George ? George : Hero,
                CurrentSpeaker == Who.George ? SitSpeakDownRight : SitSpeakDownLeft, null);
        }
        else if (At(8, 1) && OthersPoses(CurrentSpeaker) is { } p)
            StateAfter(p.Actor, p.Talking, null);
    }

    /// <summary>The speaker stands (or sits) still again after their line.</summary>
    /// <remarks>The original's FUN_00441207.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end)
    {
        if (Quest == 3 && Episode is 1 or 2 or 3)
        {
            if (CurrentSpeaker == Who.George)
                StateAfter(George, SitDownRight, end);
            else if (CurrentSpeaker == Who.Dick)
            {
                if (Episode == 3)
                    StateAfter(Hero, SitDownLeft, null, 30);
                else
                    StateAfter(Hero, StandDownLeft, end);
            }
        }
        else if (At(8, 1) && OthersPoses(CurrentSpeaker) is { } p)
            StateAfter(p.Actor, p.Standing, end);
    }

    /// <summary>Dick, Julian and Anne on quest 8 face down-right.</summary>
    TalkPoses? OthersPoses(int speaker) => speaker switch
    {
        Who.Dick => new TalkPoses(Dick, SpeakDownRight, StandDownRight),
        Who.Julian => new TalkPoses(Julian, SpeakDownRight, StandDownRight),
        Who.Anne => new TalkPoses(Anne, SpeakDownRight, StandDownRight),
        _ => null,
    };

    /// <summary>
    /// After the first talk with George (quest 3) the hero sits down beside her and may ask
    /// questions; after Julian's remark (quest 8) Julian asks whether to leave.
    /// </summary>
    /// <remarks>The original's FUN_004413dd.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (At(3, 1))
        {
            SitDownWithGeorge(end);
            return;
        }
        if (Quest == 3 && Episode < 4)
        {
            SetStep(3, 3);
            if (CurrentSpeaker == Who.George)
                StateAfter(George, SitDownRight, end);
            else if (CurrentSpeaker == Who.Dick)
                StateAfter(Hero, SitDownLeft, end);
            return;
        }
        if (!At(8, 1))
            return;
        if (Text(JulianAsks) != "Dialog")
        {
            if (OthersPoses(CurrentSpeaker) is { } p)
                StateAfter(p.Actor, p.Standing, end);
        }
        else
        {
            StateAfter(Julian, StandDownRight, end);
            // Without the torch and the axe, George asks what they still need.
            if (!(Has(Item.Torch) && Has(Item.Axe)))
            {
                PointerAfter(false, end, x: 300, y: 400);
                Ask(Who.GeorgeAlt, "VERSTECK0035", 14, after: end);
            }
        }
        SetText(JulianAsks, "");
    }

    /// <summary>Timmy comes over and wags his tail; the hero sits down beside George and the questions are offered.</summary>
    void SitDownWithGeorge(TaskHandle end)
    {
        StateAfter(George, SitDownRight, end);
        SetStep(3, 2);
        WalkAfter(Timmy, 350, 560, end);
        WalkAfter(Hero, 650, 550, end, 4);
        WalkAfter(Hero, 511, 534, end);
        PointerAfter(true, end, x: 400, y: 300);
        StateAfter(Timmy, StandRight, end);
        foreach (int delay in new[] { 10, 50, 90, 120, 200 })
            StateAfter(Timmy, WagTailRight, end, delay);
        PointerAfter(false, end, x: 300, y: 400);
        var seated = StateAfter(Hero, SitDownLeft, end);
        Ask(Who.Dick, "GEHEIM0004-02-08", 1, after: seated);
    }

    /// <summary>
    /// Quest 3: George answers Dick's question, and the remaining questions are offered again.
    /// Quest 8: the answer to Julian's question — leave now, or not yet.
    /// </summary>
    /// <remarks>The original's FUN_004417ba.</remarks>
    protected override void AfterChoice(string scene, int line, int option)
    {
        if (Var(ChoicePending) == 0)
            return;
        if (Quest < 8)
        {
            switch (option)
            {
                case 1:
                    Say(Who.George, "GEHEIM0004-02-08", 1);
                    Say(Who.DickAlt, "GEHEIM0004-02-08", 1, last: true);
                    Say(Who.George, "GEHEIM0004-02-08", 11, last: true);
                    break;
                case 2:
                    Say(Who.George, "GEHEIM0004-02-08", 2, last: true);
                    break;
                case 3:
                    Say(Who.George, "GEHEIM0004-02-08", 3, last: true);
                    break;
            }
            RemoveChosenOption();
            if (!MenuExhausted())
                Ask(Who.Dick, "GEHEIM0004-02-08", 1);
        }
        else if (option == 1)
        {
            LeaveTogether();
            SetVar(Vars.HideoutLeaving, 1);
            ChangeLocation("loc09", Chain, 12);
            SetVar(Vars.HideoutAnswer, 1);
        }
        else
        {
            Say(Who.Julian, "VERSTECK0035", 17, last: true);
            SetVar(Vars.HideoutAnswer, 2);
        }
        SetVar(ChoicePending, 0);
    }

    /// <summary>Taking the torch or the lantern (its object switched off) puts it into the rucksack.</summary>
    /// <remarks>The original's FUN_00441c21.</remarks>
    public override void ObjectChanged(GameWorld world, string name, bool on)
    {
        if (on)
            return;
        if (Same(name, "LOC08Lampe"))
        {
            if (Var(Vars.TorchTaken) == 1)
            {
                Give(Item.Torch);
                SetVar(Vars.TorchTaken, 2);
            }
        }
        else if (Same(name, "LOC08LaterneKlick") && Var(Vars.LanternTaken) == 1)
        {
            Give(Item.LanternWithCandle);
            SetVar(Vars.LanternTaken, 2);
        }
    }
}
