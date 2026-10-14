using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The guest room (loc02), where the professor stays. The wardrobe holds his coat with the
/// suitcase key in its pocket (quest 5); under the carpet lies the diary (quest 5, steps 6 and 7).
/// </summary>
public sealed class Bedroom : StoryScript
{
    /// <inheritdoc/>
    public override string Location => "loc02";

    /// <summary>
    /// Music for the story step, the story step itself moved on, then the door, carpet, wardrobe,
    /// diary, coat pocket and sleeping professor as the story has left them; the hero comes in.
    /// </summary>
    /// <remarks>The original's FUN_00423b9a.</remarks>
    protected override void Setup()
    {
        Music("");
        Music(Episode < 5 && Quest == 5 ? @"wav\atmo\gästezimmer.wav" : @"wav\atmo\hausruhi.wav");
        SetVar(Vars.LookingUnderBed, 0);
        MoveStoryOn();
        SetUpDoor();
        SetUpCarpet();
        SetUpWardrobe();
        SetUpDiary();
        SetUpCoatPocket();
        SetUpProfessor();
        Place(Hero, 94, 583, StandDownLeft);
        Show(Hero);
        ShowParty();
        WalkAndFace(290, 593, StandDownRight);
        if (Has(Item.SuitcaseKey))
        {
            // The key has been found: the wardrobe can no longer be used.
            SetVar(Vars.GuestRoomPlacingKey, 1);
            Obj("LOC02OpenSchrankOhneMantelObj", false);
            Obj("LOC02DoOpenSchrankClick", false);
            Obj("LOC02OpenSchrankClick", false);
            Obj("LOC02OpenSchrankObj", false);
            Obj("LOC02Tasche", false);
            StopAnim("DoOpenSchrank");
            StopAnim("DoCloseSchrank");
        }
    }

    /// <summary>The story steps that move on when the guest room is entered (quest 5).</summary>
    /// <remarks>The original's FUN_004246f2.</remarks>
    void MoveStoryOn()
    {
        if (At(5, 1))
            Episode = 2;
        if (At(5, 2))
        {
            SetVar(Vars.DiaryState, 1);
            Obj("LOC02UntermBett", false);
            SetVar(Vars.CarpetState, 2);
            SetVar(Vars.WardrobeState, 1);
            SetVar(Vars.DiaryState, 2);
        }
        else if (At(5, 5))
        {
            // The professor is up and about: the room is as he left it.
            SetVar(Vars.DiaryState, 1);
            SetVar(Vars.CarpetState, 0);
            SetVar(Vars.WardrobeState, 2);
            SetVar(Vars.SuitcaseKeyTaken, 1);
            SetVar(Vars.ProfessorUp, 1);
            SetVar(Vars.AnneTalk, 3);
            SetVar(Vars.DickTalk, 3);
            SetVar(Vars.JulianTalk, 3);
            Episode = 6;
        }
        else if (At(5, 6))
            Episode = 7;
    }

    /// <summary>The door closes behind the hero.</summary>
    /// <remarks>The original's FUN_00423d25.</remarks>
    void SetUpDoor()
    {
        Depth(@"bmp\obj\locs\loc02\l02t0104.brg", 100);
        Depth(@"bmp\obj\locs\loc02\l02t01.brg", 100);
        MoveObj("LOC02OpenDoorObj", 0, 182);
        Obj("LOC02OpenDoorObj", false);
        StopAnim("DoOpenDoor");
        if (Var(Vars.GuestRoomDoorAvailable) == 1)
        {
            Anim("DoCloseDoor");
            Sound(@"wav\fx\hotuezu.wav", delay: 3);
        }
    }

