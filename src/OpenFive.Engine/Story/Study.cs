using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// Uncle Quentin's study (loc03). Quest 2: Dick talks to Uncle Quentin at his desk. Quest 4: the
/// professor on the telephone. Quest 5: the children are told off. Quest 7: George brings the
/// tea, the fire and the window are used to send Uncle Quentin out, and the key from the tin
/// opens the model ship (the treasure map is inside).
/// </summary>
public sealed class Study : StoryScript
{
    /// <inheritdoc/>
    public override string Location => "loc03";

    // ---- setting up ------------------------------------------------------------------------------

    /// <summary>The room as the story has it (fire, door, window, wood, ship, cup, telephone, Uncle Quentin) and the scene of the story step.</summary>
    /// <remarks>The original's FUN_00426f13.</remarks>
    protected override void Setup()
    {
        PathEnable(0, true);
        SetVar(Vars.StudyPathPending, 1);
        SetVar(Vars.TalkingToAnneInStudy, 0);
        SetVar(Vars.TalkingToJulianInStudy, 0);
        SetVar(Vars.TalkingToDickInStudy, 0);
        SetVar(Vars.ProfessorCallOver, 0);
        SetVar(Vars.ProfessorStanding, 0);
        MoveObj("LOC03Bild", 270, 164);
        Obj("LOC03Bild", true);
        SetUpStoryStep();
        SetUpFire();
        SetUpDoor();
        SetUpWindow();
        SetUpWood();
        SetUpShip();
        SetUpCup();
        SetUpTelephone();
        SetUpQuentin();
        if (FromId == 63) // back from opening the model ship
        {
            ClearQueue(Hero);
            Place(Hero, 367, 360, StandDownLeft);
            Show(Hero);
            ShowParty();
            SetVar(Vars.StudyShipShown, 1);
            SetUpShip();
        }
        else if (Quest > 2)
        {
            if (At(7, 5))
            {
                // George climbed in through the window; Uncle Quentin has gone out.
                Chain = null;
                Say(Who.George, "ARBEIT0034", 1);
                Say(Who.George, "ARBEIT0034", 2, last: true);
                if (Has(Item.Key))
                    Episode = 6;
                InThroughTheWindow();
            }
            else if (At(7, 6))
                InThroughTheWindow();
            else if (Quest < 4)
            {
                Place(Hero, 150, 446, StandDownLeft);
                Show(Hero);
                ShowParty();
                WalkAndFace(504, 481, StandRight);
            }
            if (Quest > 4 && !At(5, 3))
            {
                // Only the hero is in the study.
                Hide(Hero == George ? Dick : George);
                Hide(Anne);
                Hide(Julian);
                Hide(Timmy);
                Obj("LOC03Anne", false);
                Obj("LOC03Julian", false);
                Obj("LOC03Dick", false);
            }
        }
    }

    /// <summary>George comes in through the window (quest 7, steps 5-6); the model ship is on the shelf.</summary>
    void InThroughTheWindow()
    {
        Place(Hero, 530, 346, StandDownLeft);
        Show(Hero);
        ShowParty();
        WalkAndFace(504, 481, StandRight);
        SetVar(Vars.StudyShipShown, 1);
        SetUpShip();
    }

    /// <summary>The scene of the story step.</summary>
    /// <remarks>The original's FUN_00427cb6.</remarks>
    void SetUpStoryStep()
    {
        if (At(2, 2) || At(2, 3))
            DickVisitsQuentin();
        else if (At(4, 1))
            ProfessorOnThePhone();
        else if (Episode < 3 && Quest == 5)
        {
            ComeInThroughTheDoor();
            RoomWhileQuentinWorks();
        }
        else if (At(5, 3))
            ToldOff();
        else if (Quest == 5 && Episode >= 5)
        {
            ComeInThroughTheDoor();
            RoomWhileQuentinWorks();
        }
        else if (Quest == 6)
        {
            ComeInThroughTheDoor();
            RoomWhileQuentinWorks();
        }
        else if (At(7, 3))
        {
            ComeInThroughTheDoor();
            SetVar(Vars.QuentinStudyTalk, 0);
            SetVar(Vars.StudyFire, 0);
            SetVar(Vars.StudyWoodTaken, 1);
            Obj("LOC03Bild", true);
            SetVar(Vars.StudyWindowOpen, 0);
            SetVar(Vars.StudyCupShown, 1);
            Chain = null;
            Say(Who.Quentin, "ARBEIT0029", 1);
            Say(Who.George, "ARBEIT0029", 2);
            Say(Who.Quentin, "ARBEIT0029", 3);
            Say(Who.George, "ARBEIT0029", 4, last: true);
        }
        else if (At(7, 4))
            TeaTime();
        else if (!(Quest == 7 && Episode >= 5))
            ComeInThroughTheDoor();
    }

    /// <summary>The hero comes in through the door from the hall.</summary>
    void ComeInThroughTheDoor()
    {
        Place(Hero, 208, 403, StandDownRight);
        Show(Hero);
        Walk(Hero, 250, 430);
        ShowParty();
    }

    /// <summary>Uncle Quentin at his desk, the fire low, the wood used, the window shut for good.</summary>
    void RoomWhileQuentinWorks()
    {
        SetVar(Vars.QuentinStudyTalk, 0);
        Obj("LOC03QuentinSit", true);
        SetVar(Vars.StudyFire, 0);
        SetVar(Vars.StudyWoodTaken, 1);
        Obj("LOC03Bild", false);
        SetVar(Vars.StudyWindowOpen, 2);
        SetVar(Vars.StudyCupShown, 1);
    }

