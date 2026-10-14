using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// What happens when an item from the rucksack is used on something, in any location (the
/// original's FUN_0041cb11), and when an item is clicked in the open rucksack (FUN_0041c126).
/// </summary>
public sealed class ItemUses : StoryScript
{
    readonly string _location;

    /// <summary>Creates the rules for the session's current location.</summary>
    public ItemUses(GameSession session)
    {
        _location = session.World.Scene?.Name ?? "";
        AttachTo(session);
    }

    /// <inheritdoc/>
    public override string Location => _location;

    /// <summary>
    /// Uses <paramref name="item"/> on <paramref name="target"/> (an object, character or another
    /// item). Returns false when nothing is done with it here (the location may still react).
    /// </summary>
    public bool Use(int item, string target)
    {
        // A few targets react the same to every item.
        if (Same(target, "LOC02UntermBett"))
        {
            LookUnderTheBedWith();
            return true;
        }
        if (Same(target, Inventory.ObjectName(Item.ClosedTin)))
        {
            Chain = null;
            if (Var(Vars.TinRemarked) == 0)
            {
                SetVar(Vars.TinRemarked, 1);
                Say(Who.Dick, "KUECHE0027", 5, last: true);
            }
            else
                Say(Who.Dick, "KUECHE0027", 6, last: true);
            return true;
        }
        if (Same(target, Inventory.ObjectName(Item.Plate)))
        {
            Chain = null;
            if (At(8, 3))
            {
                PointerOff();
                Say(Who.Dick, "RUINE0040", 24, last: true);
            }
            return true;
        }
        // Down the well shaft (quest 8, step 4) only the bucket and the stones are any use.
        if (At(8, 4) && item is not (Item.Bucket or Item.Stone1 or Item.Stone2 or Item.Stone3))
        {
            if (LocationId == 19)
                NothingUsefulDownTheWell();
            return false;
        }
        // At the treasure door (quest 9) only the axe is any use.
        if (At(9, 1) && LocationId == 24 && item != Item.Axe)
        {
            Chain = null;
            Say(Who.Dick, "LABYRINTH0046", 6, last: true);
            return false;
        }

        if (Same(target, "LOC16Schloss"))
            SetVar(Vars.CupboardLockTried, 1);
        return item switch
        {
            Item.Teapot => UseTeapot(target),
            Item.Bucket => UseBucket(target),
            Item.ClosedTin => UseClosedTin(target),
            Item.OpenKnife or Item.ClosedKnife => UseKnife(target),
            Item.BrokenKnife or Item.Screwdriver => UseScrewdriver(item, target),
            Item.Rope => UseRope(target),
            Item.Stone1 or Item.Stone2 or Item.Stone3 => UseStone(target),
            Item.Lantern => Combine(target, Item.Candle),
            Item.Candle => Combine(target, Item.Lantern),
            Item.Firewood => UseFirewood(target),
            Item.OliveOil => UseOliveOil(target),
            Item.Axe => UseAxe(target),
            Item.Torch => UseTorch(target),
            Item.Sandwich => UseSandwich(target),
            Item.Newspaper => UseNewspaper(target),
            Item.FullOilCan => UseFullOilCan(target),
            Item.EmptyOilCan => UseEmptyOilCan(target),
            Item.SuitcaseKey => UseSuitcaseKey(target),
            Item.Key => UseKey(target),
            _ => false,
        };
    }

    /// <summary>
    /// An item clicked in the open rucksack: the treasure map is looked at; the closed tin gets
    /// a remark away from the kitchen (quest 7). False when the item is simply taken in hand.
    /// </summary>
    /// <remarks>The original's FUN_0041c126.</remarks>
    public bool Click(int item)
    {
        if (item == Item.TreasureMap)
        {
            Session.CloseInventory();
            ChangeLocation("locKarte", delay: 5);
            return true;
        }
        if (item == Item.ClosedTin && At(7, 1) && LocationId != 5)
        {
            Say(Who.Dick, "KAJUETE0024", 7, last: true);
            return true;
        }
        return false;
    }

    // ---- rules for every item ------------------------------------------------------------------