    /// <summary>The carpet: flat (clickable), rolled back, or flat for good.</summary>
    /// <remarks>The original's FUN_00423dc6.</remarks>
    void SetUpCarpet()
    {
        MoveObj("LOC02Carpet0", Var(Vars.CarpetX) - 50, Var(Vars.CarpetY));
        MoveObj("LOC02Carpet1", Var(Vars.CarpetX), Var(Vars.CarpetY));
        Obj("LOC02Carpet1Click", false);
        Obj("LOC02Carpet1", false);
        Obj("LOC02Carpet0", false);
        if (Var(Vars.CarpetAvailable) != 1)
            return;
        switch (Var(Vars.CarpetState))
        {
            case 0:
                Obj("LOC02Carpet1", true);
                Obj("LOC02Carpet1Click", true);
                break;
            case 1:
                Obj("LOC02Carpet0", true);
                break;
            case 2:
                Obj("LOC02Carpet1", true);
                Obj("LOC02Carpet1Click", false);
                break;
        }
    }

    /// <summary>The wardrobe: shut, open, or open without the coat; it stays shut late on quest 5.</summary>
    /// <remarks>The original's FUN_004240cf.</remarks>
    void SetUpWardrobe()
    {
        Depth(@"bmp\obj\locs\loc02\l02t0204.brg", 5000);
        Depth(@"bmp\obj\locs\loc02\l02t02.brg", 4000);
        AnimPos("DoCloseSchrank", 216, 229);
        AnimPos("DoOpenSchrank", 216, 229);
        MoveObj("LOC02OpenSchrankObj", 216, 229);
        MoveObj("LOC02OpenSchrankClick", 220, 236);
        StopAnim("DoOpenSchrank");
        StopAnim("DoCloseSchrank");
        Obj("LOC02OpenSchrankOhneMantelObj", false);
        if (Var(Vars.WardrobeAvailable) == 1)
        {
            switch (Var(Vars.WardrobeState))
            {
                case 0:
                    Obj("LOC02OpenSchrankObj", false);
                    Obj("LOC02OpenSchrankClick", false);
                    Obj("LOC02DoOpenSchrankClick", true);
                    break;
                case 1:
                    Obj("LOC02OpenSchrankObj", true);
                    Obj("LOC02OpenSchrankClick", true);
                    Obj("LOC02DoOpenSchrankClick", false);
                    break;
                case 2:
                    Obj("LOC02OpenSchrankObj", false);
                    Obj("LOC02OpenSchrankOhneMantelObj", true);
                    Obj("LOC02OpenSchrankClick", true);
                    Obj("LOC02DoOpenSchrankClick", false);
                    break;
            }
        }
        if (Quest > 4 && Episode > 5)
        {
            Obj("LOC02OpenSchrankOhneMantelObj", false);
            Obj("LOC02DoOpenSchrankClick", false);
            Obj("LOC02OpenSchrankClick", false);
        }
    }

    /// <summary>The diary: under the carpet (until taken), or on the shelf.</summary>
    /// <remarks>The original's FUN_00423ebd.</remarks>
    void SetUpDiary()
    {
        SetVar(Vars.GuestRoomPlacingDiary, 1);
        MoveObj("LOC02Buch", 351, 523);
        Obj("LOC02Buch", false);
        SetVar(Vars.GuestRoomPlacingDiary, 0);
        if (Var(Vars.DiaryAvailable) != 1)
            return;
        if (Var(Vars.DiaryState) == 0)
            Obj("LOC02Buch", true);
        else if (Var(Vars.DiaryState) == 2)
        {
            MoveObj("LOC02Buch", 490, 370);
            Obj("LOC02Buch", true);
        }
    }

    /// <summary>The coat pocket with the key, while the key has not been taken.</summary>
    /// <remarks>The original's FUN_00423ff6.</remarks>
    void SetUpCoatPocket()
    {
        SetVar(Vars.GuestRoomPlacingKey, 1);
        Obj("LOC02Tasche", false);
        if (Var(Vars.SuitcaseKeyAvailable) == 1 && Var(Vars.SuitcaseKeyTaken) == 0)
            Obj("LOC02Tasche", true);
        SetVar(Vars.GuestRoomPlacingKey, 0);
    }