    /// <summary>Quest 2: Dick walks up to the desk; the first time, Uncle Quentin talks to him.</summary>
    void DickVisitsQuentin()
    {
        TaskHandle? arrived = null;
        if (At(2, 2) && LocationId == FromId)
        {
            Place(Hero, 504, 481, StandUpRight);
            Show(Hero);
            ShowParty();
        }
        else
        {
            Place(Hero, 150, 446, StandDownLeft);
            Show(Hero);
            ShowParty();
            Walk(Hero, 504, 481);
            arrived = State(Hero, StandUpRight);
        }
        if (Var(Vars.QuentinStudyTalk) != 0 || Var(Vars.QuentinWontTalk) != 0)
            return;
        PointerOff();
        Depth(@"bmp\obj\quentin\quwr\quwr11.brg", 2500);
        Depth(@"bmp\obj\quentin\qusp\qusp11.brg", 2500);
        Chain = null;
        if (At(2, 2))
        {
            Say(Who.Quentin, "Dick", 1, after: arrived);
            Say(Who.Dick, "Quentin", 1);
            Say(Who.Quentin, "Dick", 12);
            Say(Who.Dick, "Quentin", 12, last: true);
        }
        else
        {
            Say(Who.Quentin, "Dick", 5, after: arrived);
            Say(Who.Dick, "Quentin", 1);
            Say(Who.Quentin, "Dick", 2);
            Say(Who.Dick, "Quentin", 12);
            Say(Who.Quentin, "Dick", 3);
            Say(Who.Dick, "Quentin", 13, last: true);
        }
        SetVar(Vars.QuentinStudyTalk, 1);
    }

    /// <summary>Quest 4: George comes in on the professor telephoning; Timmy growls at him.</summary>
    void ProfessorOnThePhone()
    {
        Depth(@"bmp\obj\prof\prte\prte04.brg", 5000);
        Depth(@"bmp\obj\prof\prsp\prsp01.brg", 5000);
        Depth(@"bmp\obj\prof\prst\prst.brg", 5000);
        AnimPos("ProfStand", 412, 200);
        AnimPos("ProfSpeak", 426, 200);
        AnimPos("ProfTele", 426, 200);
        AnimPos("ProfTeleStill", 426, 200);
        AnimScale("ProfSpeak", 0.86);
        AnimScale("ProfStand", 0.48);
        AnimScale("ProfTele", 0.86);
        AnimScale("ProfTeleStill", 0.86);
        Anim("ProfTeleStill", -1);
        SetVar(Vars.TelephoneOffHook, 1);
        Place(Hero, 208, 403, StandDownRight);
        Show(Hero);
        Walk(Hero, 250, 430);
        Chain = State(Hero, StandRight);
        Place(Timmy, 180, 420, StandDownRight);
        State(Timmy, StandRight);
        ShowParty();
        Say(Who.Professor, "ARBEITS0007-03", 1);
        Say(Who.Professor, "ARBEITS0007-03", 6);
        Say(Who.Professor, "ARBEITS0007-03", 7);
        StateAfter(Timmy, BarkRight, null, 100);
        Sound(@"wav\timmy\knurren1.wav", null, 100);
        Say(Who.Professor, "ARBEITS0007-03", 2, last: true);
        Say(Who.Professor, "ARBEITS0007-03", 3);
        Say(Who.George, "ARBEITS0007-03", 4);
        Say(Who.Professor, "ARBEITS0007-03", 5);
        Say(Who.George, "ARBEITS0007-03", 6);
        Say(Who.Fanny, "ARBEITS0007-03", 7);
        Say(Who.George, "ARBEITS0007-03", 8, last: true);
    }

    /// <summary>Quest 5, step 3: the children are in the study and Uncle Quentin tells them off; Timmy wags his tail.</summary>
    void ToldOff()
    {
        RoomWhileQuentinWorks();
        Place(Hero, 208, 403, StandDownRight);
        Show(Hero);
        Place(Timmy, 392, 542, StandRight);
        Walk(Hero, 382, 399);
        PointerOff();
        foreach (int delay in new[] { 20, 60, 90, 150 })
            StateAfter(Timmy, WagTailRight, null, delay);
        StateAfter(Timmy, StandUpRight, null, 170);
        Place(Dick, 232, 450, StandDownRight);
        WalkAfter(Dick, 418, 470, null);
        State(Dick, StandUpRight);
        Place(Julian, 499, 480, StandUpRight);
        Chain = State(Hero, StandDownRight);
        ShowParty();
        Say(Who.Quentin, "ARBEITS0013-03", 1);
        Say(Who.George, "ARBEITS0013-03", 2);
        Say(Who.Julian, "ARBEITS0013-03", 3);
        Say(Who.Dick, "ARBEITS0013-03", 4);
        Say(Who.George, "ARBEITS0013-03", 5);
        Say(Who.Quentin, "ARBEITS0013-03", 6);
        Say(Who.Quentin, "ARBEITS0013-03", 30);
        Say(Who.Quentin, "ARBEITS0013-03", 31);
        Say(Who.Anne, "ARBEITS0013-03", 7, last: true);
    }

    /// <summary>
    /// Quest 7, step 4: George brings Uncle Quentin his tea and sandwiches (or one of them).
    /// The fire burns (the first time) and Uncle Quentin's patience starts running down.
    /// </summary>
    void TeaTime()
    {
        Place(Hero, 208, 403, StandDownRight);
        Show(Hero);
        SetVar(Vars.QuentinStudyTalk, 0);
        if (Var(Vars.StudyTeaVisit) == 0)
            SetVar(Vars.StudyFire, 1);
        SetVar(Vars.StudyWoodTaken, 0);
        Obj("LOC03Bild", true);
        SetVar(Vars.StudyCupShown, 1);
        SetVar(Vars.StudyTeaVisit, 1);
        SetVar(Vars.GrumbleTimer, 800);
        bool tea = Has(Item.Teapot), plate = Has(Item.Plate);
        if (tea && plate)
        {
            Walk(Hero, 250, 430);
            ShowParty();
            Chain = null;
            SetVar(Vars.TeaNotBrought, 0);
            if (Var(Vars.QuentinTeaTalk) == 0)
            {
                Say(Who.Quentin, "ARBEIT0032", 3);
                Say(Who.George, "ARBEIT0032", 4, last: true);
            }
            else
                Say(Who.Quentin, "ARBEIT0032", 7, last: true);
        }
        else if (tea)
        {
            Walk(Hero, 250, 430);
            ShowParty();
            SetVar(Vars.TeaNotBrought, 0);
            Chain = null;
            if (Var(Vars.QuentinTeaTalk) == 0)
            {
                SetVar(Vars.QuentinTeaTalk, 1);
                Say(Who.Quentin, "ARBEIT0032", 5);
                Say(Who.George, "ARBEIT0032", 6, last: true);
            }
            else
                Say(Who.Quentin, "ARBEIT0032", 8, last: true);
        }
        else if (plate)
        {
            // Sandwiches without tea: Uncle Quentin sends George back out.
            Walk(Hero, 230, 430);
            ShowParty();
            SetText(StoryTexts.StudyScene, "ToastOnly");
            Chain = State(Hero, TakeMidLeft);
            PointerOff();
            Say(Who.Quentin, "ARBEIT0032", 1);
            Say(Who.George, "ARBEIT0032", 2, last: true);
        }
    }

