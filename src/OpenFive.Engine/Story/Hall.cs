using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The hall of Kirrin Cottage (loc04), with doors to the study, the kitchen, the guest room and
/// the garden. Quests 3-4: the study door, locked while the professor works. Quest 5: getting the
/// guest-room key (keyhole, newspaper, knife) and Uncle Quentin's suitcase. Quest 7: George takes
/// Uncle Quentin his tea, and the key of the locked study door.
/// </summary>
public sealed class Hall : StoryScript
{
    /// <summary>Who was clicked last ("Anne", "Dick", "Julian"), or "enter" before going into the study.</summary>
    const string Clicked = "clicked";

    /// <summary>Things that are only looked at: where the hero stands, which way they face, and the flag it sets.</summary>
    static readonly Dictionary<string, (int X, int Y, int Facing, string Seen)> LookAt = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LOC04Pflanze1"] = (476, 271, StandRight, Vars.HallPlant1Seen),
        ["LOC04Pflanze2"] = (590, 599, StandRight, Vars.HallPlant2Seen),
        ["LOC04Milchkanne"] = (360, 552, StandRight, Vars.MilkChurnSeen),
        ["LOC04Mantel"] = (360, 495, StandUpRight, Vars.HallCoatSeen),
        ["LOC04Bild"] = (175, 590, StandLeft, Vars.HallPictureSeen),
        ["LOC04Schmetterlingssammlung"] = (564, 463, StandRight, Vars.ButterfliesSeen),
    };

    /// <inheritdoc/>
    public override string Location => "loc04";

    // ---- setting up ------------------------------------------------------------------------------

    /// <summary>
    /// The professor snores (quest 5); the others and the doors as the story step has them; the
    /// hero comes in through the door they came by.
    /// </summary>
    /// <remarks>The original's FUN_0042bee1.</remarks>
    protected override void Setup()
    {
        Obj("LOC04Tuerspalte2", true);
        if (Quest == 5 && Episode < 4)
            Sound(@"wav\fx\schnarch.wav");
        SetVar(Vars.TryingStudyDoor, 0);
        SetVar(Vars.TalkingToAnneInHall, 0);
        SetVar(Vars.TalkingToJulianInHall, 0);
        SetVar(Vars.TalkingToDickInHall, 0);
        SetVar(Vars.KeyholeLookedThrough, 0);
        SetVar(Vars.GuestRoomDoorTried, 0);
        SetVar(Vars.KeyPushedOut, 0);
        SetUpStoryStep();
        ComeIn();
        SetUpAjarStudyDoor();
        SetUpSuitcase();
        if (Quest > 2)
            Obj("LOC04Schirm", false);
        if (Quest == 5)
            Obj("LOC04ExitToLOC06", false);
        else if (Quest > 5 && !At(6, 1))
            Obj("LOC04ExitToLOC06", true);
        if (Quest == 7 && Episode > 3)
        {
            Hide(Anne);
            Hide(Julian);
            Hide(Dick);
            Hide(Timmy);
        }
    }

    /// <summary>The scene of the story step: the professor behind the study door, the others waiting in the hall.</summary>
    /// <remarks>The original's FUN_0042dae5.</remarks>
    void SetUpStoryStep()
    {
        if (At(3, 4))
        {
            if (Var(Vars.StudyDoorState) == 1)
                Sound(German ? @"wav\deutsch\profesor\pr060104.wav" : @"wav\english\profesor\pr060104.wav");
        }
        else if (Episode < 3 && Quest == 5)
        {
            if (Var(Vars.AnneHallTalk) < 7)
            {
                // Waiting by the guest-room door.
                Place(Anne, 214, 269, StandUpRight);
                Obj("LOC04Anne", true);
                Place(Dick, 230, 268, StandDownRight);
                Obj("LOC04Dick", true);
                Place(Julian, 260, 268, StandDownRight);
                Place(Timmy, 200, 268, StandDownRight);
                Obj("LOC04Julian", true);
            }
            else
                OthersByTheStairs();
            Obj("LOC04ExitToLOC06", false);
        }
        else if (At(5, 4))
        {
            Episode = 5;
            SetVar(Vars.V64f534, 0);
            Obj("LOC04ExitToLOC06", false);
        }
        else if (At(7, 3))
        {
            Hero = George;
            OthersByTheStairs();
            Obj("LOC04Tuerspalte4", true);
            Obj("LOC04Koffer", false);
            Chain = null;
            if (FromId == 60) // after the close-up of the hand at the door
            {
                Say(Who.Dick, "FLUR28", 1, last: true);
                Say(Who.Anne, "FLUR28", 2);
                Say(Who.Julian, "FLUR28", 3);
                Say(Who.Dick, "FLUR28", 4);
                Say(Who.Anne, "FLUR28", 5);
                Say(Who.George, "FLUR28", 6, last: true);
            }
        }
        else if (Episode > 3 && Quest == 7)
        {
            Obj("LOC04Anne", false);
            Obj("LOC04Dick", false);
            Obj("LOC04Julian", false);
            Obj("LOC04Tuerspalte4", true);
            Obj("LOC04Koffer", false);
            Sound(@"wav\english\quentin\qu300104.wav");
        }
    }

    /// <summary>Anne, Dick, Julian and Timmy wait at the foot of the stairs.</summary>
    void OthersByTheStairs()
    {
        Place(Anne, 490, 599, StandUpLeft);
        Place(Dick, 385, 560, StandDownLeft);
        Place(Julian, 370, 495, StandDownLeft);
        Place(Timmy, 530, 599, StandDownLeft);
        Obj("LOC04Anne", true);
        Obj("LOC04Dick", true);
        Obj("LOC04Julian", true);
        MoveObj("LOC04Anne", 461, 345);
        MoveObj("LOC04Dick", 372, 341);
        MoveObj("LOC04Julian", 344, 337);
    }

    /// <summary>
    /// The hero comes in through the door they came by, which closes behind them. Out of the guest
    /// room on quest 5, Julian has something to say at once.
    /// </summary>
    /// <remarks>The original's FUN_0042d4d4.</remarks>
    void ComeIn()
    {
        MoveObj("LOC04Door1Obj", 160, 300);
        MoveObj("LOC04Door2Obj", 300, 300);
        MoveObj("LOC04Door3Obj", 300, 108);
        Obj("LOC04Door1Obj", false);
        Obj("LOC04Door2Obj", false);
        Obj("LOC04Door3Obj", false);
        Depth(@"obj\obj\locs\loc04\l04t2.brg", 2000);
        Depth(@"obj\obj\locs\loc04\l04t204.brg", 2000);
        Obj("LOC04ExitToLOC02", true);
        switch (FromId)
        {
            case 2: // the guest room
                CloseDoorBehind("DoCloseDoor3");
                PlaceHere(282, 271, StandLeft);
                if (Episode != 2 && Quest != 5)
                {
                    Walk(Hero, 425, 268);
                    return;
                }
                Walk(Hero, 303, 272);
                var turned = State(Hero, StandDownLeft);
                SetText(Clicked, "Julian");
                SetVar(Vars.TalkingToJulianInHall, 1);
                ObjAfter("LOC04Julian", false, turned);
                if (Var(Vars.WardrobeState) == 0)
                    Obj("LOC04ExitToLOC02", false);
                break;
            case 3: // the study
                CloseDoorBehind("DoCloseDoor1");
                PlaceHere(188, 516, StandLeft);
                Walk(Hero, 291, 565);
                break;
            case 5: // the kitchen
                if (At(5, 1) && Var(Vars.BreakfastOver) == 0)
                {
                    PlaceHere(282, 271, StandDownLeft);
                    return;
                }
                CloseDoorBehind("DoCloseDoor2");
                PlaceHere(346, 491, StandLeft);
                Walk(Hero, 346, 520);
                break;
            default: // the front door
                PlaceHere(232, 620, StandUpRight);
                Walk(Hero, 281, 574);
                break;
        }
    }

    void CloseDoorBehind(string anim)
    {
        Anim(anim);
        Sound(@"wav\fx\hotuezu.wav", delay: 3);
    }

    /// <summary>Puts the hero (and whoever is with them) here.</summary>
    /// <remarks>The original's FUN_0042d7b5.</remarks>
    void PlaceHere(int x, int y, int state)
    {
        Place(Hero, x, y, state);
        Show(Hero);
        ShowParty();
    }

    /// <summary>The study door stands ajar while the professor is inside (quest 3).</summary>
    /// <remarks>The original's FUN_0042efc9.</remarks>
    void SetUpAjarStudyDoor()
    {
        SetVar(Vars.StudyDoorAjarReady, 0);
        Obj("LOC04Door1HalfOpen", false);
        if (Var(Vars.StudyDoorState) == 1)
            Obj("LOC04Door1HalfOpen", true);
    }

    /// <summary>Uncle Quentin's suitcase: standing, or (once emptied) lying down.</summary>
    /// <remarks>The original's FUN_0042d83e.</remarks>
    void SetUpSuitcase()
    {
        StopAnim("Koffer");
        StopAnim("KofferStehend");
        StopAnim("KofferLiegend");
        Depth(@"bmp\obj\locs\loc09\koffer.brg", 1000);
        foreach (string anim in new[] { "KofferStehend", "Koffer", "KofferLiegend" })
        {
            AnimPos(anim, 90, 440);
            AnimScale(anim, 0.9);
        }
        switch (Var(Vars.SuitcaseState))
        {
            case 0 or 1:
                Anim("KofferStehend", -1);
                break;
            case 2:
                Anim("KofferLiegend", -1);
                break;
        }
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    // ---- clicks --------------------------------------------------------------------------------

    /// <summary>The doors, the suitcase, the keyhole, things to look at, and the others.</summary>
    /// <remarks>The original's FUN_0042a882.</remarks>
    public override bool Click(string name)
    {
        if (LookAt.TryGetValue(name, out var spot))
        {
            WalkAndFace(spot.X, spot.Y, spot.Facing);
            SetVar(spot.Seen, 1);
        }
        else if (Same(name, "LOC04ExitToLOC03"))
            GoToTheStudy();
        else if (Same(name, "LOC04TueschlossLoc03"))
        {
            var reached = WalkAndFace(193, 517, TakeMidLeft);
            if (At(7, 4))
            {
                Chain = reached;
                Say(Who.George, "EINGANG0030", 1, last: true, delay: 4);
            }
        }
        else if (Same(name, "LOC04Koffer"))
            LookAtTheSuitcase();
        else if (Same(name, "LOC04ExitToLOC02"))
            GoToTheGuestRoom();
        else if (Same(name, "LOC04Schluesselloch"))
        {
            Walk(Hero, 360, 271);
            Chain = State(Hero, KeyholeLeft);
            if (At(5, 1))
            {
                Say(Who.George, "Diele0010-04", 2, delay: 16);
                Say(Who.Julian, "Diele0010-04", 3, last: true);
                SetVar(Vars.KeyholeLookedThrough, 1);
            }
        }
        else if (Same(name, "LOC04ExitToLOC05"))
        {
            var reached = WalkAndFace(314, 493, TakeMidUpRight);
            Sound(@"wav\fx\hotueauf.wav", reached, 3);
            ChangeLocation("loc05", Anim("DoOpenDoor2", after: reached, delay: 5, endObject: "LOC04Door2Obj", endObjectOn: true));
        }
        else if (Same(name, "LOC04ExitToLOC06"))
            ChangeLocation("loc06", WalkAndFace(310, 595, StandDownRight));
        else if (Same(name, "LOC04Schirm"))
        {
            var reached = WalkAndFace(360, 552, TakeUmbrella);
            Sound(@"wav\fx\schirm.wav", reached);
            ObjAfter("LOC04Schirm", false, reached, 5);
        }
        else if (Same(name, "LOC04Anne"))
            TalkToAnne();
        else if (Same(name, "LOC04Dick"))
            TalkToDick();
        else if (Same(name, "LOC04Julian"))
            TalkToJulian();
        return true;
    }

    /// <summary>
    /// The study door. Quest 5, step 2: with the ship's plan the story moves on. Quest 7: Uncle
    /// Quentin will not be disturbed (step 2), or (step 4) George brings his tea — or the key.
    /// </summary>
    void GoToTheStudy()
    {
        Walk(Hero, 193, 517);
        if (At(5, 2))
        {
            if (Has(Item.ShipPlan))
                Episode = 3;
            TryTheStudyDoor();
        }
        else if (At(7, 2))
        {
            Chain = State(Hero, TakeMidLeft);
            Say(Who.Quentin, "KUECHE0027", 4, last: true, delay: 5);
        }
        else if (At(7, 4))
            KnockWithTheTea();
        else if (!At(7, 5))
            TryTheStudyDoor();
    }

    /// <summary>
    /// Quest 7, step 4: Uncle Quentin answers the knock; George says what she has brought (tea,
    /// sandwiches, both, or nothing). With "enter" remembered, the hero goes in after her line.
    /// </summary>
    void KnockWithTheTea()
    {
        var reached = Chain = State(Hero, TakeMidLeft);
        if (Var(Vars.StudyTeaVisit) == 0)
            Say(Who.Quentin, "EINGANG0030", 5, last: true, delay: 5);
        else
            Say(Who.Quentin, "EINGANG0031", 1, last: true, delay: 5);
        if (Var(Vars.StudyKeyPushedThrough) == 1)
        {
            SetAfter(Vars.StudyKeyPushedThrough, 0, reached);
            Say(Who.Quentin, "EINGANG0031", 5, last: true);
        }
        bool tea = Has(Item.Teapot), plate = Has(Item.Plate);
        if (tea || plate)
        {
            SetText(Clicked, "enter");
            Say(Who.George, "EINGANG0031", tea && plate ? 3 : tea ? 4 : 2, last: true);
        }
        else
        {
            Say(Who.George, "EINGANG0030", 8);
            Say(Who.Quentin, "EINGANG0030", 9, last: true);
        }
    }

    /// <summary>The hero tries the study door: it opens, stands ajar, is locked (the hero rattles it), or is oiled.</summary>
    /// <remarks>The original's FUN_0042a882 (the study door part).</remarks>
    void TryTheStudyDoor()
    {
        switch (Var(Vars.StudyDoorState))
        {
            case 0:
            {
                var reached = State(Hero, TakeMidLeft);
                Sound(@"wav\fx\hotueauf.wav", reached, 3);
                ChangeLocation("loc03", Anim("DoOpenDoor1", after: reached, delay: 5, endObject: "LOC04Door1Obj", endObjectOn: true));
                break;
            }
            case 1:
            {
                var reached = State(Hero, TakeMidLeft);
                PointerAfter(false, reached, x: 100, y: 200);
                ObjAfter("LOC04Door1HalfOpen", false, reached, 5);
                break;
            }
            case 2:
            {
                Chain = null;
                var reached = State(Hero, TakeMidLeft);
                SetVar(Vars.TryingStudyDoor, 1);
                PointerAfter(false, reached, x: 100, y: 200);
                ObjAfter("LOC04Tuerspalte2", false, reached, 5);
                break;
            }
            case 3 or 4:
            {
                var reached = State(Hero, TakeMidLeft);
                SetVar(Vars.TryingStudyDoor, 1);
                ObjAfter("LOC04Tuerspalte2", false, reached, 5);
                PointerAfter(false, reached, x: 100, y: 200);
                break;
            }
            case 5:
                State(Hero, StandLeft);
                break;
        }
    }

    /// <summary>Uncle Quentin's suitcase: on quest 5, step 2 the pointer comes back over it (for the key).</summary>
    void LookAtTheSuitcase()
    {
        if (!At(5, 2))
            WalkAndFace(230, 570, StandLeft);
        else if (Var(Vars.SuitcaseState) == 0)
        {
            PointerOff();
            var reached = WalkAndFace(230, 570, StandLeft);
            PointerAfter(true, reached, x: 100, y: 200);
        }
        else if (Var(Vars.SuitcaseState) == 1)
            WalkAndFace(230, 570, StandLeft);
    }

    /// <summary>The guest-room door: locked on quest 5, step 1 (George says so); from quest 5 the way in.</summary>
    void GoToTheGuestRoom()
    {
        var reached = WalkAndFace(340, 271, TakeMidUpLeft);
        if (At(5, 1))
        {
            Say(Who.George, "Diele0010-04", 1, last: true, after: reached, delay: 6);
            SetVar(Vars.GuestRoomDoorTried, 1);
        }
        else if (Quest >= 5)
        {
            var opened = Anim("DoOpenDoor3", after: reached, delay: 5, endObject: "LOC04Door3Obj", endObjectOn: true);
            ChangeLocation("loc02", opened, 6);
        }
    }

    // ---- talking to the others -----------------------------------------------------------

    /// <summary>The hero walks up to the others at the guest-room door; the next line waits for that.</summary>
    void ApproachTheDoor(string clicked)
    {
        Walk(Hero, 303, 272);
        Chain = State(Hero, StandDownLeft);
        SetText(Clicked, clicked);
    }

    /// <summary>
    /// Quest 5, step 2: the hero walks over (to the stairs once the ship's plan has been found)
    /// and the character's object is switched off; <see cref="ObjectChanged"/> then says the line.
    /// </summary>
    void TalkInTheHall(string clicked, string talkingFlag, string obj)
    {
        if (!At(5, 2))
            return;
        TaskHandle turned;
        if (Has(Item.ShipPlan))
        {
            Walk(Hero, 290, 516);
            turned = State(Hero, StandDownRight);
        }
        else if (Var(Vars.AnneHallTalk) > 6)
            return;
        else
        {
            Walk(Hero, 303, 272);
            turned = State(Hero, StandDownLeft);
        }
        SetText(Clicked, clicked);
        SetVar(talkingFlag, 1);
        ObjAfter(obj, false, turned);
    }

    /// <summary>Anne: on quest 5, step 1 about the keyhole and the newspaper; later through <see cref="TalkInTheHall"/>.</summary>
    void TalkToAnne()
    {
        if (!At(5, 1))
            TalkInTheHall("Anne", Vars.TalkingToAnneInHall, "LOC04Anne");
        else if (Var(Vars.GuestRoomKeyPlan) == 2)
        {
            ApproachTheDoor("Anne");
            Say(Who.Anne, "DIELE0010-04", 14, last: true);
        }
        else if (Var(Vars.KeySeenInLock) == 1)
        {
            ApproachTheDoor("Anne");
            Say(Who.Anne, "DIELE0010-04", 4, last: true);
        }
    }

    /// <summary>Dick: like Anne.</summary>
    void TalkToDick()
    {
        if (!At(5, 1))
            TalkInTheHall("Dick", Vars.TalkingToDickInHall, "LOC04Dick");
        else if (Var(Vars.GuestRoomKeyPlan) == 2)
        {
            ApproachTheDoor("Dick");
            Say(Who.Dick, "DIELE0010-04", 16, last: true);
        }
        else if (Var(Vars.KeySeenInLock) == 1)
        {
            ApproachTheDoor("Dick");
            Say(Who.Dick, "DIELE0010-04", 5, last: true);
        }
    }

    /// <summary>Julian: like Anne, with two lines about the key once he has heard about it.</summary>
    void TalkToJulian()
    {
        if (!At(5, 1))
        {
            TalkInTheHall("Julian", Vars.TalkingToJulianInHall, "LOC04Julian");
            return;
        }
        if (Var(Vars.GuestRoomKeyPlan) == 2)
        {
            ApproachTheDoor("Julian");
            Say(Who.Julian, "Diele0010-04", 15, last: true);
        }
        else if (Var(Vars.KeySeenInLock) == 1 && Var(Vars.JulianHallTalk) == 5)
        {
            ApproachTheDoor("Julian");
            Say(Who.Julian, "Diele0010-04", 6, last: true);
        }
        else if (Var(Vars.KeySeenInLock) == 1 && Var(Vars.JulianHallTalk) == 6)
        {
            ApproachTheDoor("Julian");
            Say(Who.Julian, "Diele0010-04", 7, last: true);
        }
    }

    // ---- object hooks --------------------------------------------------------------------------

    /// <summary>
    /// Switching an object off: taking the umbrella; trying the ajar or locked study door (the
    /// professor answers); and the lines of Anne, Julian and Dick (quest 5).
    /// </summary>
    /// <remarks>The original's FUN_0042c064.</remarks>
    public override void ObjectChanged(GameWorld world, string name, bool on)
    {
        if (on)
            return;
        if (Same(name, "LOC04Schirm"))
        {
            Give(Item.Umbrella);
            SetVar(Vars.UmbrellaTaken, 1);
        }
        else if (Same(name, "LOC04Door1HalfOpen"))
        {
            if (Var(Vars.StudyDoorAjarReady) == 0 || Var(Vars.StudyDoorState) != 1)
                SetVar(Vars.StudyDoorAjarReady, 1);
            else
            {
                PointerOff();
                SetVar(Vars.StudyDoorAjarReady, 0);
                Sound(@"wav\fx\hotuqauf.wav");
                Say(Who.Professor, "Diele0006-01-04", 1);
                Say(Who.George, "Diele0006-01-04", 1, last: true);
            }
        }
        else if (Same(name, "LOC04Tuerspalte2"))
        {
            Chain = null;
            if (Var(Vars.TryingStudyDoor) != 0)
                AnswerAtTheStudyDoor();
        }
        else if (Same(name, "LOC04Anne"))
        {
            Chain = null;
            if (Var(Vars.TalkingToAnneInHall) != 0)
            {
                AnneSays();
                SetVar(Vars.TalkingToAnneInHall, 0);
                ObjAfter("LOC04Anne", true, null);
            }
        }
        else if (Same(name, "LOC04Julian"))
        {
            Chain = null;
            if (Var(Vars.TalkingToJulianInHall) != 0)
            {
                JulianSays();
                SetVar(Vars.TalkingToJulianInHall, 0);
                ObjAfter("LOC04Julian", true, null);
            }
        }
        else if (Same(name, "LOC04Dick"))
        {
            Chain = null;
            if (Var(Vars.TalkingToDickInHall) != 0)
            {
                DickSays();
                SetVar(Vars.TalkingToDickInHall, 0);
                ObjAfter("LOC04Dick", true, null);
            }
        }
    }

    /// <summary>
    /// The locked study door was tried: the professor tells them off (George replies); with an oil
    /// can in the rucksack nothing is said; oiled, the door opens and quest 4 begins.
    /// </summary>
    void AnswerAtTheStudyDoor()
    {
        switch (Var(Vars.StudyDoorState))
        {
            case 2:
                Sound(@"wav\fx\hotuqauf.wav");
                Say(Who.Professor, "Diele0006-01-04", 2);
                Say(Who.George, "Diele0006-01-04", 2, last: true);
                SetVar(Vars.TryingStudyDoor, 0);
                ObjAfter("LOC04Tuerspalte2", true, null, 30);
                Sound(@"wav\fx\hotuqauf.wav");
                break;
            case 3 or 4:
                if (!Has(Item.FullOilCan) && !Has(Item.EmptyOilCan))
                {
                    Sound(@"wav\fx\hotuqauf.wav");
                    Say(Who.Professor, "Diele0006-01-04", 3);
                    Say(Who.George, "Diele0006-01-04", 3, last: true);
                    SetVar(Vars.TryingStudyDoor, 0);
                    ObjAfter("LOC04Tuerspalte2", true, null, 30);
                    return;
                }
                SetVar(Vars.TryingStudyDoor, 0);
                ObjAfter("LOC04Tuerspalte2", true, null, 30);
                PointerOn();
                break;
            case 5: // tried with the empty oil can
                Say(Who.George, "Diele0006-01-04", 4, last: true);
                Sound(@"wav\fx\hotuqauf.wav");
                SetVar(Vars.TryingStudyDoor, 0);
                ObjAfter("LOC04Tuerspalte2", true, null, 30);
                break;
            case 6: // oiled
                SetVar(Vars.StudyDoorState, 0);
                SetStep(4, 1);
                Say(Who.George, "Diele0006-01-04", 1, last: true);
                SetVar(Vars.TryingStudyDoor, 0);
                ObjAfter("LOC04Tuerspalte2", true, null, 30);
                break;
        }
    }

    /// <summary>Anne's lines in the hall (quest 5), in turn.</summary>
    void AnneSays()
    {
        switch (Var(Vars.AnneHallTalk))
        {
            case 0:
                Say(Who.Anne, "FLUR0012-01", 1);
                Say(Who.George, "FLUR0012-01", 1, last: true);
                break;
            case 1:
                Say(Who.Anne, "FLUR0012-01", 2);
                Say(Who.George, "FLUR0012-01", 2, last: true);
                break;
            case 2:
                Say(Who.Anne, "FLUR0012-01", 3, last: true);
                break;
            case 3:
                Say(Who.George, "FLUR0012-01", 4);
                Say(Who.Anne, "FLUR0012-01", 4, last: true);
                break;
            case 8:
                Say(Who.Anne, "FLUR0012-01", 64, last: true);
                SetVar(Vars.AnneHallTalk, 9);
                break;
            case 9:
                Say(Who.Anne, "FLUR0012-01", 65, last: true);
                break;
        }
    }

    /// <summary>Julian's lines in the hall (quest 5), in turn.</summary>
    void JulianSays()
    {
        switch (Var(Vars.JulianHallTalk))
        {
            case 0:
                Say(Who.Julian, "FLUR0012-01", 1);
                Say(Who.George, "FLUR0012-01", 5, last: true);
                break;
            case 1:
                Say(Who.Julian, "FLUR0012-01", 2);
                Say(Who.George, "FLUR0012-01", 6, last: true);
                break;
            case 2:
                Say(Who.George, "FLUR0012-01", 3);
                Say(Who.Anne, "FLUR0012-01", 3);
                Say(Who.George, "FLUR0012-01", 4);
                Say(Who.Julian, "FLUR0012-01", 3, last: true);
                break;
            case 3:
                Say(Who.Julian, "FLUR0012-01", 4);
                Say(Who.George, "FLUR0012-01", 8, last: true);
                SetVar(Vars.JulianHallTalk, 4);
                break;
            case 8:
                Say(Who.Julian, "FLUR0012-01", 68, last: true);
                Say(Who.George, "FLUR0012-01", 69, last: true);
                SetVar(Vars.JulianHallTalk, 9);
                break;
            case 9:
                Say(Who.Julian, "FLUR0012-01", 70, last: true);
                break;
        }
    }

    /// <summary>Dick's lines in the hall (quest 5), in turn.</summary>
    void DickSays()
    {
        switch (Var(Vars.DickHallTalk))
        {
            case 0:
                Say(Who.Dick, "FLUR0012-01", 1);
                Say(Who.George, "FLUR0012-01", 9, last: true);
                break;
            case 1:
                Say(Who.Dick, "FLUR0012-01", 2);
                Say(Who.George, "FLUR0012-01", 10, last: true);
                break;
            case 2:
                Say(Who.Dick, "FLUR0012-01", 3, last: true);
                break;
            case 3:
                Say(Who.Dick, "FLUR0012-01", 4, last: true);
                break;
            case 8:
                Say(Who.Dick, "FLUR0012-01", 66, last: true);
                SetVar(Vars.DickHallTalk, 9);
                break;
            case 9:
                Say(Who.Dick, "FLUR0012-01", 67, last: true);
                break;
        }
    }

    // ---- talking poses ---------------------------------------------------------------------------

    /// <summary>On quest 5 the others stand by the guest-room door until the ship's plan has been found.</summary>
    bool AtTheDoor => !Has(Item.SuitcaseKey) || Var(Vars.AnneHallTalk) < 7;

    /// <summary>The speaker takes their talking pose (depending on the story step and where everyone stands).</summary>
    /// <remarks>The original's FUN_0042e087.</remarks>
    public override void LineStarting(GameWorld world, int speaker)
    {
        if (At(3, 4) || At(4, 1))
        {
            if (CurrentSpeaker == Who.George)
                StateAfter(Hero, SpeakDownLeft, null);
        }
        else if (Episode < 3 && Quest == 5)
        {
            bool door = AtTheDoor;
            if (CurrentSpeaker == Who.George)
            {
                if (Var(Vars.KeyPushedOut) == 0)
                    StateAfter(Hero, door ? SpeakDownLeft : SpeakDownRight, null);
            }
            else if (CurrentSpeaker == Who.Dick)
                StateAfter(Dick, door ? SpeakDownRight : SpeakDownLeft, null);
            else if (CurrentSpeaker == Who.Julian)
                StateAfter(Julian, door ? SpeakDownRight : SpeakDownLeft, null);
        }
        else if (At(7, 3))
        {
            if (CurrentSpeaker == Who.George)
                StateAfter(Hero, SpeakDownRight, null);
            else if (CurrentSpeaker == Who.Dick)
                StateAfter(Dick, SpeakDownLeft, null);
            else if (CurrentSpeaker == Who.Julian)
                StateAfter(Julian, SpeakDownLeft, null);
        }
        else if (GoingInWithTheTea)
            StateAfter(Hero, SpeakDownLeft, null);
    }

    /// <summary>Quest 7, step 4: George's line before going into the study.</summary>
    bool GoingInWithTheTea => At(7, 4) && Text(Clicked) == "enter" && CurrentSpeaker == Who.George;

    /// <summary>The speaker stands again; after George's line at the keyhole, the key has been seen.</summary>
    /// <remarks>The original's FUN_0042e34f.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end)
    {
        if (At(3, 4) || At(4, 1))
        {
            if (CurrentSpeaker == Who.George)
                StateAfter(Hero, StandDownLeft, end);
        }
        else if (Episode < 3 && Quest == 5)
        {
            switch (CurrentSpeaker)
            {
                case Who.George:
                    if (Var(Vars.KeyholeLookedThrough) != 0)
                    {
                        SetVar(Vars.KeySeenInLock, 1);
                        SetVar(Vars.KeyholeLookedThrough, 0);
                    }
                    StateAfter(Hero, StandDownLeft, end);
                    break;
                case Who.Dick:
                    StateAfter(Dick, StandDownRight, end);
                    break;
                case Who.Anne:
                    StateAfter(Anne, StandUpRight, end);
                    break;
                case Who.Julian:
                    StateAfter(Julian, StandDownRight, end);
                    break;
            }
        }
        else if (At(7, 3))
            StandAtTheStairs(end);
        else if (GoingInWithTheTea)
            StateAfter(Hero, StandDownLeft, end);
    }

    /// <summary>Quest 7, step 3: George, Dick and Julian stand again after their lines.</summary>
    void StandAtTheStairs(TaskHandle end)
    {
        if (CurrentSpeaker == Who.George)
            StateAfter(Hero, StandDownRight, end);
        else if (CurrentSpeaker == Who.Dick)
            StateAfter(Dick, StandDownLeft, end);
        else if (CurrentSpeaker == Who.Julian)
            StateAfter(Julian, StandDownLeft, end);
    }

    // ---- after a conversation ----------------------------------------------------------------

    /// <summary>
    /// What follows a conversation: the study door's state moves on (quest 3); on quest 5 it
    /// depends on who was talked to; on quest 7, step 4 George goes into the study.
    /// </summary>
    /// <remarks>The original's FUN_0042e58f.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (At(3, 4))
        {
            StateAfter(Hero, StandDownLeft, end);
            int door = Var(Vars.StudyDoorState);
            if (door == 1)
                SetVar(Vars.StudyDoorState, 2);
            else if (door == 2)
                SetVar(Vars.StudyDoorState, 3);
            else if (door == 3)
                SetVar(Vars.StudyDoorState, 4);
        }
        else if (At(4, 1))
            StateAfter(Hero, StandDownLeft, end);
        else if (Episode < 3 && Quest == 5)
        {
            if (!AfterTalkOnQuest5(end))
                return;
            SetText(Clicked, "");
        }
        else if (At(7, 3))
            StandAtTheStairs(end);
        else if (GoingInWithTheTea)
        {
            StateAfter(Hero, StandDownLeft, end);
            PointerAfter(false, end, x: Var(Vars.RucksackPointerX), y: Var(Vars.RucksackPointerY));
            var reached = StateAfter(Hero, TakeMidLeft, end);
            var opened = Anim("DoOpenDoor1", after: reached, delay: 5, endObject: "LOC04Door1Obj", endObjectOn: true);
            Sound(@"wav\fx\hotueauf.wav", opened, 3);
            ChangeLocation("loc03", opened);
            SetText(Clicked, "");
        }
    }

    /// <summary>
    /// Quest 5, steps 1-2: the talk counters move on, and everyone turns back. A line that was not
    /// started by clicking someone ends the suitcase scene, or (with the key pushed out) opens the
    /// guest room. Returns false when the remembered click is to be kept.
    /// </summary>
    bool AfterTalkOnQuest5(TaskHandle end)
    {
        switch (Text(Clicked))
        {
            case "Anne":
                EveryoneTurnsBack(end);
                MoveOn(Vars.AnneHallTalk);
                return true;
            case "Dick":
                EveryoneTurnsBack(end);
                MoveOn(Vars.DickHallTalk);
                return true;
            case "Julian":
                EveryoneTurnsBack(end);
                AfterJulian(end);
                return true;
        }
        switch (CurrentSpeaker)
        {
            case Who.George:
                if (Text(StoryTexts.HallScene) == "Koffer")
                {
                    CloseTheSuitcase(end);
                    return false;
                }
                if (Has(Item.SuitcaseKey) && Var(Vars.AnneHallTalk) > 6)
                {
                    FaceTheStairs(end);
                    return false;
                }
                if (Var(Vars.KeyPushedOut) == 0)
                {
                    if (Var(Vars.GuestRoomDoorTried) != 0)
                        Obj("LOC04Schluesselloch", true);
                    StateAfter(Hero, StandDownLeft, end);
                }
                else
                    OpenTheGuestRoom(end);
                break;
            case Who.Dick:
                if (Var(Vars.AnneHallTalk) > 6)
                {
                    FaceTheStairs(end);
                    return false;
                }
                if (Var(Vars.KeyPushedOut) == 0)
                    StateAfter(Dick, StandDownRight, end);
                break;
            case Who.Julian:
                if (Has(Item.SuitcaseKey))
                {
                    FaceTheStairs(end);
                    return false;
                }
                StateAfter(Julian, StandDownRight, end);
                break;
        }
        return true;
    }

    /// <summary>A talk counter goes from 0 to 1 and from 2 to 3 (the second remark of each pair).</summary>
    void MoveOn(string talk)
    {
        if (Var(talk) == 0)
            SetVar(talk, 1);
        else if (Var(talk) == 2)
            SetVar(talk, 3);
    }

    /// <summary>After Julian's lines: the next line, the close-up of the hand at the door, or everyone going off.</summary>
    void AfterJulian(TaskHandle end)
    {
        switch (Var(Vars.JulianHallTalk))
        {
            case 0:
                SetVar(Vars.JulianHallTalk, 1);
                break;
            case 2:
                SetVar(Vars.JulianHallTalk, 3);
                PointerAfter(false, end, x: 100, y: 200);
                ChangeLocation("hand", StateAfter(Hero, TakeMidLeftLong, end), 6);
                break;
            case 4:
                Obj("LOC04Julian", false);
                Obj("LOC04Anne", false);
                Obj("LOC04Dick", false);
                break;
            case 5:
                SetVar(Vars.JulianHallTalk, 6);
                break;
        }
    }

    /// <summary>Everyone turns back to face each other, by the guest-room door or at the stairs.</summary>
    void EveryoneTurnsBack(TaskHandle end)
    {
        if (Var(Vars.AnneHallTalk) < 7)
        {
            StateAfter(Hero, StandDownLeft, end);
            StateAfter(Dick, StandDownRight, end);
            StateAfter(Anne, StandUpRight, end);
            StateAfter(Julian, StandDownRight, end);
        }
        else
            FaceTheStairs(end);
    }

    /// <summary>Everyone turns to face each other at the stairs.</summary>
    void FaceTheStairs(TaskHandle end)
    {
        StateAfter(Hero, StandDownRight, end);
        StateAfter(Dick, StandDownLeft, end);
        StateAfter(Anne, StandUpLeft, end);
        StateAfter(Julian, StandDownLeft, end);
    }

    /// <summary>After the suitcase scene the hero shuts the suitcase; everyone's talk moves on.</summary>
    void CloseTheSuitcase(TaskHandle end)
    {
        SetText(StoryTexts.HallScene, "");
        StateAfter(Hero, StandDownRight, end);
        var kneeling = StateAfter(Hero, KneelLeft, end, 4);
        StopAnimAfter("KofferKarte", kneeling, 15);
        Anim("KofferZu", after: kneeling, delay: 15);
        Anim("KofferStehend", -1, kneeling, 18);
        Give(Item.ShipPlan);
        SetVar(Vars.DickHallTalk, 8);
        SetVar(Vars.AnneHallTalk, 8);
        SetVar(Vars.JulianHallTalk, 8);
    }

    /// <summary>With the key pushed out, the hero opens the guest-room door and goes in (quest 5, step 2).</summary>
    void OpenTheGuestRoom(TaskHandle end)
    {
        StateAfter(Hero, TakeMidUpLeft, end);
        var opened = Anim("DoOpenDoor3", after: end, delay: 4, endObject: "LOC04Door3Obj", endObjectOn: true);
        ChangeLocation("loc02", opened, 6);
        Episode = 2;
        SetVar(Vars.AnneHallTalk, 0);
        SetVar(Vars.JulianHallTalk, 0);
        SetVar(Vars.DickHallTalk, 0);
    }
}