    /// <summary>Kneels by the guest-room bed with an item; George remarks on it (see <c>Bedroom</c>).</summary>
    void LookUnderTheBedWith()
    {
        SetVar(Vars.UnderBedTalkBefore, Var(Vars.UnderBedTalk));
        SetVar(Vars.UnderBedTalk, 2);
        Walk(Hero, 380, 540);
        var kneeling = State(Hero, KneelingRight);
        SetVar(Vars.LookingUnderBed, 1);
        ObjAfter("LOC02UntermBett", false, kneeling);
    }

    /// <summary>Dick's remarks on trying other things down the well shaft (three in turn).</summary>
    void NothingUsefulDownTheWell()
    {
        Chain = null;
        PointerOff();
        switch (Var(Vars.WellShaftItemTalk))
        {
            case 0:
                IncAfter(Vars.WellShaftItemTalk, null);
                Say(Who.Dick, "BRUNNEN0042", 6, last: true);
                break;
            case 1:
                IncAfter(Vars.WellShaftItemTalk, null);
                Say(Who.Dick, "BRUNNEN0042", 7, last: true);
                break;
            case 2:
                Say(Who.Dick, "BRUNNEN0042", 8, last: true);
                break;
        }
    }

    // ---- the items -------------------------------------------------------------------------------

    /// <summary>George with the teapot: puts out the study fire, or pours Uncle Quentin's tea.</summary>
    bool UseTeapot(string target)
    {
        if (Hero != George)
            return false;
        if (Same(target, "LOC03Kamin"))
        {
            Walk(Hero, 380, 368);
            var pouring = State(Hero, TeapotLeft);
            StopAnimAfter("FeuerGross", pouring, 5);
            StopAnimAfter("FeuerMittel", pouring, 5);
            Anim("FeuerKlein", -1, pouring, 5);
            SetVar(Vars.StudyFire, 0);
            if (At(7, 4))
                SetVar(Vars.StudyFireOut, 1);
            return true;
        }
        if (Same(target, "LOC03Tasse"))
        {
            Walk(Hero, 450, 436);
            SetVar(Vars.V64d464, 2000);
            Chain = State(Hero, TeapotRight);
            PointerAfter(false, Chain, x: 300, y: 400);
            Say(Who.Quentin, "ARBEIT0032", 10, last: true);
            return true;
        }
        return false;
    }

    /// <summary>The bucket on the rope in the well shaft: Timmy gets into it (quest 8, step 4).</summary>
    bool UseBucket(string target)
    {
        if (!Same(target, "LOC19SeilKlick"))
            return false;
        if (At(8, 4) && Var(Vars.TimmyInBucket) == 0)
        {
            PointerOff();
            SetVar(Vars.TimmyInBucket, 1);
            Walk(Hero, 257, 522);
            var reached = Chain = State(Hero, TakeMidRight);
            Take(Item.Bucket);
            Walk(Timmy, 194, 550);
            var inBucket = State(Timmy, InBucketRight);
            StopAnimAfter("Seil", inBucket);
            SetText(StoryTexts.WellRescue, "Tim");
            Say(Who.Dick, "BRUNNEN0042", 5, last: true, after: reached);
        }
        return true;
    }

    /// <summary>Dick opens the tin with the tin opener in the kitchen: the map and a key are inside (quest 7, step 3).</summary>
    bool UseClosedTin(string target)
    {
        if (!Same(target, "LOC05Dosenoeffner"))
            return false;
        if (Hero == George)
            return false;
        SetText(StoryTexts.KitchenTopic, "nobody");
        Walk(Hero, 620, 454);
        Chain = State(Hero, TakeTin);
        PointerOff();
        SetVar(Vars.TinOpened, 1);
        Say(Who.Dick, "KUECHE0027", 3, last: true);
        PointerAfter(true, Chain, x: 100, y: 200);
        SetStep(7, 3);
        Take(Item.ClosedTin);
        Give(Item.OpenTin);
        Give(Item.Key);
        return true;
    }