    /// <summary>The fire: low, burning or high.</summary>
    /// <remarks>The original's FUN_004273b4.</remarks>
    void SetUpFire()
    {
        string[] fires = { "FeuerKlein", "FeuerMittel", "FeuerGross" };
        foreach (string fire in fires)
            StopAnim(fire);
        foreach (string fire in fires)
            AnimPos(fire, 275, 279);
        Depth(@"bmp\obj\locs\loc03\l03f1.brg", 8100);
        Depth(@"bmp\obj\locs\loc03\l03f2.brg", 8100);
        Depth(@"bmp\obj\locs\loc03\l03f3.brg", 8100);
        if (Var(Vars.StudyFireAvailable) == 1 && Var(Vars.StudyFire) is >= 0 and <= 2)
            Anim(fires[Var(Vars.StudyFire)], -1);
    }

    /// <summary>The door: open (quest 4), shut, or closing behind the hero.</summary>
    /// <remarks>The original's FUN_00427a2c.</remarks>
    void SetUpDoor()
    {
        MoveObj("LOC03OpenDoorObj", 115, 164);
        MoveObj("LOC03CloseDoorObj", 115, 164);
        Depth(@"bmp\obj\locs\loc03\l03t0.brg", 2100);
        Depth(@"bmp\obj\locs\loc03\l03t0_00.brg", 2090);
        Obj("LOC03CloseDoorObj", false);
        Obj("LOC03OpenDoorObj", false);
        StopAnim("DoOpenDoor");
        if (Var(Vars.StudyDoorAvailable) != 1)
            return;
        if (At(4, 1))
            Obj("LOC03OpenDoorObj", true);
        else if (Quest == 7 && Episode >= 5)
            Obj("LOC03CloseDoorObj", true);
        else if (LocationId == FromId || FromId == 63)
            Obj("LOC03CloseDoorObj", true);
        else
        {
            Obj("LOC03OpenDoorObj", true);
            ObjAfter("LOC03OpenDoorObj", false, null, 8);
            Anim("DoCloseDoor", delay: 8, endObject: "LOC03CloseDoorObj", endObjectOn: true);
            Sound(@"wav\fx\hotuezu.wav", delay: 8);
        }
    }

    /// <summary>The window: shut, open, or (from quest 5) not to be opened.</summary>
    /// <remarks>The original's FUN_00427bbe.</remarks>
    void SetUpWindow()
    {
        MoveObj("LOC03OpenWindowObj", 515, 174);
        Obj("LOC03OpenWindowObj", false);
        StopAnim("DoOpenWindow");
        StopAnim("DoCloseWindow");
        Obj("LOC03WindowClickObj", true);
        if (Var(Vars.StudyWindowAvailable) != 1)
            return;
        if (Var(Vars.StudyWindowOpen) == 1)
            Obj("LOC03OpenWindowObj", true);
        else if (Var(Vars.StudyWindowOpen) == 2)
            Obj("LOC03WindowClickObj", false);
    }

    /// <summary>The wood in the log basket, before and after a log has been taken.</summary>
    /// <remarks>The original's FUN_004274ca.</remarks>
    void SetUpWood()
    {
        if (Var(Vars.StudyWoodAvailable) != 1)
            return;
        if (Var(Vars.StudyWoodTaken) == 0)
        {
            MoveObj("LOC03Holz1", 235, 346);
            Obj("LOC03Holz1", true);
        }
        else if (Var(Vars.StudyWoodTaken) == 1)
        {
            MoveObj("LOC03Holz2", 235, 352);
            Obj("LOC03Holz2", true);
        }
    }

    /// <summary>The model ship on the shelf, once it is there.</summary>
    /// <remarks>The original's FUN_00427934.</remarks>
    void SetUpShip()
    {
        Obj("LOC03Schiff", false);
        MoveObj("LOC03Schiff", 280, 224);
        if (Var(Vars.StudyShipShown) is 0 or 1)
            Obj("LOC03Schiff", Var(Vars.StudyShipShown) == 1);
    }

    /// <summary>Uncle Quentin's cup on the desk.</summary>
    /// <remarks>The original's FUN_004279b0.</remarks>
    void SetUpCup()
    {
        Obj("LOC03Tasse", false);
        MoveObj("LOC03Tasse", 510, 314);
        if (Var(Vars.StudyCupShown) is 0 or 1)
            Obj("LOC03Tasse", Var(Vars.StudyCupShown) == 1);
    }

    /// <summary>The telephone, with the receiver on the hook or off it.</summary>
    /// <remarks>The original's FUN_0042755e.</remarks>
    void SetUpTelephone()
    {
        Obj("LOC03Telefon1", false);
        Obj("LOC03Telefon2", false);
        if (Var(Vars.TelephoneAvailable) != 1)
            return;
        if (Var(Vars.TelephoneOffHook) == 0)
        {
            MoveObj("LOC03Telefon1", 465, 299);
            Obj("LOC03Telefon1", true);
        }
        else if (Var(Vars.TelephoneOffHook) == 1)
        {
            MoveObj("LOC03Telefon2", 465, 299);
            Obj("LOC03Telefon2", true);
        }
    }

