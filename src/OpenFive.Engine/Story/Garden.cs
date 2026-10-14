using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The garden in front of Kirrin Cottage (loc06): the game starts here with Aunt Fanny greeting
/// Dick, Julian and Anne. Exits lead into the house, the shed and down to the beach.
/// </summary>
public sealed class Garden : StoryScript
{
    public override string Location => "loc06";

    const string FannyScene = "Fass0";
    const int TakeLow = 21, LookUpRight = 76, TakeDown = 45, SpeakDownRight = 28;

    protected override void Setup()
    {
        Obj("LOC06OpenDoor2", true);
        Obj("LOC06ClosedDoor1Obj", true);
        Obj("LOC06ExitToLOC03", false);
        // The bucket stands by the barrel until someone takes it.
        MoveObj("LOC06Eimer", 96, 395);
        Obj("LOC06Eimer", Var(Vars.GardenBucketTaken) == 0);
        Obj("LOC06OpenDoor1Obj", false);
        // The barrel by the shed may have been moved.
        if (Var(Vars.BarrelMoved) == 1)
            Obj("LOC06Fass0", false);
        Obj("LOC06ClosedWindowObj", Var(Vars.LivingRoomWindow) == 0);
        Obj("LOC06OpenWindowObj", Var(Vars.LivingRoomWindow) == 1);
        AnimPos("doBirds", 0, 170);
        Anim("doBirds", -1);
    }

    void HideOthers()
    {
        Hide(Anne);
        Hide(Julian);
        Hide(Hero == Dick ? George : Dick);
    }

    protected override void Arrive()
    {
        switch (From)
        {
            case "loc04": // out of the front door
            {
                Place(Hero, 463, 419, StandDownRight);
                ShowParty();
                Walk(Hero, 487, 439);
                State(Hero, StandDownRight);
                ObjAfter("LOC06OpenDoor2", false, null, 1);
                var closed = Anim("DoCloseDoor2", delay: 8);
                Sound(@"wav\fx\htuerzu.wav", delay: 8);
                ObjAfter("LOC06OpenDoor2", true, closed);
                StartScenes();
                HideOthers();
                break;
            }
            case "loc06": // the opening scene has ended; the others have gone in
                Place(Hero, 545, 490, StandDownRight);
                Walk(Hero, 545, 490);
                State(Hero, StandDownRight);
                ShowParty();
                HideOthers();
                ObjAfter("LOC06OpenDoor2", true, null);
                if (At(7, 5))
                    StartScenes();
                break;
            case "loc07": // out of the shed
            {
                ObjAfter("LOC06ClosedDoor1Obj", false, null);
                ObjAfter("LOC06OpenDoor1Obj", false, null);
                var closed = Anim("DoCloseDoor1Schuppen", endObject: "LOC06ClosedDoor1Obj", endObjectOn: true);
                Sound(@"wav\fx\stuerzu.wav", closed);
                Place(Hero, 140, 422, StandDownRight);
                Walk(Hero, 159, 473);
                State(Hero, StandDownRight);
                StartScenes();
                ShowParty();
                HideOthers();
                break;
            }
            case "loc09": // up from the beach
                Place(Hero, 86, 340, StandDownRight);
                Walk(Hero, 175, 476);
                State(Hero, StandDownRight);
                StartScenes();
                ShowParty();
                HideOthers();
                break;
            default: // a new game
                Place(Hero, 660, 450, StandDownRight);
                StartScenes();
                ObjAfter("LOC06OpenDoor2", false, null);
                ShowParty();
                Hide(Hero == Dick ? George : Dick);
                break;
        }
    }

    void StartScenes()
    {
        if (At(1, 1))
        {
            // Aunt Fanny waits at the door; Dick, Julian and Anne arrive.
            PointerOff();
            Depth(@"bmp\obj\fanny\fast\fast.brg", 2000);
            Depth(@"bmp\obj\fanny\fasp\fasp.brg", 2000);
            FannyStands();
            Walk(Hero, 545, 490);
            Place(Anne, 779, 490, StandLeft);
            Place(Julian, 799, 599, StandLeft);
            Walk(Anne, 690, 490);
            Walk(Julian, 647, 523);
            State(Anne, StandUpLeft);
            State(Julian, StandUpLeft);
            State(Hero, StandDownRight);
            Chain = State(Hero, StandDownRight);
            Say(Who.Fanny, "ANFANG", 1);
            Say(Who.Anne, "ANFANG", 1);
            Say(Who.Fanny, "ANFANG", 2);
            Say(Who.Julian, "ANFANG", 2);
            Say(Who.Fanny, "ANFANG", 3);
            Say(Who.Julian, "ANFANG", 3);
            Say(Who.Fanny, "ANFANG", 4);
            Say(Who.Fanny, "ANFANG", 5, last: true);
        }
        if (At(7, 5))
        {
            // Uncle Quentin calls from the study.
            Obj("LOC06ExitToLOC03", true);
            Sound(@"wav\english\quentin\qu300104.wav");
        }
    }