    /// <summary>The professor sleeps in the bed until he is up.</summary>
    /// <remarks>The original's FUN_00423f79.</remarks>
    void SetUpProfessor()
    {
        AnimPos("ProfSleep", 390, 300);
        StopAnim("ProfSleep");
        Depth(@"bmp\obj\prof\prsl\prsl.brg", 4000);
        if (Var(Vars.ProfessorUp) == 0)
            Anim("ProfSleep", -1);
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>The door, the wardrobe, the window, the carpet, under the bed, the diary and the coat pocket.</summary>
    /// <remarks>The original's FUN_00423447.</remarks>
    public override bool Click(string name)
    {
        if (Same(name, "LOC02Sessel"))
            Walk(Hero, 378, 583);
        else if (Same(name, "LOC02ExitToLOC04"))
        {
            Walk(Hero, 195, 592);
            Sound(@"wav\fx\hotueauf.wav", delay: 3);
            ChangeLocation("loc04");
        }
        else if (Same(name, "LOC02OpenSchrankClick"))
        {
            Walk(Hero, 351, 488);
            if (Quest < 6 && !(Quest == 5 && (Episode == 2 || Episode >= 5)))
                CloseWardrobe();
        }
        else if (Same(name, "LOC02DoOpenSchrankClick"))
            OpenWardrobe();
        else if (Same(name, "LOC02Fenster"))
        {
            WalkAndFace(350, 444, StandUpLeft);
            SetVar(Vars.V64f25c, 0);
        }
        else if (Same(name, "LOC02Carpet1Click"))
        {
            if (Episode > 5 && Quest == 5)
                RollBackCarpet();
        }
        else if (Same(name, "LOC02UntermBett"))
        {
            Walk(Hero, 380, 540);
            SetVar(Vars.LookingUnderBed, 1);
            ObjAfter("LOC02UntermBett", false, null);
        }
        else if (Same(name, "LOC02Buch"))
        {
            if (At(5, 2))
                WalkAndFace(335, 540, StandRight);
            else
            {
                Walk(Hero, 290, 540);
                ObjAfter("LOC02Buch", false, null, 4);
            }
        }
        else if (Same(name, "LOC02Tasche"))
        {
            Walk(Hero, 300, 488);
            SetVar(Vars.GuestRoomPlacingKey, 0);
            ObjAfter("LOC02Tasche", false, null, 4);
            Sound(@"wav\fx\rmantel.wav", delay: 3);
        }
        return true;
    }

    /// <summary>Shuts the wardrobe (and hides the coat pocket with it).</summary>
    void CloseWardrobe()
    {
        Sound(@"wav\fx\sctuezu.wav", delay: 3);
        var shut = ObjAfter("LOC02OpenSchrankClick", false, null);
        shut = ObjAfter("LOC02DoOpenSchrankClick", true, shut);
        if (Var(Vars.SuitcaseKeyTaken) == 0)
        {
            SetVar(Vars.GuestRoomPlacingKey, 1);
            ObjAfter("LOC02Tasche", false, shut);
        }
        SetVar(Vars.WardrobeState, 0);
    }

    /// <summary>Opens the wardrobe: before quest 5 with the coat (and its pocket), later without it.</summary>
    void OpenWardrobe()
    {
        Walk(Hero, 300, 488);
        var open = ObjAfter("LOC02OpenSchrankClick", true, null);
        open = ObjAfter("LOC02DoOpenSchrankClick", false, open);
        if (Episode < 7 && Quest < 5)
        {
            if (Var(Vars.SuitcaseKeyTaken) == 0)
            {
                SetVar(Vars.GuestRoomPlacingKey, 1);
                ObjAfter("LOC02Tasche", true, open);
            }
            SetVar(Vars.WardrobeState, 1);
        }
        else
        {
            StateAfter(Hero, StandUpLeft, open);
            SetVar(Vars.WardrobeState, 2);
        }
    }

    /// <summary>The hero kneels and rolls the carpet back: the diary lies underneath; George remarks on it.</summary>
    void RollBackCarpet()
    {
        Walk(Hero, 232, 538);
        Chain = State(Hero, KneelRight);
        Sound(@"wav\fx\teppholz.wav", Chain, 3);
        Say(Who.George, "GAESTE0015-02", 4, last: true, delay: 2);
        ObjAfter("LOC02Carpet1", false, null, 6);
        var rolled = ObjAfter("LOC02Carpet0", true, null, 6);
        rolled = ObjAfter("LOC02Carpet1Click", false, rolled);
        SetVar(Vars.CarpetState, 1);
        ObjAfter("LOC02Buch", true, rolled, 1);
        SetVar(Vars.DiaryState, 0);
        // (sic) The original switches the hall's object here, not the guest room's.
        ObjAfter("LOC04UntermBett", false, rolled);
    }

    /// <summary>
    /// Taking the diary or the key, and looking under the bed (its object switched off). Taking the
    /// key frightens the hero: the wardrobe shuts, and they run out to the hall.
    /// </summary>
    /// <remarks>The original's FUN_00424282.</remarks>
    public override void ObjectChanged(GameWorld world, string name, bool on)
    {
        if (on)
            return;
        if (Same(name, "LOC02Buch") && Var(Vars.GuestRoomPlacingDiary) == 0)
        {
            Give(Item.Diary);
            SetVar(Vars.DiaryState, 1);
            SetStep(6, 1);
        }
        else if (Same(name, "LOC02Tasche") && Var(Vars.GuestRoomPlacingKey) == 0)
            TakeKey();
        else if (Same(name, "LOC02UntermBett") && Var(Vars.LookingUnderBed) != 0)
            LookUnderBed();
    }

    /// <summary>The key goes into the rucksack; the wardrobe slams shut, Anne calls, and the hero hurries out.</summary>
    void TakeKey()
    {
        Give(Item.SuitcaseKey);
        SetVar(Vars.WardrobeState, 0);
        Walk(Hero, 320, 510);
        ObjAfter("LOC02OpenSchrankObj", false, null, 5);
        var shut = Anim("DoCloseSchrank", delay: 5);
        Sound(@"wav\fx\sctuerzu.wav", shut, 5);
        ObjAfter("LOC02OpenSchrankClick", false, shut);
        ObjAfter("LOC02DoOpenSchrankClick", true, shut);
        State(Hero, FrightenedDownRight);
        WalkAfter(Hero, 195, 592, shut, 10);
        Say(Who.Anne, "GAESTE0011-02", 1, after: shut);
        var reached = State(Hero, TakeMidLeft);
        var open = Anim("DoOpenDoor", after: reached, delay: 5, endObject: "LOC02OpenDoorObj", endObjectOn: true);
        ChangeLocation("loc04", open);
        SetVar(Vars.AnneHallTalk, 2);
        SetVar(Vars.DickHallTalk, 2);
        SetVar(Vars.JulianHallTalk, 2);
    }

    /// <summary>George remarks on looking under the bed (a different remark each time); the bed can be looked under again.</summary>
    void LookUnderBed()
    {
        switch (Var(Vars.UnderBedTalk))
        {
            case 0:
                SetVar(Vars.UnderBedTalk, 1);
                Say(Who.George, "GAESTE0015-02", 1, last: true);
                break;
            case 1:
                Say(Who.George, "GAESTE0015-02", 2, last: true);
                SetVar(Vars.UnderBedTalk, 3);
                break;
            case 2: // an item was used under the bed
                Say(Who.George, "GAESTE0015-02", 3, last: true);
                SetVar(Vars.UnderBedTalk, Var(Vars.UnderBedTalkBefore));
                break;
            case 3:
                Say(Who.George, "GAESTE0015-02", 5, last: true);
                break;
        }
        SetVar(Vars.LookingUnderBed, 0);
        ObjAfter("LOC02UntermBett", true, null);
    }
}