    /// <summary>Uncle Quentin: writing at his desk, asleep in his chair, or reading in the corner (quest 7).</summary>
    /// <remarks>The original's FUN_0042760a.</remarks>
    void SetUpQuentin()
    {
        StopAnim("QuentinWrite");
        StopAnim("QuentinSitTalk");
        StopAnim("QuentinWithBook");
        Obj("LOC03QuentinSit", false);
        Depth(@"bmp\obj\quentin\qusp\qusp11.brg", 2500);
        if (Var(Vars.QuentinAvailable) != 1)
            return;
        if (Quest < 3 || (Episode < 4 && Quest == 5))
        {
            QuentinWriting();
            if (Var(Vars.QuentinWontTalk) == 0)
                Obj("LOC03QuentinSit", true);
        }
        else if (Quest == 6 || (Quest == 5 && Episode >= 5))
        {
            QuentinWriting();
            Obj("LOC03QuentinSit", true);
        }
        else if (At(8, 1) || (Quest == 7 && Episode >= 5))
        {
            Depth(@"bmp\obj\quentin\qusl\qusl.brg", 2500);
            AnimPos("QuentinSleep", 580, 240);
            AnimScale("QuentinSleep", 0.95);
            Anim("QuentinSleep", -1);
            Music("");
            Music(@"wav\atmo\arbeitszimmer.wav");
        }
        else if (Episode > 2 && Quest == 7)
        {
            Depth(@"bmp\obj\quentin\qubk\qubk.brg", 1500);
            foreach (string anim in new[] { "QuentinWithBook", "QuentinWithBookSpeak" })
            {
                AnimPos(anim, 80, 200);
                AnimScale(anim, 1.2);
            }
            Anim("QuentinWithBook", -1);
            Obj("LOC03QuentinSit", false);
        }
    }