    /// <summary>
    /// The pocket knife: it breaks on the cabin cupboard; at the guest-room keyhole the others
    /// talk about how to get the key out, and with the newspaper under the door, Dick pushes it out.
    /// </summary>
    bool UseKnife(string target)
    {
        if (Same(target, "LOC16Scharnier") || Same(target, "LOC16Schloss"))
        {
            Walk(Hero, 620, 464);
            Chain = State(Hero, TakeLowRight);
            Take(Item.OpenKnife);
            Take(Item.ClosedKnife);
            Give(Item.BrokenKnife);
            Say(Who.Dick, "KAJUETE0024", 10, last: true);
            return true;
        }
        if (!Same(target, "LOC04Schluesselloch"))
            return false;
        if (Episode != 1 && Quest != 5)
            return false;
        State(Hero, StandDownLeft);
        Chain = null;
        switch (Var(Vars.GuestRoomKeyPlan))
        {
            case 0:
                Say(Who.Julian, "Diele0010-04", 8, last: true);
                SetVar(Vars.GuestRoomKeyPlan, 1);
                break;
            case 1:
                Say(Who.Julian, "Diele0010-04", 9);
                Say(Who.Dick, "DIELE0010-04", 10);
                Say(Who.Anne, "DIELE0010-04", 11);
                Say(Who.Julian, "Diele0010-04", 12, last: true);
                break;
            case 2: // the newspaper is under the door: push the key out onto it
                PointerOff();
                Walk(Hero, 340, 271);
                Chain = State(Hero, TakeMidUpLeft);
                Say(Who.Julian, "Diele0010-04", 17, last: true);
                Take(Item.ClosedKnife);
                Give(Item.OpenKnife);
                Give(Item.Newspaper);
                Chain = StateAfter(Hero, KneelRight, Chain);
                Say(Who.George, "DIELE0010-04", 18, last: true);
                SetVar(Vars.KeyPushedOut, 1);
                break;
        }
        return true;
    }

    /// <summary>
    /// The screwdriver (or the broken knife): it opens the cabin cupboard (quest 6, step 4), and
    /// pushes the key out of the study door lock (quest 7, step 4).
    /// </summary>
    bool UseScrewdriver(int item, string target)
    {
        if (item == Item.Screwdriver && At(6, 4))
        {
            if (Same(target, "LOC16Scharnier"))
            {
                UnscrewTheCupboard();
                return true;
            }
            if (Same(target, "LOC16Schloss"))
            {
                Walk(Hero, 547, 464);
                Chain = State(Hero, TakeLowRight);
                Say(Who.Dick, "KAJUETE0024", 2, last: true, delay: 4);
                return true;
            }
            return false;
        }
        if (Same(target, "LOC04TueschlossLoc03") && At(7, 4))
        {
            PointerOff();
            Walk(Hero, 193, 517);
            Chain = State(Hero, TakeMidLeft);
            if (Var(Vars.NewspaperUnderStudyDoor) == 0)
                Say(Who.George, "EINGANG0030", 3, last: true);
            else
            {
                Say(Who.George, "EINGANG0030", 4, last: true);
                SetVar(Vars.StudyKeyPushedThrough, 1);
            }
        }
        return true;
    }

    /// <summary>The hinges come off and the cupboard opens: quest 7 begins.</summary>
    void UnscrewTheCupboard()
    {
        Walk(Hero, 547, 464);
        Chain = State(Hero, TakeLowRight);
        Say(Who.Dick, "KAJUETE0024", 5, last: true, delay: 6);
        var off = ObjAfter("LOC16SchrankZu", false, null, 5);
        ObjAfter("LOC16Schrank", false, off);
        ObjAfter("LOC16Scharnier", false, off);
        Anim("DoOpenSchrank", after: off, endObject: "LOC16SchrankAuf", endObjectOn: true);
        SetVar(Vars.CabinCupboardOpen, 1);
        SetStep(7, 1);
        SetVar(Vars.AnneBeachTalk, 0);
        SetVar(Vars.AnneIslandTalk, 0);
        SetVar(Vars.AnneWreckTalk, 0);
        SetVar(Vars.JulianBeachTalk, 0);
        SetVar(Vars.JulianIslandTalk, 0);
        SetVar(Vars.JulianWreckTalk, 0);
        SetVar(Vars.GeorgeBeachTalk, 0);
        SetVar(Vars.GeorgeIslandTalk, 0);
        SetVar(Vars.GeorgeWreckTalk, 0);
        SetVar(Vars.CabinVisited, 1);
    }