    /// <summary>Aunt Fanny's walk to the door: picture, position, size and start time.</summary>
    static readonly (int Frame, int X, int Y, double Scale, int Delay)[] FannyWalk =
    {
        (1, 568, 330, 0.69, 4), (2, 552, 329, 0.69, 4), (3, 540, 328, 0.69, 6), (4, 534, 327, 0.68, 8),
        (5, 528, 326, 0.67, 10), (6, 522, 325, 0.66, 12), (7, 516, 324, 0.65, 14), (8, 510, 323, 0.64, 16),
        (9, 504, 322, 0.63, 18), (1, 498, 321, 0.62, 12), (2, 492, 320, 0.61, 13), (3, 486, 319, 0.60, 14),
        (4, 480, 318, 0.59, 15), (5, 474, 317, 0.58, 16), (6, 468, 316, 0.57, 17), (7, 462, 315, 0.56, 18),
        (8, 456, 314, 0.54, 19), (9, 450, 313, 0.53, 20),
    };

    void FannyStands(TaskHandle? after = null, int delay = 0)
    {
        AnimPos("FannyStandLeft", 568, 330, 0.35);
        Anim("FannyStandLeft", -1, after, delay);
    }

    // ---- conversation hooks ------------------------------------------------------------------

    public override void LineStarting(GameWorld world, int speaker)
    {
        if (World.Episode != 1 || World.Quest >= 3)
            return;
        State(Anne, StandUpLeft);
        State(Julian, StandUpLeft);
        State(Hero, StandDownRight);
        StopAnim("FannyStandLeft");
        StopAnim("FannyStandLeftTalk");
        if (speaker == Who.Fanny)
        {
            AnimPos("FannyStandLeftTalk", 572, 333, 0.69);
            Anim("FannyStandLeftTalk", -1);
        }
        else
        {
            FannyStands();
            if (speaker == Who.Dick)
                State(Hero, SpeakDownRight);
        }
    }