    void QuentinWriting()
    {
        Depth(@"bmp\obj\quentin\quwr\quwr11.brg", 2500);
        AnimPos("QuentinWrite", 540, 240);
        AnimScale("QuentinWrite", 0.95);
        Anim("QuentinWrite", -1);
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    // ---- clicks --------------------------------------------------------------------------------

    /// <summary>The door, the wood, the window, the things on the desk and shelf, Uncle Quentin and the others.</summary>
    /// <remarks>The original's FUN_0042486f.</remarks>
    public override bool Click(string name)
    {
        if (Same(name, "LOC03ExitToLOC04"))
            TheDoor();
        else if (Same(name, "LOC03Holz1"))
        {
            Walk(Hero, 330, 346);
            ObjAfter("LOC03Holz1", false, State(Hero, KneelLeft), 5);
        }
        else if (Same(name, "LOC03Holz2"))
        {
            Walk(Hero, 330, 346);
            var kneeling = State(Hero, KneelLeft);
            if (At(7, 3))
            {
                Chain = kneeling;
                Say(Who.Quentin, "ARBEIT0029", 7, last: true);
            }
        }
        else if (Same(name, "LOC03WindowClickObj") && Var(Vars.StudyWindowOpen) == 0)
            TheWindow();
        else if (Same(name, "LOC03Telefon1"))
            QuentinMinds(450, 450, TakeMidRightLong);
        else if (Same(name, "LOC03Telefon2"))
            QuentinMinds(450, 440, TakeMidRightLong);
        else if (Same(name, "LOC03Schiff"))
            TheShip();
        else if (Same(name, "LOC03Tasse"))
        {
            Walk(Hero, 470, 470);
            var reached = State(Hero, TakeMidRightLong);
            if (At(7, 3))
            {
                Chain = reached;
                Say(Who.Quentin, "ARBEIT0029", 7, last: true);
            }
        }
        else if (Same(name, "LOC03Bild"))
            QuentinMinds(367, 360, TakeLowLeftLong);
        else if (Same(name, "LOC03QuentinSit"))
            TalkToQuentin();
        else if (At(5, 3))
        {
            if (Same(name, "LOC03Anne"))
                TalkToAnne();
            else if (Same(name, "LOC03Julian"))
                TalkToJulian();
            else if (Same(name, "LOC03Dick"))
                TalkToDick();
        }
        return true;
    }

    /// <summary>
    /// Out of the study: on quest 5, step 3 Uncle Quentin has more to say; on quest 7, step 4
    /// George leaves once he is going out (step 5 follows); on quest 8 off to the hideout.
    /// </summary>
    void TheDoor()
    {
        if (At(5, 3))
        {
            Walk(Hero, 380, 399);
            State(Hero, StandDownRight);
            Say(Who.Quentin, "ARBEITS0013-03", 8);
            Say(Who.George, "ARBEITS0013-03", 9);
            Say(Who.Quentin, "ARBEITS0013-03", 10);
            Say(Who.Quentin, "ARBEITS0013-03", 40);
            Say(Who.George, "ARBEITS0013-03", 11);
            Say(Who.Quentin, "ARBEITS0013-03", 12, last: true);
            return;
        }
        if (At(7, 4) && Var(Vars.StudyWindowTalk) == 2)
        {
            Episode = 5;
            SetVar(Vars.AnneTalk, 0);
            SetVar(Vars.DickTalk, 0);
            SetVar(Vars.JulianTalk, 0);
            SetVar(Vars.TeaPlanStage, 0);
            var opened = OpenTheDoor();
            ObjAfter("LOC03OpenDoorObj", true, opened);
            ChangeLocation("loc04", opened);
            PointerOff();
            return;
        }
        if (At(7, 6))
        {
            WalkAndFace(228, 435, StandDownLeft);
            return;
        }
        if (At(8, 1))
        {
            var reached = WalkAndFace(228, 435, StandDownLeft);
            Say(Who.George, "ARBEIT0034", 3, last: true, after: reached);
            ChangeLocation("loc08", reached);
            return;
        }
        var open = OpenTheDoor();
        PointerAfter(false, open, x: 100, y: 200);
        ObjAfter("LOC03OpenDoorObj", true, open);
        ChangeLocation("loc04", open, 5);
    }

    /// <summary>The hero walks to the door and opens it.</summary>
    TaskHandle? OpenTheDoor()
    {
        var reached = WalkAndFace(228, 435, TakeMidUpLeft);
        Sound(@"wav\fx\hotueauf.wav", reached, 3);
        return Anim("DoOpenDoor", after: reached, delay: 5, endObject: "LOC03CloseDoorObj");
    }

    /// <summary>
    /// The window: Uncle Quentin objects while he works (quest 7, step 3) and while the fire is
    /// low (step 4); with the fire high he agrees it is hot, and the window is opened.
    /// </summary>
    void TheWindow()
    {
        Walk(Hero, 530, 346);
        var reached = State(Hero, TakeLowRight);
        if (At(7, 3))
        {
            Chain = reached;
            PointerAfter(false, reached, x: 300, y: 400);
            Say(Who.Quentin, "ARBEIT0029", 5, last: true);
            return;
        }
        if (At(7, 4))
        {
            if (Var(Vars.StudyFire) < 2)
            {
                Chain = reached;
                PointerAfter(false, reached, x: 300, y: 400);
                Say(Who.Quentin, "ARBEIT0032", 11, last: true);
                SetAfter(Vars.StudyWindowTalk, 1, reached);
                return;
            }
            if (Var(Vars.StudyFire) == 2)
            {
                Chain = reached;
                PointerAfter(false, reached, x: 300, y: 400);
                Say(Who.Quentin, "ARBEIT0032", 15, last: true);
                SetAfter(Vars.StudyWindowTalk, 2, reached);
            }
        }
        SetVar(Vars.StudyWindowOpen, 1);
        Depth(@"bmp\obj\locs\loc03\l03w0.brg", 6000);
        Depth(@"bmp\obj\locs\loc03\l03w0_04.brg", 6000);
        AnimPos("DoOpenWindow", 515, 174);
        MoveObj("LOC03OpenWindowObj", 515, 174);
        var opened = Anim("DoOpenWindow", after: reached, delay: 6, endObject: "LOC03OpenWindowObj", endObjectOn: true);
        Sound(@"wav\fx\Fensauf.wav", opened);
        SetVar(Vars.StudyWindowOpen, 1);
    }

    /// <summary>The hero touches something of Uncle Quentin's; on quest 7 he minds (a different line after the first time on step 4).</summary>
    void QuentinMinds(int x, int y, int reach)
    {
        Walk(Hero, x, y);
        var reached = State(Hero, reach);
        if (At(7, 3))
        {
            Chain = reached;
            Say(Who.Quentin, "ARBEIT0029", 7, last: true);
        }
        else if (At(7, 4))
        {
            Chain = reached;
            if (Var(Vars.QuentinDisturbed) == 0)
            {
                IncAfter(Vars.QuentinDisturbed, reached);
                Say(Who.Quentin, "ARBEIT0032", 16, last: true);
            }
            else
                Say(Who.Quentin, "ARBEIT0032", 13, last: true);
        }
    }

    /// <summary>The model ship: Uncle Quentin will not let it be touched while he works (quest 7).</summary>
    void TheShip()
    {
        Walk(Hero, 367, 360);
        var reached = State(Hero, TakeMidLeftLong);
        if (At(7, 3))
        {
            PointerOff();
            SetText(StoryTexts.StudyScene, "Schiff");
            Chain = reached;
            Say(Who.Quentin, "ARBEIT0029", 8, last: true);
            SetAfter(Vars.AnneTalk, 0, reached);
            SetAfter(Vars.DickTalk, 0, reached);
            SetAfter(Vars.JulianTalk, 0, reached);
        }
        else if (At(7, 4))
        {
            SetVar(Vars.GrumbleTimer, 700);
            Chain = reached;
            if (Var(Vars.QuentinDisturbed) == 0)
            {
                Say(Who.Quentin, "ARBEIT0032", 16, last: true);
                IncAfter(Vars.QuentinDisturbed, reached);
            }
            else
                Say(Who.Quentin, "ARBEIT0032", 13, last: true);
        }
    }

    /// <summary>The hero walks up to Uncle Quentin's desk; returns the turn to him.</summary>
    TaskHandle WalkUpToQuentin()
    {
        Walk(Hero, 504, 481);
        return State(Hero, StandUpRight);
    }

    /// <summary>Walks up to the desk for one of Uncle Quentin's quest 2 or quest 5 talks.</summary>
    TaskHandle WalkUpToTheDesk()
    {
        var turned = WalkUpToQuentin();
        Depth(@"bmp\obj\quentin\quwr\quwr11.brg", 2500);
        Depth(@"bmp\obj\quentin\qusp\qusp11.brg", 2500);
        Chain = null;
        return turned;
    }

    /// <summary>What Uncle Quentin says at each story step (until he will not talk any more).</summary>
    void TalkToQuentin()
    {
        bool willTalk = Var(Vars.QuentinWontTalk) == 0;
        int talk = Var(Vars.QuentinStudyTalk);
        if (At(2, 2))
        {
            if (!willTalk)
                return;
            if (talk == 1)
            {
                var turned = WalkUpToTheDesk();
                Say(Who.Quentin, "Dick", 2, after: turned);
                Say(Who.Dick, "Quentin", 2, last: true);
                SetVar(Vars.QuentinStudyTalk, 2);
            }
            else if (talk == 2)
            {
                var turned = WalkUpToTheDesk();
                Say(Who.Quentin, "Dick", 3, after: turned);
                Say(Who.Dick, "Quentin", 3, last: true);
            }
        }
        else if (At(2, 3))
        {
            if (willTalk && talk is >= 0 and <= 5)
                QuentinOnQuest2(talk, WalkUpToTheDesk());
        }
        else if (At(6, 2))
        {
            var turned = WalkUpToQuentin();
            // As in the original, each line waits only for the hero to turn.
            Say(Who.Quentin, "ARBEIT0016-03", 1, last: true, after: turned);
            Say(Who.George, "ARBEIT0016-03", 2, last: true, after: turned);
            Say(Who.Quentin, "ARBEIT0016-03", 3, last: true, after: turned);
        }
        else if (At(6, 3))
        {
            Walk(Hero, 504, 481);
            Chain = State(Hero, StandUpRight);
            Say(Who.Quentin, "ARBEIT0019-03", 1, last: true);
        }
        else if (At(5, 3))
        {
            Walk(Hero, 382, 399);
            State(Hero, StandDownRight);
            Say(Who.Quentin, "ARBEITS0013-03", 23);
            Say(Who.George, "ARBEITS0013-03", 24);
            Say(Who.Quentin, "ARBEITS0013-03", 10);
            Say(Who.Quentin, "ARBEITS0013-03", 40);
            Say(Who.George, "ARBEITS0013-03", 11);
            Say(Who.Quentin, "ARBEITS0013-03", 12, last: true);
        }
        else if (Episode < 3 && Quest == 5)
        {
            var turned = WalkUpToTheDesk();
            Say(Who.Quentin, "George", 1, last: true, after: turned);
        }
        else if (Quest == 5 && Episode >= 5)
        {
            var turned = WalkUpToQuentin();
            Say(Who.Quentin, "ARBEIT0015-03", willTalk ? 1 : 2, last: true, after: turned);
        }
    }

    /// <summary>Quest 2, step 3: Uncle Quentin and Dick's talk, one part per click.</summary>
    void QuentinOnQuest2(int talk, TaskHandle turned)
    {
        switch (talk)
        {
            case 0:
                Say(Who.Quentin, "Dick", 5, after: turned);
                Say(Who.Dick, "Quentin", 1);
                Say(Who.Quentin, "Dick", 2);
                Say(Who.Dick, "Quentin", 12);
                Say(Who.Quentin, "Dick", 3);
                Say(Who.Dick, "Quentin", 13, last: true);
                break;
            case 1:
                Say(Who.Quentin, "Dick", 6, after: turned);
                Say(Who.Dick, "Quentin", 2, last: true);
                break;
            case 2:
                Say(Who.Quentin, "Dick", 7, after: turned);
                Say(Who.Dick, "Quentin", 3, last: true);
                break;
            case 3:
                Say(Who.Quentin, "Dick", 8, after: turned);
                Say(Who.Dick, "Quentin", 8);
                Say(Who.Quentin, "Dick", 81, last: true);
                break;
            case 4:
                Say(Who.Quentin, "Dick", 9, after: turned);
                Say(Who.Dick, "Quentin", 9);
                // As in the original, this line waits only for the hero to turn.
                Say(Who.Quentin, "Dick", 91, last: true, after: turned);
                break;
            case 5:
                Say(Who.Quentin, "Dick", 10, last: true, after: turned);
                return;
        }
        SetVar(Vars.QuentinStudyTalk, talk + 1);
    }

    /// <summary>Anne (quest 5, step 3): a remark; then another, with George's answer.</summary>
    void TalkToAnne()
    {
        Walk(Hero, 382, 399);
        var turned = State(Hero, StandDownLeft);
        if (Var(Vars.AnneStudyTalk) == 0)
        {
            Say(Who.Anne, "ARBEITS0013-03", 14, last: true, after: turned);
            SetVar(Vars.TalkingToAnneInStudy, 1);
        }
        else if (Var(Vars.AnneStudyTalk) == 1)
        {
            Say(Who.Anne, "ARBEITS0013-03", 15, after: turned);
            Say(Who.George, "ARBEITS0013-03", 16, last: true);
            SetVar(Vars.TalkingToAnneInStudy, 1);
        }
    }

    /// <summary>Julian (quest 5, step 3): a remark with George's answer; then another.</summary>
    void TalkToJulian()
    {
        Walk(Hero, 382, 399);
        var turned = State(Hero, StandDownRight);
        if (Var(Vars.JulianStudyTalk) == 0)
        {
            Say(Who.Julian, "ARBEITS0013-03", 20, after: turned);
            Say(Who.George, "ARBEITS0013-03", 21, last: true);
            SetVar(Vars.TalkingToJulianInStudy, 1);
        }
        else if (Var(Vars.JulianStudyTalk) == 1)
        {
            Say(Who.Julian, "ARBEITS0013-03", 22, last: true, after: turned);
            SetVar(Vars.TalkingToJulianInStudy, 1);
        }
    }

    /// <summary>Dick (quest 5, step 3): a remark with George's answer; then another.</summary>
    void TalkToDick()
    {
        Walk(Hero, 382, 399);
        var turned = State(Hero, StandDownRight);
        if (Var(Vars.DickStudyTalk) == 0)
        {
            Say(Who.Dick, "ARBEITS0013-03", 17, after: turned);
            Say(Who.George, "ARBEITS0013-03", 18, last: true);
            SetVar(Vars.TalkingToDickInStudy, 1);
        }
        else if (Var(Vars.DickStudyTalk) == 1)
        {
            Say(Who.Dick, "ARBEITS0013-03", 19, last: true, after: turned);
            SetVar(Vars.TalkingToDickInStudy, 1);
        }
    }

    /// <summary>Taking a log (its object switched off): the basket shows fewer logs, and the wood goes into the rucksack.</summary>
    /// <remarks>The original's FUN_004272f8.</remarks>
    public override void ObjectChanged(GameWorld world, string name, bool on)
    {
        if (!on && Same(name, "LOC03Holz1"))
        {
            MoveObj("LOC03Holz2", 235, 352);
            ObjAfter("LOC03Holz2", true, null);
            Give(Item.Firewood);
            SetVar(Vars.StudyWoodTaken, 1);
        }
    }

    // ---- talking ---------------------------------------------------------------------------------

    /// <summary>Uncle Quentin at his desk looks up to talk.</summary>
    void QuentinLooksUp()
    {
        StopAnimAfter("QuentinWrite", null);
        StopAnimAfter("QuentinSitStill", null);
        AnimPos("QuentinSitTalk", 540, 240);
        AnimScale("QuentinSitTalk", 0.95);
        Anim("QuentinSitTalk", -1);
    }

    /// <summary>Uncle Quentin at his desk sits still between lines.</summary>
    void QuentinSitsStill(TaskHandle end)
    {
        StopAnimAfter("QuentinSitTalk", end);
        StopAnimAfter("QuentinWrite", end);
        AnimPos("QuentinSitStill", 540, 240);
        AnimScale("QuentinSitStill", 0.95);
        Anim("QuentinSitStill", -1, end);
    }

    /// <summary>Uncle Quentin goes back to writing after the conversation.</summary>
    void QuentinWritesAgain(TaskHandle end, bool stopSitStill = true)
    {
        StopAnimAfter("QuentinSitTalk", end);
        if (stopSitStill)
            StopAnimAfter("QuentinSitStill", end);
        AnimPos("QuentinWrite", 540, 240);
        AnimScale("QuentinWrite", 0.95);
        Anim("QuentinWrite", -1, end);
    }

    /// <summary>
    /// The speaker takes their talking pose: Uncle Quentin looks up from his desk (or his book),
    /// the professor turns from the telephone, George faces the room.
    /// </summary>
    /// <remarks>The original's FUN_004294ed.</remarks>
    public override void LineStarting(GameWorld world, int speaker)
    {
        if (At(7, 5) || At(7, 6) || At(8, 1))
            return;
        if (At(4, 1))
        {
            if (CurrentSpeaker == Who.George)
                StateAfter(Hero, SpeakDownRight, null);
            else if (CurrentSpeaker == Who.Professor && Var(Vars.ProfessorStanding) != 0)
            {
                StopAnimAfter("ProfStand", null);
                Anim("ProfSpeak", -1);
            }
        }
        else if (At(5, 3))
        {
            if (CurrentSpeaker == Who.Quentin)
                QuentinLooksUp();
            else if (CurrentSpeaker == Who.George)
                StateAfter(Hero, SpeakDownRight, null);
        }
        else if (Episode < 5 && Quest == 2)
        {
            if (CurrentSpeaker == Who.Quentin)
                QuentinLooksUp();
        }
        else if (Quest == 5 && Episode >= 5)
            QuentinLooksUp();
        else if (Quest == 5)
        {
            if (CurrentSpeaker == Who.Quentin)
                QuentinLooksUp();
        }
        else if (Episode > 2 && Quest == 7)
        {
            if (CurrentSpeaker == Who.George)
                StateAfter(Hero, SpeakDownLeft, null);
            else if (CurrentSpeaker == Who.Quentin)
            {
                StopAnimAfter("QuentinWithBook", null);
                Anim("QuentinWithBookSpeak", -1);
            }
        }
    }

    /// <summary>The speaker stops talking: Uncle Quentin sits still (or reads on), the professor stands, George stands.</summary>
    /// <remarks>The original's FUN_004298dd.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end)
    {
        if (Episode < 4 && Quest == 2)
        {
            if (CurrentSpeaker == Who.Quentin)
                QuentinSitsStill(end);
        }
        else if (At(4, 1))
        {
            if (CurrentSpeaker == Who.George)
                StateAfter(Hero, StandDownRight, end);
            else if (CurrentSpeaker == Who.Professor && Var(Vars.ProfessorCallOver) != 0)
            {
                StopAnimAfter("ProfSpeak", end);
                Anim("ProfStand", -1, end);
                SetVar(Vars.ProfessorStanding, 1);
            }
        }
        else if (At(5, 3))
        {
            if (CurrentSpeaker == Who.Quentin)
                QuentinSitsStill(end);
            else if (CurrentSpeaker == Who.George)
                StateAfter(Hero, StandDownRight, end);
        }
        else if (Episode > 2 && Quest == 7)
        {
            if (CurrentSpeaker == Who.George)
                StateAfter(Hero, StandDownLeft, end);
            else if (CurrentSpeaker == Who.Quentin)
            {
                StopAnimAfter("QuentinWithBookSpeak", end);
                Anim("QuentinWithBook", -1, end);
            }
        }
    }

    /// <summary>What follows a conversation, for each story step.</summary>
    /// <remarks>The original's FUN_00429c4b.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (Episode < 4 && Quest == 2)
            QuentinWritesAgain(end);
        else if (At(4, 1))
            AfterTheProfessorsCall(end);
        else if (At(5, 3))
            AfterTellingOff(end);
        else if (Episode < 3 && Quest == 5)
            QuentinWritesAgain(end, stopSitStill: false);
        else if (Quest == 5 && Episode >= 5)
            QuentinHasSaidHisPiece(end);
        else if (Quest == 6)
        {
            if (CurrentSpeaker == Who.Quentin)
                QuentinHasSaidHisPiece(end);
        }
        else if (At(7, 3))
        {
            if (CurrentSpeaker == Who.George)
                StateAfter(Hero, StandDownLeft, end);
            else if (Text(StoryTexts.StudyScene) == "Schiff")
                SentOutAfterTheShip(end);
            else if (CurrentSpeaker == Who.Quentin)
                QuentinReadsOn(end);
        }
        else if (At(7, 4))
        {
            if (CurrentSpeaker == Who.George)
            {
                StateAfter(Hero, StandDownLeft, end);
                if (Text(StoryTexts.StudyScene) == "ToastOnly")
                {
                    PointerAfter(false, end, x: 300, y: 400);
                    ChangeLocation("loc04", end, 4);
                    SetText(StoryTexts.StudyScene, "");
                }
            }
            else if (CurrentSpeaker == Who.Quentin)
                QuentinReadsOn(end);
        }
    }