    /// <summary>
    /// The rope: tied to the stone by the well (quest 8, step 3), down the wreck's railing into
    /// the cabin, and to the ivy in the courtyard (quest 11).
    /// </summary>
    bool UseRope(string target)
    {
        if (Same(target, "LOC17Stein"))
        {
            Walk(Hero, 490, 458);
            if (At(8, 3))
            {
                var tied = Chain = State(Hero, TieRight);
                PointerOff();
                Say(Who.Julian, "RUINE0040", 2, last: true);
                IncAfter(Vars.RopeTiedOnce, tied);
                SetVar(Vars.RopeOnStone, 1);
                Take(Item.Rope);
            }
            return true;
        }
        if (Same(target, "LOC15Reling"))
        {
            ClimbDownIntoTheCabin();
            return true;
        }
        if (Same(target, "LOC17Efeu"))
        {
            if (At(11, 1))
            {
                if (Var(Vars.RopeOnIvy) != 0)
                    return false;
                PointerOff();
                Walk(Hero, 64, 419);
                Chain = State(Hero, TakeMidLeft);
                Say(Who.Dick, "RUINE0053", 15, last: true);
                SetText(StoryTexts.CourtyardScene, "EfeuMitSeil");
                SetVar(Vars.RopeOnIvy, 1);
                Obj("LOC17ExitToLoc21", false);
                Obj("LOC17Funkgeraet", true);
            }
            return true;
        }
        if (At(8, 3))
        {
            Chain = null;
            PointerOff();
            Say(Who.Dick, "RUINE0040", 24, last: true);
        }
        return false;
    }

    /// <summary>The hero ties the rope to the railing and climbs down it into the wreck's cabin.</summary>
    void ClimbDownIntoTheCabin()
    {
        var climbing = State(Anne, SitDownRight);
        HideAfter(Hero, climbing);
        PointerAfter(false, climbing, x: 400, y: 400);
        foreach (string anim in new[] { "Rope", "RopeStill", "RopeUp", "RopeUpStill" })
        {
            AnimScale(anim, 0.57);
            AnimPos(anim, 290, 83);
        }
        Depth(@"bmp\obj\dick\dicl\diclup\diclup.brg", 6000);
        Depth(@"bmp\obj\dick\dicl\diclup\diclcl.brg", 6000);
        climbing = Anim("Rope", after: climbing);
        climbing = Anim("RopeUp", after: climbing);
        Anim("RopeUpStill", -1, climbing);
        ChangeLocation("loc16", climbing);
    }

    /// <summary>
    /// A stone: thrown at the bush in the courtyard (quest 8, step 3), down the well, or at the
    /// professor on the telephone (quest 11) — the third throw makes him walk off.
    /// </summary>
    bool UseStone(string target)
    {
        if (Same(target, "LOC17Busch"))
        {
            if (At(8, 3))
            {
                Walk(Hero, 410, 442);
                var thrown = Chain = State(Hero, ThrowRight);
                Sound(@"wav\fx\steingeb.wav", thrown, 8);
                if (Var(Vars.StonesAtBush) == 0)
                {
                    PointerOff();
                    IncAfter(Vars.StonesAtBush, thrown);
                    Say(Who.Julian, "RUINE0040", 7, last: true, delay: 25);
                }
                else if (Var(Vars.StonesAtBush) == 1)
                {
                    PointerOff();
                    Say(Who.George, "RUINE0040", 8, last: true, delay: 25);
                }
                return true;
            }
            if (At(11, 1))
            {
                if (Var(Vars.ProfessorDistracted) == 0)
                {
                    SetVar(Vars.ProfessorDistracted, 1);
                    if (Session.InventoryOpen || Var(Vars.StonesThrown) > 2)
                        return false;
                    ThrowAtTheProfessor(@"wav\fx\steinbru.wav");
                }
                else if (Var(Vars.ProfessorDistracted) == 1)
                    DriveTheProfessorOff();
                return false;
            }
            Walk(Hero, 410, 442);
            State(Hero, Hero == Dick ? ThrowRight : StandUpRight);
            return true;
        }
        if (Same(target, "LOC19Loch"))
        {
            Walk(Hero, 257, 522);
            var thrown = Chain = State(Hero, ThrowRight);
            Sound(@"wav\fx\steinbru.wav", thrown, 8);
            if (At(8, 4))
            {
                PointerOff();
                Say(Who.Dick, "BRUNNEN0042", 2, last: true, delay: 25);
            }
            return true;
        }
        if (At(11, 1))
        {
            if (Session.InventoryOpen || Var(Vars.StonesThrown) > 2)
                return false;
            ThrowAtTheProfessor(@"wav\fx\steinste.wav");
        }
        return false;
    }