    public override void LineQueued(GameWorld world, TaskHandle end)
    {
        if (World.Episode != 1 || World.Quest >= 3)
            return;
        StopAnimAfter("FannyStandLeftTalk", end);
        FannyStands(end);
    }

    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (At(1, 1))
        {
            // Dick may ask Aunt Fanny some questions.
            SetStep(2, 1);
            StopAnimAfter("FannyStandLeftTalk", end);
            FannyStands(end);
            Chain = end;
            Ask(Who.Dick, FannyScene, 1);
        }
        else if (At(2, 1))
        {
            // Aunt Fanny takes Julian and Anne into the house.
            SetStep(2, 2);
            PointerAfter(false, end, x: 400, y: 300);
            StopAnimAfter("FannyStandLeftTalk", end);
            FannyStands(end);
            StopAnimAfter("FannyStandLeft", end, 1);
            AnimPos("FannyStandLeft02", 568, 330, 0.35);
            Anim("FannyStandLeft02", -1, end, 1);
            StopAnimAfter("FannyStandLeft02", end, 2);
            AnimPos("FannyStandLeft03", 568, 330, 0.35);
            Anim("FannyStandLeft03", -1, end, 2);
            Depth(@"bmp\obj\fanny\fawl03\fawl03.brg", 10);
            WalkAfter(Anne, 450, 320, end);
            WalkAfter(Julian, 450, 320, end, 4);
            // Aunt Fanny walks back to the door, one picture at a time.
            StopAnimAfter("FannyStandLeft03", end, 4);
            string previous = "";
            foreach (var (frame, x, y, scale, delay) in FannyWalk)
            {
                string name = "FannyStandWalkLeft" + frame;
                if (previous.Length > 0)
                    StopAnimAfter(previous, end, delay);
                AnimPos(name, x, y, scale);
                Anim(name, -1, end, delay);
                previous = name;
            }
            ChangeLocation("loc06", end, 10);
        }
        else if (At(7, 5))
        {
            PointerAfter(false, end, x: 300, y: 400);
            ChangeLocation("loc03", end);
        }
    }

    protected override void AfterChoice(string scene, int line, int option)
    {
        if (scene != FannyScene || Var(ChoicePending) == 0)
            return;
        switch (option)
        {
            case 1:
                Say(Who.Fanny, FannyScene, 1);
                break;
            case 2:
                Say(Who.Fanny, FannyScene, 2);
                Say(Who.Julian, FannyScene, 1);
                Say(Who.Fanny, FannyScene, 22);
                break;
            case 3:
                // "Where is George?" ends the questions.
                Say(Who.Fanny, FannyScene, 3);
                CloseMenu();
                break;
        }
        RemoveChosenOption();
        if (MenuExhausted())
            Say(Who.Fanny, FannyScene, 0, last: true);
        else
            Ask(Who.Dick, FannyScene, 1);
        SetVar(ChoicePending, 0);
    }

    // ---- clicks ------------------------------------------------------------------------------

    public override bool Click(string name)
    {
        switch (name)
        {
            case "LOC06ExitToLOC07": // the shed
            {
                Walk(Hero, 140, 422);
                var take = State(Hero, TakeLow);
                Sound(@"wav\fx\stuerauf.wav", take, 3);
                ObjAfter("LOC06ClosedDoor1Obj", false, take, 3);
                var open = Anim("DoOpenDoorSchuppen", after: take, endObject: "LOC06OpenDoor1Obj", endObjectOn: true);
                ChangeLocation("loc07", open, 4);
                return true;
            }
            case "LOC06Vorgarten":
                WalkAndFace(631, 457, StandUpRight);
                return true;
            case "LOC06Angestelltenbau":
                WalkAndFace(781, 477, StandUpLeft);
                return true;
            case "LOC06Fenster4":
                WalkAndFace(528, 438, StandUpRight);
                return true;
            case "LOC06Fenster3":
                WalkAndFace(696, 473, StandUpLeft);
                return true;
            case "LOC06Fenster2":
                WalkAndFace(423, 486, StandUpLeft);
                return true;
            case "LOC06Fenster1":
                WalkAndFace(44, 395, LookUpRight);
                return true;
            case "LOC06Blume":
                WalkAndFace(75, 509, StandDownLeft);
                return true;
            case "LOC06Faß":
                WalkAndFace(110, 441, StandUpLeft);
                return true;
            case "LOC06Haus":
                WalkAndFace(539, 475, StandUpLeft);
                return true;
            case "LOC06ExitToLOC09": // down to the beach
                Walk(Hero, 86, 340);
                ChangeLocation("loc09");
                return true;
            case "LOC06ExitToLOC04": // the front door
            {
                Walk(Hero, 467, 395);
                var take = State(Hero, TakeLow);
                PointerAfter(false, take, x: 400, y: 300);
                ObjAfter("LOC06OpenDoor2", false, take, 5);
                Sound(@"wav\fx\htuerauf.wav", take, 3);
                var open = Anim("DoOpenDoor2", after: take, delay: 5);
                ChangeLocation("loc04", open, 6);
                return true;
            }
            case "LOC06ExitToLOC03": // the study window
                Walk(Hero, 250, 424);
                Chain = State(Hero, StandUpRight);
                if (At(7, 5))
                    Say(HeroSpeaker, "AUSSEN0033", 1, last: true);
                return true;
            case "LOC06FensterToLOC03":
                WalkAndFace(250, 424, StandUpRight);
                return true;
            case "LOC06Eimer": // the bucket
            {
                Walk(Hero, 129, 423);
                var take = State(Hero, TakeDown);
                ObjAfter("LOC06Eimer", false, take, 4);
                Sound(@"wav\fx\eimer2.wav", take, 3);
                return true;
            }
        }
        return false;
    }

    public override void ObjectChanged(GameWorld world, string name, bool on)
    {
        if (!on && name == "LOC06Eimer")
        {
            Give(Item.Bucket);
            SetVar(Vars.GardenBucketTaken, 1);
        }
    }
}