    void QuentinReadsOn(TaskHandle end)
    {
        StopAnimAfter("QuentinWithBookSpeak", end);
        Anim("QuentinWithBook", -1, end);
    }

    /// <summary>Uncle Quentin goes back to writing and will not talk again.</summary>
    void QuentinHasSaidHisPiece(TaskHandle end)
    {
        StopAnimAfter("QuentinSitTalk", end);
        if (Var(Vars.QuentinWontTalk) == 0)
            SetVar(Vars.QuentinWontTalk, 1);
        AnimPos("QuentinWrite", 540, 240);
        AnimScale("QuentinWrite", 0.95);
        Anim("QuentinWrite", -1, end);
    }

    /// <summary>
    /// Quest 4: after the professor's call he puts the receiver down and Timmy barks at him; after
    /// the rest, George leaves and the close-up of the model ship follows.
    /// </summary>
    void AfterTheProfessorsCall(TaskHandle end)
    {
        if (CurrentSpeaker != Who.Professor)
        {
            StateAfter(Hero, StandDownRight, end);
            WalkAfter(Hero, 210, 390, end);
            ChangeLocation("schiff", end, 10);
            return;
        }
        if (Var(Vars.ProfessorCallOver) != 0)
            return;
        StopAnimAfter("ProfTeleStill", end);
        var hangingUp = Anim("ProfTele", after: end);
        ObjAfter("LOC03Telefon2", false, end, 1);
        ObjAfter("LOC03Telefon1", true, end, 1);
        Anim("ProfSpeak", -1, hangingUp);
        StateAfter(Timmy, BarkRight, end);
        Sound(@"wav\timmy\bellen1.wav", end);
        Sound(@"wav\timmy\bellen1.wav", end, 4);
        Sound(@"wav\timmy\bellen1.wav", end, 8);
        SetVar(Vars.TelephoneOffHook, 0);
        SetVar(Vars.ProfessorCallOver, 1);
    }