    /// <summary>
    /// A stone lands near the professor (quest 11): he hangs up the telephone and says
    /// something (a different line for each of the first three throws).
    /// </summary>
    void ThrowAtTheProfessor(string sound)
    {
        int thrown = Var(Vars.StonesThrown);
        var throwing = State(Hero, ThrowLeft);
        Sound(sound, throwing, 8);
        StopAnimAfter("ProfTelefon", throwing, 5);
        var hungUp = Chain = Anim("ProfHangUp", after: throwing, delay: 5);
        Anim("ProfStandLeft", -1, hungUp);
        Anim("Radio", -1, hungUp);
        PointerAfter(false, hungUp, x: 400, y: 400);
        if (thrown < 2)
            IncAfter(Vars.StonesThrown, hungUp);
        Say(Who.Professor, "RUINE0053", 5 + thrown, last: true, after: hungUp, delay: 9);
    }

    /// <summary>The stone in the bush is too much: the professor walks off to look, and the courtyard is free.</summary>
    void DriveTheProfessorOff()
    {
        SetVar(Vars.StonesThrown, 3);
        SetVar(Vars.ProfessorDistracted, 2);
        SetVar(Vars.CourtyardFree, 2);
        var throwing = State(Hero, ThrowLeft);
        Sound(@"wav\fx\steinbru.wav", throwing, 8);
        StopAnimAfter("ProfTelefon", throwing, 5);
        var hungUp = Chain = Anim("ProfHangUp", after: throwing, delay: 5);
        Anim("ProfWalkRight", -1, hungUp);
        Anim("Radio", -1, hungUp);
        Say(Who.Professor, "RUINE0053", 8, after: hungUp);
        Say(Who.Professor, "RUINE0053", 9);
        Say(Who.Dick, "RUINE0053", 10, last: true);
    }

    /// <summary>The lantern and the candle combine into a lit lantern (in the rucksack).</summary>
    bool Combine(string target, int other)
    {
        if (!Same(target, Inventory.ObjectName(other)))
            return false;
        Take(Item.Lantern);
        Take(Item.Candle);
        Give(Item.LanternWithCandle);
        ShowRucksack();
        return true;
    }

    /// <summary>
    /// Wood on the study fire. While Uncle Quentin works (quest 7, steps 3-4) he objects, unless
    /// the fire was put out first (then the wood goes on); otherwise the fire burns up.
    /// </summary>
    bool UseFirewood(string target)
    {
        if (!Same(target, "LOC03Kamin"))
            return false;
        Walk(Hero, 395, 377);
        var kneeling = State(Hero, KneelLeft);
        if (At(7, 3))
        {
            Chain = kneeling;
            PointerAfter(false, kneeling, x: 300, y: 400);
            Say(Who.Quentin, "ARBEIT0029", 6, last: true);
            return false;
        }
        if (At(7, 4))
        {
            Chain = kneeling;
            PointerAfter(false, kneeling, x: 300, y: 400);
            if (Var(Vars.StudyFireOut) == 0)
            {
                SetAfter(Vars.V64f3e4, 1, kneeling);
                Say(Who.Quentin, "ARBEIT0032", 12, last: true);
                return false;
            }
            SetAfter(Vars.V64f3e4, 2, kneeling);
            Say(Who.Quentin, "ARBEIT0032", 14, last: true);
        }
        StopAnimAfter("FeuerKlein", kneeling, 5);
        StopAnimAfter("FeuerMittel", kneeling, 5);
        Anim("FeuerGross", -1, kneeling, 5);
        SetVar(Vars.StudyFire, 2);
        Take(Item.Firewood);
        return true;
    }

    /// <summary>The olive oil fills the empty oil can; on the cabin cupboard it does not help.</summary>
    bool UseOliveOil(string target)
    {
        if (Same(target, Inventory.ObjectName(Item.EmptyOilCan)))
        {
            FillTheOilCan();
            return true;
        }
        if (Same(target, "LOC16Schloss"))
        {
            Walk(Hero, 547, 464);
            Chain = State(Hero, TakeLowRight);
            Say(Who.Dick, "KAJUETE0024", 3, last: true, delay: 4);
            return true;
        }
        if (Same(target, "LOC16Scharnier"))
        {
            Walk(Hero, 620, 464);
            Chain = State(Hero, TakeLowRight);
            Say(Who.Dick, "KAJUETE0024", 4, last: true, delay: 4);
            return true;
        }
        return false;
    }

    void FillTheOilCan()
    {
        Take(Item.EmptyOilCan);
        Give(Item.FullOilCan);
        ShowRucksack();
    }

    /// <summary>The axe: it cuts down the bush in the courtyard, and breaks the treasure door (quest 9).</summary>
    bool UseAxe(string target)
    {
        if (Same(target, "LOC17Busch"))
        {
            if (Var(Vars.BushState) == 1)
                CutDownTheBush();
            return true;
        }
        if (!Same(target, "LOC24ExitToLoc27"))
            return false;
        if (!At(9, 1))
            return true;
        if (Var(Vars.TreasureDoorDiscussed) == 0)
        {
            // George goes for the others first; the door is broken after the talk (see TreasureDoor).
            if (Var(Vars.TreasureDoorBroken) == 1)
                return false;
            var turned = Chain = State(Hero, FacingGeorge);
            Say(Who.George, "LABYRINTH0046", 2, after: turned);
            Say(Who.Dick, "LABYRINTH0046", 3, after: turned);
            Say(Who.George, "LABYRINTH0046", 4, after: turned);
            Say(Who.Dick, "LABYRINTH0046", 5, last: true, after: turned);
            SetText(StoryTexts.TreasureDoorScene, "geh2");
        }
        else if (Var(Vars.TreasureDoorDiscussed) == 1)
        {
            if (Var(Vars.TreasureDoorBroken) == 1)
                return false;
            WalkAfter(Hero, 348, 380, null, 5);
            PointerAfter(false, null, x: 400, y: 400);
            var chopping = Chain = State(Hero, AxeRight);
            Sound(@"wav\fx\axt2.wav", chopping, 8);
            Sound(@"wav\fx\axt2.wav", chopping, 18);
            Sound(@"wav\fx\axt2.wav", chopping, 28);
            Say(Who.Dick, "LABYRINTH0046", 7, last: true, delay: 31);
            SetVar(Vars.TreasureDoorBroken, 1);
            ObjAfter("LOC24Tuer", false, chopping, 6);
            Anim("TuerClose", -1, chopping, 6);
            SetText(StoryTexts.TreasureDoorScene, "Tuer");
        }
        return true;
    }

    /// <summary>Three blows of the axe, and the bush in the courtyard is gone.</summary>
    void CutDownTheBush()
    {
        SetVar(Vars.BushState, 2);
        WalkAfter(Hero, 431, 438, null, 5);
        PointerAfter(false, null, x: 400, y: 400);
        var chopping = Chain = State(Hero, AxeRight);
        IncAfter(Vars.AnneCastleTalk, chopping);
        IncAfter(Vars.GeorgeCastleTalk, chopping);
        IncAfter(Vars.JulianCastleTalk, chopping);
        PointerAfter(false, chopping, x: 400, y: 400);
        ObjAfter("LOC17BuschObj", false, chopping, 28);
        Sound(@"wav\fx\axtgestr.wav", chopping, 8);
        Sound(@"wav\fx\axtgestr.wav", chopping, 18);
        Sound(@"wav\fx\axtgestr.wav", chopping, 28);
        SetText(StoryTexts.CourtyardScene, "Axt");
        SetVar(Vars.BushCut, 1);
    }