    /// <summary>
    /// Quest 5, step 3: after Uncle Quentin's last word everyone goes to the kitchen (step 4);
    /// after the others' remarks the talks move on; after the telling off, Anne comes in crying.
    /// </summary>
    void AfterTellingOff(TaskHandle end)
    {
        if (CurrentSpeaker == Who.Quentin)
        {
            WalkAfter(Hero, 250, 394, end);
            WalkAfter(Dick, 277, 394, end);
            WalkAfter(Julian, 277, 394, end, 3);
            WalkAfter(Anne, 277, 394, end, 1);
            WalkAfter(Timmy, 277, 394, end, 6);
            QuentinWritesAgain(end);
            Episode = 4;
            SetVar(Vars.AnneTalk, 0);
            SetVar(Vars.DickTalk, 0);
            SetVar(Vars.JulianTalk, 0);
            ReopenMenu("loc05");
            PointerAfter(false, end, x: 100, y: 200);
            PointerAfter(true, end, 16, 100, 200);
            ChangeLocation("loc05", end, 1);
        }
        else if (Var(Vars.TalkingToAnneInStudy) != 0)
        {
            SetVar(Vars.AnneStudyTalk, 1);
            StateAfter(Hero, StandDownRight, end);
            SetVar(Vars.TalkingToAnneInStudy, 0);
        }
        else if (Var(Vars.TalkingToJulianInStudy) != 0)
        {
            SetVar(Vars.JulianStudyTalk, 1);
            StateAfter(Hero, StandDownRight, end);
            SetVar(Vars.TalkingToJulianInStudy, 0);
        }
        else if (Var(Vars.TalkingToDickInStudy) != 0)
        {
            SetVar(Vars.DickStudyTalk, 1);
            SetVar(Vars.TalkingToDickInStudy, 0);
            StateAfter(Hero, StandDownRight, end);
        }
        else if (CurrentSpeaker == Who.Anne)
            AnneComesInCrying(end);
    }