    /// <summary>The torch on the bush: Dick looks into it.</summary>
    bool UseTorch(string target)
    {
        if (!Same(target, "LOC17Busch"))
            return false;
        if (Var(Vars.BushState) == 1)
        {
            WalkAfter(Hero, 431, 442, null, 5);
            PointerAfter(false, null, x: 400, y: 400);
            Chain = State(Hero, StandUpRight);
            Say(Who.Dick, "RUINE0038", 20, last: true, delay: 10);
        }
        return true;
    }

    /// <summary>
    /// The sandwich: on the bush Dick and George talk; given to Timmy under the kitchen table,
    /// Uncle Quentin notices — unless he is busy with his tea.
    /// </summary>
    bool UseSandwich(string target)
    {
        if (Same(target, "LOC17Busch") && Var(Vars.BushState) == 1)
        {
            WalkAfter(Hero, 431, 442, null, 5);
            PointerAfter(false, null, x: 400, y: 400);
            Chain = State(Hero, StandDownRight);
            Say(Who.Dick, "RUINE0038", 35);
            Say(Who.George, "RUINE0038", 21);
            Say(Who.Dick, "RUINE0038", 22);
            Say(Who.George, "RUINE0038", 23, last: true);
        }
        if (Same(target, "LOC05Timmy"))
        {
            if (Var(Vars.Distracted) == 0)
                TryToFeedTimmy();
            else
                FeedTimmy();
        }
        return true;
    }

    /// <summary>George tries to feed Timmy, but Uncle Quentin sees it (a different remark each time).</summary>
    void TryToFeedTimmy()
    {
        Chain = null;
        StopAnimAfter("GeorgesBreadStill", null, 3);
        StopAnimAfter("GeorgeSitDownRight", null);
        Anim("GeorgeTryFeed");
        Anim("GeorgeTryFeedArm");
        StateAfter(Timmy, EatRight, null, 3);
        Anim("GeorgeSitDownRight", -1, null, 6);
        int tries = Var(Vars.TimmyFeedTries);
        if (tries > 2)
            return;
        PointerOff();
        if (tries < 2)
            SetVar(Vars.TimmyFeedTries, tries + 1);
        Say(Who.Quentin, "KUECHE0005-05", 34 + tries, last: true, delay: 16);
    }

    /// <summary>While Uncle Quentin drinks his tea, George slips the sandwich to Timmy.</summary>
    void FeedTimmy()
    {
        SetText(StoryTexts.KitchenTopic, "Sandwich");
        Chain = null;
        PointerOff();
        StopAnimAfter("GeorgesBreadStill", null, 3);
        StopAnimAfter("GeorgeSitDownRight", null);
        Anim("GeorgeFeed");
        Anim("GeorgeFeedArm");
        StateAfter(Timmy, EatRight, null, 10);
        Anim("GeorgeSitDownRight", -1, null, 4);
        StopAnimAfter("QuentinDrinkAll", null, 22);
        Anim("QuentinStopDrinking", delay: 22);
        Take(Item.Sandwich);
        Say(Who.Quentin, "KUECHE0005-05", 37, last: true, delay: 26);
    }