    /// <summary>Uncle Quentin writes on; the door opens and Anne comes in crying, the others with her.</summary>
    void AnneComesInCrying(TaskHandle end)
    {
        QuentinWritesAgain(end);
        ObjAfter("LOC03CloseDoorObj", false, end);
        var opened = Anim("DoOpenDoor", after: end);
        ObjAfter("LOC03OpenDoorObj", true, opened);
        Place(Anne, 10, 290, StandRight);
        PointerAfter(false, end, x: 200, y: 300);
        PointerAfter(true, end, 14, 200, 300);
        ObjAfter("LOC03Anne", true, end, 10);
        ObjAfter("LOC03Julian", true, end, 10);
        ObjAfter("LOC03Dick", true, end, 10);
        WalkAfter(Anne, 250, 410, end, 10);
        State(Anne, CryRight);
        Sound(@"wav\deutsch\anne\an130203.wav", end);
        ShowParty();
    }

    /// <summary>Quest 7, step 3: after the remark about the model ship George is sent out (step 4).</summary>
    void SentOutAfterTheShip(TaskHandle end)
    {
        PointerAfter(false, end, x: 300, y: 400);
        WalkAfter(Hero, 228, 435, end, 10);
        var reached = State(Hero, TakeMidUpLeft);
        Sound(@"wav\fx\hotueauf.wav", reached, 20);
        PointerAfter(false, reached, x: 100, y: 200);
        var opened = Anim("DoOpenDoor", after: reached, delay: 20, endObject: "LOC03CloseDoorObj");
        PointerAfter(true, opened, 4, 100, 200);
        Episode = 4;
        QuentinReadsOn(end);
        ChangeLocation("loc04", opened);
    }

    /// <summary>
    /// Quest 7, step 4: Uncle Quentin's patience runs down while George is in the study, and he
    /// grumbles when it runs out. Once the hero has stepped into the room, the path by the door
    /// is opened (quest 5, steps 3-4) or closed.
    /// </summary>
    /// <remarks>The original's FUN_0042a738.</remarks>
    public override void EveryTick()
    {
        if (At(7, 4))
        {
            if (Var(Vars.GrumbleTimer) == 0)
            {
                SetVar(Vars.GrumbleTimer, 700);
                Chain = null;
                Say(Who.Quentin, "ARBEIT0032", 9, last: true, delay: 6);
            }
            else
                SetVar(Vars.GrumbleTimer, Var(Vars.GrumbleTimer) - 1);
        }
        if (Hero.Y > 410.0 && Var(Vars.StudyPathPending) != 0)
        {
            if (Quest == 5)
            {
                SetVar(Vars.StudyPathPending, 0);
                PathEnable(0, Episode is 3 or 4);
            }
            else
                PathEnable(0, false);
        }
    }
}