    /// <summary>
    /// The newspaper under a door: the guest-room door (from quest 5), so the key can be pushed
    /// out onto it; the study door (quest 7, step 4).
    /// </summary>
    bool UseNewspaper(string target)
    {
        Chain = null;
        if (Same(target, "LOC04Tuerspalte2"))
        {
            if (Quest < 5)
                return false;
            PointerOff();
            Walk(Hero, 360, 271);
            Chain = State(Hero, KneelLeft);
            Say(Who.Dick, "DIELE0010-04", 13, last: true);
            SetVar(Vars.GuestRoomKeyPlan, 2);
            Obj("LOC04Schluesselloch", true);
            Take(Item.Newspaper);
            return true;
        }
        if (Same(target, "LOC04Tuerspalte4"))
        {
            if (At(7, 4))
            {
                PointerOff();
                Walk(Hero, 193, 517);
                Chain = State(Hero, KneelLeft);
                Say(Who.George, "EINGANG0030", 2, last: true, delay: 6);
                SetVar(Vars.NewspaperUnderStudyDoor, 1);
            }
            return true;
        }
        return false;
    }

    /// <summary>The full oil can on the study door: its lock is oiled (from quest 3).</summary>
    bool UseFullOilCan(string target)
    {
        if (Quest < 3)
            return false;
        if (!Same(target, "LOC04ExitToLOC03") && !Same(target, "LOC04TueschlossLoc03"))
            return false;
        Walk(Hero, 193, 517);
        var reached = State(Hero, TakeMidLeft);
        Sound(@"wav\fx\oelgluck.wav", reached, 3);
        SetVar(Vars.TryingStudyDoor, 1);
        ObjAfter("LOC04Tuerspalte2", false, reached, 5);
        PointerOff();
        Take(Item.FullOilCan);
        SetVar(Vars.StudyDoorState, 6);
        return true;
    }

    /// <summary>The empty oil can: filled with the olive oil, or tried on the study door (it is empty).</summary>
    bool UseEmptyOilCan(string target)
    {
        if (Same(target, Inventory.ObjectName(Item.OliveOil)))
        {
            FillTheOilCan();
            return true;
        }
        if (!Same(target, "LOC04ExitToLOC03"))
            return false;
        Walk(Hero, 193, 517);
        var reached = State(Hero, TakeMidLeft);
        SetVar(Vars.TryingStudyDoor, 1);
        ObjAfter("LOC04Tuerspalte2", false, reached, 5);
        PointerOff();
        SetVar(Vars.StudyDoorState, 5);
        SetVar(Vars.AnneTalk, 0);
        SetVar(Vars.DickTalk, 0);
        SetVar(Vars.JulianTalk, 0);
        SetVar(Vars.FannyTalk, 0);
        SetVar(Vars.QuentinKitchenTalk, 0);
        return true;
    }

    /// <summary>The suitcase key opens Uncle Quentin's suitcase in the hall (quest 5, step 2): the ship's plan is inside.</summary>
    bool UseSuitcaseKey(string target)
    {
        if (!Same(target, "LOC04Koffer"))
            return false;
        if (!At(5, 2))
            return true;
        if (Var(Vars.SuitcaseState) == 0)
        {
            Walk(Hero, 270, 570);
            PointerOff();
            var kneeling = Chain = State(Hero, KneelLeft);
            PointerAfter(false, kneeling, x: 200, y: 300);
            StopAnimAfter("KofferStehend", kneeling, 10);
            Anim("Koffer", after: kneeling, delay: 10);
            Anim("KofferKarte", -1, kneeling, 13);
            SetText(StoryTexts.HallScene, "Koffer");
            Give(Item.ShipPlan);
            Say(Who.George, "FLUR0012-01", 61, last: true, after: kneeling, delay: 18);
            Say(Who.Julian, "FLUR0012-01", 62, last: true);
            Say(Who.Anne, "FLUR0012-01", 63, last: true);
        }
        SetVar(Vars.SuitcaseState, 1);
        return true;
    }

    /// <summary>
    /// The key from the tin on the model ship in the study: Uncle Quentin objects while he works
    /// (quest 7, step 3); later it opens the ship, which holds the treasure map (quest 8 begins).
    /// </summary>
    bool UseKey(string target)
    {
        if (!Same(target, "LOC03Schiff"))
            return false;
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
        if (At(7, 6))
        {
            PointerOff();
            SetStep(8, 1);
            Give(Item.TreasureMap);
            Take(Item.Key);
            ChangeLocation("schiffc1", reached);
        }
        return true;
    }
}
