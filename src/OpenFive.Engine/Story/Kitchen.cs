using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The kitchen at Kirrin Cottage (loc05). Breakfast in quests 3-5 (George feeds Timmy under the
/// table, Uncle Quentin's question, George slips out with a sandwich), the rainy day in quest 5,
/// George's book in quest 6, tea time in quest 7 (the teapot, the toast and the olive oil) and the
/// policeman at the end of the story.
/// </summary>
public sealed class Kitchen : StoryScript
{
    /// <inheritdoc/>
    public override string Location => "loc05";

    /// <inheritdoc/>
    protected override void Arrive() { }

    // ---- clicks --------------------------------------------------------------------------------

    /// <summary>Things that are only looked at: where the hero stands, which way they face, and the flag it sets.</summary>
    static readonly Dictionary<string, (int X, int Y, int Facing, string Seen)> LookAt = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LOC05Kochbuecher"] = (360, 400, LookUpLeft, Vars.CookbooksSeen),
        ["LOC05Essigflaschen"] = (360, 400, StandUpLeft, Vars.VinegarSeen),
        ["LOC05Tassen"] = (360, 400, StandRight, Vars.CupsSeen),
        ["LOC05Spueltuch"] = (650, 400, StandUpLeft, Vars.DishclothSeen),
        ["LOC05Muelleimer"] = (650, 400, StandUpLeft, Vars.KitchenBinSeen),
        ["LOC05Wassernapf"] = (700, 400, StandUpRight, Vars.WaterBowlSeen),
        ["LOC05Dosenoeffner"] = (500, 400, StandRight, Vars.TinOpened),
        ["LOC05Obstkorb"] = (400, 410, StandRight, Vars.FruitBowlSeen),
        ["LOC05Schrank5"] = (400, 410, LookUpRight, Vars.Cupboard5Seen),
        ["LOC05Fenster"] = (253, 433, StandUpLeft, Vars.KitchenWindowSeen),
        ["LOC05Spuele"] = (253, 433, StandUpLeft, Vars.SinkSeen),
        ["LOC05Tellerregal"] = (253, 433, LookUpLeft, Vars.PlateRackSeen),
        ["LOC05Schrank1"] = (80, 433, LookUpLeft, Vars.Cupboard1Seen),
    };

    /// <summary>The cupboards, the stove, the food, the people at the table and the door to the hall.</summary>
    /// <remarks>The original's FUN_0042f05a.</remarks>
    public override bool Click(string name)
    {
        // Every click makes Uncle Quentin a little more patient (see Study.EveryTick).
        SetVar(Vars.GrumbleTimer, Var(Vars.GrumbleTimer) + 300);
        if (LookAt.TryGetValue(name, out var spot))
        {
            WalkAndFace(spot.X, spot.Y, spot.Facing);
            if (Var(spot.Seen) == 0)
                SetVar(spot.Seen, 1);
        }
        else if (Same(name, "LOC05Herd"))
            TheStove();
        else if (Same(name, "LOC05Zettel"))
            ReadTheNote();
        else if (Same(name, "LOC05Julian"))
            TalkToJulian();
        else if (Same(name, "LOC05Anne"))
            TalkToAnne();
        else if (Same(name, "LOC05Timmy"))
            TheDogUnderTheTable();
        else if (Same(name, "LOC05Quentin"))
            TalkToQuentin();
        else if (Same(name, "LOC05Dick"))
            TalkToDick();
        else if (Same(name, "LOC05FannyStand"))
            TalkToFanny();
        else if (Same(name, "LOC05Brotkasten"))
            TheBreadBin();
        else if (Same(name, "LOC05Toast"))
            TakeTheToast();
        else if (Same(name, "LOC05Flasche"))
            TakeTheBottle();
        else if (Same(name, "LOC05Schrank2"))
            Cupboard2();
        else if (Same(name, "LOC05Schrank3"))
            Cupboard3();
        else if (Same(name, "LOC05Schrank4"))
            Cupboard4();
        else if (Same(name, "LOC05Schrank6"))
            Cupboard6();
        else if (Same(name, "LOC05GeorgesSandwich"))
            TakeGeorgesSandwich();
        else if (Same(name, "LOC05ExitToLOC04"))
            ToTheHall();
        else if (Same(name, "LOC05Kanne") && Var(Vars.Distracted) == 0)
            TheTeapot();
        return true;
    }

    void TheStove()
    {
        TaskHandle? ready = null;
        Walk(Hero, 83, 520);
        ready = State(Hero, TakeMidUpRight);
        if (Var(Vars.StoveOpen) == 0)
        {
            Sound(@"wav\fx\ofenauf.wav", ready, 3);
            Anim("DoOpenOfen", after: ready, delay: 5, endObject: "LOC05OpenOfenObj", endObjectOn: true);
            SetVar(Vars.StoveOpen, 1);
        }
        else if (Var(Vars.StoveOpen) == 1)
        {
            ObjAfter("LOC05OpenOfenObj", false, ready);
            Sound(@"wav\fx\ofenzu.wav", ready, 3);
            Anim("DoCloseOfen", after: ready);
            SetVar(Vars.StoveOpen, 0);
        }
    }

    void ReadTheNote()
    {
        Walk(Hero, 545, 435);
        Chain = State(Hero, StandDownLeft);
        Say(Who.Dick, "KUECHE0027", 1);
        Say(Who.Dick, "KUECHE0027", 2);
        Say(Who.Dick, "KUECHE0027", 8);
        Say(Who.Dick, "KUECHE0027", 9, last: true);
    }

    void TalkToJulian()
    {
        TaskHandle? ready = null;
        SetText(StoryTexts.KitchenTopic, "Julian");
        if (At(2, 2))
        {
            Walk(Hero, 100, 450);
            Chain = State(Hero, StandDownRight);
            ready = Chain;
            if (Var(Vars.JulianTalk) == 0)
            {
                Say(Who.Julian, "KUECHE", 3);
                Say(Who.Dick, "KUECHE", 3, last: true);
                SetVar(Vars.JulianTalk, 1);
            }
            else if (Var(Vars.JulianTalk) == 1)
            {
                Say(Who.Julian, "KUECHE", 4, last: true);
            }
        }
        else if (At(2, 3))
        {
            Walk(Hero, 100, 450);
            Chain = State(Hero, StandDownRight);
            ready = Chain;
            if (Var(Vars.JulianTalk) == 0)
            {
                Say(Who.Julian, "KUECHE0003-05", 2);
                Say(Who.Dick, "KUECHE0003-05", 2, last: true);
                SetVar(Vars.JulianTalk, 1);
            }
            else if (Var(Vars.JulianTalk) == 1)
            {
                Say(Who.Julian, "KUECHE0003-05", 21);
                Say(Who.Dick, "KUECHE0003-05", 21, last: true);
            }
            else if (Var(Vars.JulianTalk) == 2)
            {
                Say(Who.Julian, "KUECHE0003-03-05", 1);
                Say(Who.Dick, "KUECHE0003-03-05", 2);
                Say(Who.Julian, "KUECHE0003-03-05", 11, last: true, after: ready);
                SetVar(Vars.JulianTalk, 3);
            }
            else if (Var(Vars.JulianTalk) == 3)
            {
                Say(Who.Julian, "KUECHE0003-03-05", 2);
                Say(Who.Dick, "KUECHE0003-03-05", 21);
                Say(Who.Julian, "KUECHE0003-03-05", 21, last: true, after: ready);
            }
        }
        else if (At(5, 0))
        {
            ready = null;
            Chain = null;
            Say(Who.Julian, "KUECHE0009-05", 1, last: true);
        }
        else if (At(3, 3))
        {
            ready = null;
            Chain = null;
            if (Var(Vars.JulianTalk) < 2)
            {
                Say(Who.Julian, "KUECHE0005-05", 29, last: true);
            }
            else if (Var(Vars.JulianTalk) == 2)
            {
                Say(Who.Julian, "KUECHE0005-05", 39, last: true);
            }
            else if (Var(Vars.JulianTalk) == 3)
            {
                Say(Who.Julian, "KUECHE0005-05", 40, last: true);
            }
            else if (Var(Vars.JulianTalk) == 4)
            {
                Say(Who.Julian, "KUECHE0005-05", 52, last: true);
            }
        }
        else if (At(3, 4))
        {
            Walk(Hero, 100, 450);
            Chain = State(Hero, StandDownRight);
            if (Var(Vars.StudyDoorState) == 5)
            {
                ready = Chain;
                Say(Who.Julian, "KUECHE0006-05", 15, last: true);
            }
        }
        else if (At(5, 4))
        {
            ready = null;
            Chain = null;
            SetText(StoryTexts.KitchenTopic, "");
            if (Var(Vars.JulianTalk) == 0)
            {
                SetText(StoryTexts.KitchenTopic, "Julian");
                Say(Who.Julian, "KUECHE0014-05", 3, last: true);
            }
            else if (Var(Vars.KitchenMenuAnswer) == 1)
            {
                Say(Who.Julian, "KUECHE0014-05", 7, last: true);
            }
        }
        else if (Quest == 5 && Episode > 5)
        {
            ready = null;
            Chain = null;
            if (Var(Vars.UnderBedTalk) > 0)
            {
                SetText(StoryTexts.KitchenTopic, "");
                if (Var(Vars.JulianTalk) == 3)
                {
                    Say(Who.Julian, "KUECHE0015-05", 16, last: true);
                }
                else if (Var(Vars.JulianTalk) == 4)
                {
                    Say(Who.Julian, "KUECHE0015-05", 17, last: true);
                }
                else
                {
                    Say(Who.Julian, "KUECHE0015-05", 18);
                    Say(Who.George, "KUECHE0015-05", 19);
                    Say(Who.Julian, "KUECHE0015-05", 20, last: true);
                }
            }
        }
        else if (At(7, 4))
        {
            Walk(Hero, 100, 450);
            Chain = State(Hero, StandDownRight);
            ready = Chain;
            if (Var(Vars.JulianTalk) == 0)
            {
                IncAfter(Vars.JulianTalk, Chain);
                Say(Who.Julian, "KUECHE0031", 9, last: true);
            }
            else if (Var(Vars.JulianTalk) == 1)
            {
                Say(Who.Julian, "KUECHE0031", 10, last: true);
            }
            else if (Var(Vars.JulianTalk) == 2)
            {
                IncAfter(Vars.JulianTalk, Chain);
                Say(Who.Julian, "KUECHE0031", 18, last: true);
            }
            else if (Var(Vars.JulianTalk) == 3)
            {
                Say(Who.Julian, "KUECHE0031", 19, last: true);
            }
            else if (Var(Vars.JulianTalk) == 6)
            {
                IncAfter(Vars.JulianTalk, Chain);
                Say(Who.Julian, "KUECHE0032", 14, last: true);
            }
            else if (Var(Vars.JulianTalk) == 7)
            {
                IncAfter(Vars.JulianTalk, Chain);
                Say(Who.Julian, "KUECHE0032", 15, last: true);
            }
            else if (Var(Vars.JulianTalk) == 8)
            {
                Say(Who.Julian, "KUECHE0032", 16);
                Say(Who.George, "KUECHE0032", 17, last: true, after: ready);
            }
            else if (Var(Vars.JulianTalk) == 9)
            {
                IncAfter(Vars.JulianTalk, Chain);
                Say(Who.Julian, "KUECHE0032", 32, last: true);
            }
            else if (Var(Vars.JulianTalk) == 10)
            {
                Say(Who.Julian, "KUECHE0032", 33);
                Say(Who.George, "KUECHE0032", 34, after: ready);
                Say(Who.Dick, "KUECHE0032", 35, after: ready);
                Say(Who.George, "KUECHE0032", 36, last: true, after: ready);
            }
            else if (Var(Vars.JulianTalk) == 11)
            {
                Say(Who.Julian, "KUECHE0032", 47);
                Say(Who.George, "KUECHE0032", 48, last: true, after: ready);
            }
        }
        else if (At(7, 5))
        {
            Walk(Hero, 100, 450);
            Chain = State(Hero, StandDownRight);
            ready = Chain;
            Say(Who.Julian, "KUECHE0033", 4, last: true);
        }
    }

    void TalkToAnne()
    {
        TaskHandle? ready = null;
        SetText(StoryTexts.KitchenTopic, "Anne");
        if (At(2, 2))
        {
            Walk(Hero, 100, 450);
            Chain = State(Hero, StandDownRight);
            ready = Chain;
            if (Var(Vars.AnneTalk) == 0)
            {
                Say(Who.Anne, "KUECHE", 5);
                Say(Who.Dick, "KUECHE", 5, last: true);
                SetVar(Vars.AnneTalk, 1);
            }
            else if (Var(Vars.AnneTalk) == 1)
            {
                Say(Who.Anne, "KUECHE", 6, last: true);
            }
        }
        else if (At(2, 3))
        {
            Walk(Hero, 100, 450);
            Chain = State(Hero, StandDownRight);
            ready = Chain;
            if (Var(Vars.AnneTalk) == 0)
            {
                Say(Who.Anne, "KUECHE0003-05", 3);
                Say(Who.Dick, "KUECHE0003-05", 3, after: ready);
                Say(Who.Anne, "KUECHE0003-05", 31, last: true, after: ready);
                SetVar(Vars.AnneTalk, 1);
            }
            else if (Var(Vars.AnneTalk) == 1)
            {
                Say(Who.Anne, "KUECHE0003-05", 32);
                Say(Who.Dick, "KUECHE0003-05", 32);
                Say(Who.Anne, "KUECHE0003-05", 33, last: true);
            }
            else if (Var(Vars.AnneTalk) == 2)
            {
                Say(Who.Anne, "KUECHE0003-03-05", 1);
                Say(Who.Dick, "KUECHE0003-03-05", 3, last: true);
                SetVar(Vars.AnneTalk, 3);
            }
            else if (Var(Vars.AnneTalk) == 3)
            {
                Say(Who.Anne, "KUECHE0003-03-05", 2);
                Say(Who.Dick, "KUECHE0003-03-05", 31, last: true);
            }
        }
        else if (At(3, 4))
        {
            Walk(Hero, 100, 450);
            Chain = State(Hero, StandDownRight);
            ready = Chain;
            if (Var(Vars.StudyDoorState) < 5)
            {
                Say(Who.Anne, "KUECHE0006-05", 11, last: true);
            }
            else
            {
                Say(Who.Anne, "KUECHE0006-05", 21, last: true);
            }
        }
        else if (At(3, 3))
        {
            ready = null;
            Chain = null;
            if (Var(Vars.AnneTalk) == 0)
            {
                Say(Who.Anne, "KUECHE0005-05", 26, last: true);
            }
            else if (Var(Vars.AnneTalk) == 2)
            {
                Say(Who.Anne, "KUECHE0005-05", 35, last: true);
            }
            else if (Var(Vars.AnneTalk) == 3)
            {
                Say(Who.Anne, "KUECHE0005-05", 36, last: true);
            }
            else if (Var(Vars.AnneTalk) == 4)
            {
                Say(Who.Anne, "KUECHE0005-05", 53, last: true);
            }
        }
        else if (At(5, 0))
        {
            ready = null;
            Chain = null;
            Say(Who.Anne, "KUECHE0009-05", 1, last: true);
        }
        else if (At(5, 4))
        {
            ready = null;
            Chain = null;
            SetText(StoryTexts.KitchenTopic, "");
            if (Var(Vars.AnneTalk) == 0)
            {
                SetText(StoryTexts.KitchenTopic, "Anne");
                Say(Who.Anne, "KUECHE0014-05", 1, last: true);
            }
            else if (Var(Vars.KitchenMenuAnswer) == 1)
            {
                Say(Who.Anne, "KUECHE0014-05", 8, last: true);
            }
        }
        else if (Quest == 5 && Episode > 5)
        {
            ready = null;
            Chain = null;
            if (Var(Vars.UnderBedTalk) > 0)
            {
                if (Var(Vars.AnneTalk) == 3)
                {
                    Say(Who.Anne, "KUECHE0015-05", 21, last: true);
                    Say(Who.George, "KUECHE0015-05", 22, last: true);
                }
                else
                {
                    Say(Who.Anne, "KUECHE0015-05", 23, last: true);
                }
            }
        }
        else if (At(7, 4))
        {
            Walk(Hero, 399, 391);
            Chain = State(Hero, StandDownRight);
            ready = Chain;
            if (Var(Vars.AnneTalk) == 0)
            {
                IncAfter(Vars.AnneTalk, Chain);
                Say(Who.Anne, "KUECHE0031", 7, last: true);
            }
            else if (Var(Vars.AnneTalk) == 1)
            {
                Say(Who.Anne, "KUECHE0031", 8, last: true);
            }
            else if (Var(Vars.AnneTalk) == 2)
            {
                Say(Who.Anne, "KUECHE0031", 13);
                Say(Who.George, "KUECHE0031", 14, after: ready);
                Say(Who.Anne, "KUECHE0031", 15, last: true, after: ready);
            }
            else if (Var(Vars.AnneTalk) == 4)
            {
                IncAfter(Vars.AnneTalk, Chain);
                Say(Who.Anne, "KUECHE0032", 5);
                Say(Who.Julian, "KUECHE0032", 6, last: true, after: ready);
            }
            else if (Var(Vars.AnneTalk) == 5)
            {
                Say(Who.Anne, "KUECHE0032", 7, last: true);
            }
            else if (Var(Vars.AnneTalk) == 6)
            {
                IncAfter(Vars.AnneTalk, Chain);
                Say(Who.Anne, "KUECHE0032", 18, last: true);
            }
            else if (Var(Vars.AnneTalk) == 7)
            {
                Say(Who.Anne, "KUECHE0032", 19, last: true);
            }
            else if (Var(Vars.AnneTalk) == 9)
            {
                IncAfter(Vars.AnneTalk, Chain);
                Say(Who.Anne, "KUECHE0032", 30, last: true);
            }
            else if (Var(Vars.AnneTalk) == 10)
            {
                Say(Who.Anne, "KUECHE0032", 31, last: true);
            }
        }
        else if (At(7, 5))
        {
            Walk(Hero, 399, 391);
            Chain = State(Hero, StandDownRight);
            ready = Chain;
            Say(Who.Anne, "KUECHE0033", 3, last: true);
        }
    }

    void TheDogUnderTheTable()
    {
        if (At(3, 3))
        {
            PointerOff();
            if (Var(Vars.Distracted) != 0)
            {
                StopAnimAfter("QuentinDrink", null);
                StopAnimAfter("QuentinDrinkAll", null);
                Anim("QuentinDrink", -1);
            }
            SetText(StoryTexts.KitchenTopic, "Timmy");
            if (Var(Vars.TimmyTableTalk) == 0)
            {
                Chain = null;
                Say(Who.Quentin, "KUECHE0005-05", 13, delay: 2);
                Say(Who.George, "KUECHE0005-05", 14);
                Say(Who.Quentin, "KUECHE0005-05", 15, last: true);
                SetVar(Vars.TimmyTableTalk, 1);
            }
            else
            {
                Chain = null;
                Say(Who.Quentin, "KUECHE0005-05", 16, last: true, delay: 2);
            }
        }
    }

    void TalkToQuentin()
    {
        TaskHandle? ready = null;
        Chain = null;
        if (Hero.Depth != 0)
        {
            Walk(Hero, 565, 600);
            ready = State(Hero, StandDownLeft);
        }
        if (At(3, 3))
        {
            ready = null;
            Chain = null;
            if (Var(Vars.Distracted) == 0)
            {
                if (Var(Vars.QuentinKitchenTalk) == 0)
                {
                    SetText(StoryTexts.KitchenTopic, "Quentin");
                    Say(Who.Quentin, "KUECHE0005-05", 30);
                    Say(Who.George, "KUECHE0005-05", 31);
                    Say(Who.Quentin, "KUECHE0005-05", 32);
                    Say(Who.Dick, "KUECHE0005-05", 33);
                    Say(Who.Dick, "KUECHE0005-05", 70, last: true);
                }
                else if (Var(Vars.QuentinKitchenTalk) == 2)
                {
                    Say(Who.Quentin, "KUECHE0005-05", 41, last: true);
                }
                else if (Var(Vars.QuentinKitchenTalk) == 3)
                {
                    Say(Who.Quentin, "KUECHE0005-05", 42, last: true);
                }
                else if (Var(Vars.QuentinKitchenTalk) == 4)
                {
                    Say(Who.Quentin, "KUECHE0005-05", 54, last: true);
                }
                else if (Var(Vars.QuentinKitchenTalk) == 5)
                {
                    Say(Who.Quentin, "KUECHE0005-05", 55, last: true);
                }
            }
        }
        else
        {
            Chain = ready;
            if (Quest == 3 && ((Episode == 4 && Var(Vars.StudyDoorState) == 5)))
            {
                if (Var(Vars.QuentinKitchenTalk) == 0)
                {
                    Say(Who.Quentin, "KUECHE0006-05", 13, last: true, after: ready);
                }
                else
                {
                    Say(Who.Quentin, "KUECHE0006-05", 14, last: true, after: ready);
                }
            }
        }
    }

    void TalkToDick()
    {
        TaskHandle? ready = null;
        SetText(StoryTexts.KitchenTopic, "Dick");
        ready = null;
        Chain = null;
        if (Hero.Depth != 0)
        {
            Walk(Hero, 300, 300);
            ready = State(Hero, StandDownLeft);
        }
        if (At(3, 3))
        {
            ready = null;
            Chain = null;
            if (Var(Vars.DickTalk) == 0)
            {
                Say(Who.Dick, "KUECHE0005-05", 27, last: true);
            }
            else if (Var(Vars.DickTalk) == 1)
            {
                Say(Who.Dick, "KUECHE0005-05", 28, last: true);
            }
            else if (Var(Vars.DickTalk) == 2)
            {
                Say(Who.Dick, "KUECHE0005-05", 37, last: true);
            }
            else if (Var(Vars.DickTalk) == 3)
            {
                Say(Who.Dick, "KUECHE0005-05", 38, last: true);
            }
            else if (Var(Vars.DickTalk) == 4)
            {
                Say(Who.Dick, "KUECHE0005-05", 50, last: true);
            }
            else if (Var(Vars.DickTalk) == 5)
            {
                Say(Who.Dick, "KUECHE0005-05", 51, last: true);
            }
        }
        else
        {
            Chain = ready;
            if (At(3, 4))
            {
                if (Var(Vars.StudyDoorState) < 5)
                {
                    if (Var(Vars.DickTalk) == 0)
                    {
                        Say(Who.Dick, "KUECHE0006-05", 6, after: ready);
                        Say(Who.George, "KUECHE0006-05", 7, last: true);
                        if (Var(Vars.KitchenMenuAnswer) == 1)
                        {
                            Say(Who.Dick, "KUECHE0006-05", 8, last: true);
                        }
                        else if (Var(Vars.KitchenMenuAnswer) == 2)
                        {
                            Say(Who.Dick, "KUECHE0006-05", 9, last: true);
                        }
                    }
                    else if (Var(Vars.DickTalk) == 1)
                    {
                        if (Var(Vars.KitchenMenuAnswer) == 1)
                        {
                            Say(Who.Dick, "KUECHE0006-05", 8, last: true);
                        }
                        else if (Var(Vars.KitchenMenuAnswer) == 2)
                        {
                            Say(Who.Dick, "KUECHE0006-05", 10, last: true);
                        }
                    }
                }
                else if (Var(Vars.DickTalk) == 0)
                {
                    Say(Who.Dick, "KUECHE0006-05", 16, last: true);
                    Say(Who.George, "KUECHE0006-05", 17, last: true);
                    IncAfter(Vars.DickTalk, ready);
                }
                else if (Var(Vars.DickTalk) == 1)
                {
                    Say(Who.Dick, "KUECHE0006-05", 18);
                    Say(Who.George, "KUECHE0006-05", 19, last: true);
                    Say(Who.Dick, "KUECHE0006-05", 20, last: true);
                }
            }
            else if (At(5, 0))
            {
                Say(Who.Dick, "KUECHE0009-05", 1, last: true, after: ready);
            }
            else if (At(5, 4))
            {
                ready = null;
                Chain = null;
                SetText(StoryTexts.KitchenTopic, "");
                if (Var(Vars.DickTalk) == 0)
                {
                    SetText(StoryTexts.KitchenTopic, "Dick");
                    Say(Who.Dick, "KUECHE0014-05", 2, last: true);
                }
                else if (Var(Vars.KitchenMenuAnswer) == 1)
                {
                    if (Var(Vars.DickTalk) == 1)
                    {
                        Say(Who.Dick, "KUECHE0014-05", 5, last: true);
                    }
                    else if (Var(Vars.DickTalk) == 2)
                    {
                        Say(Who.Dick, "KUECHE0014-05", 6, last: true);
                    }
                }
            }
            else if (Quest == 5 && Episode > 5)
            {
                ready = null;
                Chain = null;
                if (Var(Vars.UnderBedTalk) > 0)
                {
                    if (Var(Vars.DickTalk) == 3)
                    {
                        Say(Who.Dick, "KUECHE0015-05", 13, last: true);
                        Say(Who.George, "KUECHE0015-05", 14, last: true);
                    }
                    else
                    {
                        Say(Who.Dick, "KUECHE0015-05", 15, last: true);
                    }
                }
            }
            else if (At(7, 4))
            {
                Walk(Hero, 100, 450);
                Chain = State(Hero, StandDownRight);
                ready = Chain;
                if (Var(Vars.DickTalk) == 0)
                {
                    IncAfter(Vars.DickTalk, Chain);
                    Say(Who.Dick, "KUECHE0031", 11, last: true);
                }
                else if (Var(Vars.DickTalk) == 1)
                {
                    Say(Who.Dick, "KUECHE0031", 12, last: true);
                }
                else if (Var(Vars.DickTalk) == 2)
                {
                    IncAfter(Vars.DickTalk, Chain);
                    Say(Who.Dick, "KUECHE0031", 16, last: true);
                }
                else if (Var(Vars.DickTalk) == 3)
                {
                    Say(Who.Dick, "KUECHE0031", 17, last: true);
                }
                else if (Var(Vars.DickTalk) == 4)
                {
                    IncAfter(Vars.DickTalk, Chain);
                    Say(Who.Dick, "KUECHE0032", 8, last: true);
                }
                else if (Var(Vars.DickTalk) == 5)
                {
                    Say(Who.Dick, "KUECHE0032", 9, last: true);
                }
                else if (Var(Vars.DickTalk) == 6)
                {
                    IncAfter(Vars.DickTalk, Chain);
                    Say(Who.Dick, "KUECHE0032", 20, last: true);
                    // The original has lines 21-22 behind a flag that no script ever sets.
                    Say(Who.George, "KUECHE0032", 23, last: true, after: ready);
                }
                else if (Var(Vars.DickTalk) == 7)
                {
                    Say(Who.Dick, "KUECHE0032", 24, last: true);
                }
                else if (Var(Vars.DickTalk) == 9)
                {
                    IncAfter(Vars.DickTalk, Chain);
                    Say(Who.Dick, "KUECHE0032", 37);
                    Say(Who.George, "KUECHE0032", 38, last: true, after: ready);
                }
                else if (Var(Vars.DickTalk) == 10)
                {
                    Say(Who.Dick, "KUECHE0032", 39, last: true);
                }
            }
            else if (At(7, 5))
            {
                Walk(Hero, 100, 450);
                Chain = State(Hero, StandDownRight);
                ready = Chain;
                Say(Who.Dick, "AUSSEN0033", 5);
                Say(Who.George, "KUECHE0033", 6, after: ready);
                Say(Who.George, "KUECHE0033", 7, last: true, after: ready);
            }
        }
    }

    void TalkToFanny()
    {
        TaskHandle? ready = null;
        SetText(StoryTexts.KitchenTopic, "Fanny");
        if (At(2, 2))
        {
            Walk(Hero, 300, 200);
            Chain = State(Hero, StandDownLeft);
            ready = Chain;
            if (Var(Vars.FannyTalk) == 0)
            {
                Say(Who.Fanny, "KUECHE", 1);
                Say(Who.Dick, "KUECHE", 1, last: true);
                SetVar(Vars.FannyTalk, 1);
            }
            else if (Var(Vars.FannyTalk) == 1)
            {
                Say(Who.Fanny, "KUECHE", 2, last: true);
            }
        }
        else if (At(2, 3))
        {
            Walk(Hero, 300, 200);
            Chain = State(Hero, StandDownLeft);
            ready = Chain;
            if (Var(Vars.FannyTalk) == 0)
            {
                Say(Who.Fanny, "KUECHE0003-05", 1);
                Say(Who.Dick, "KUECHE0003-05", 1);
                Say(Who.Fanny, "KUECHE0003-05", 11, last: true, after: ready);
                SetVar(Vars.FannyTalk, 1);
            }
            else if (Var(Vars.FannyTalk) == 1)
            {
                Say(Who.Fanny, "KUECHE0003-05", 2, last: true);
            }
            else if (Var(Vars.FannyTalk) == 2)
            {
                Say(Who.Fanny, "KUECHE0003-03-05", 1);
                Say(Who.Dick, "KUECHE0003-03-05", 1);
                Say(Who.Fanny, "KUECHE0003-03-05", 11, after: ready);
                Say(Who.Dick, "KUECHE0003-03-05", 11, last: true);
                SetVar(Vars.FannyTalk, 3);
            }
            else if (Var(Vars.FannyTalk) == 3)
            {
                Say(Who.Fanny, "KUECHE0003-03-05", 2, last: true);
            }
        }
        else if (At(3, 4))
        {
            Walk(Hero, 300, 200);
            Chain = State(Hero, StandDownLeft);
            ready = Chain;
            if (Var(Vars.StudyDoorState) < 5)
            {
                Say(Who.Fanny, "KUECHE0006-05", 12, last: true);
            }
            else
            {
                Say(Who.Fanny, "KUECHE0006-05", 22);
                Say(Who.George, "KUECHE0006-05", 23, last: true);
            }
        }
        else if (At(3, 3))
        {
            ready = null;
            Chain = null;
            if (Var(Vars.FannyTalk) == 5)
            {
                Say(Who.Fanny, "KUECHE0005-05", 24, last: true);
            }
            else if (Var(Vars.FannyTalk) == 6)
            {
                Say(Who.Fanny, "KUECHE0005-05", 25, last: true);
            }
            else if (Var(Vars.FannyTalk) == 7)
            {
                Say(Who.Fanny, "KUECHE0005-05", 34, last: true);
            }
            else if (Var(Vars.FannyTalk) == 8)
            {
                Say(Who.Fanny, "KUECHE0005-05", 49, last: true);
            }
        }
    }

    void TheBreadBin()
    {
        TaskHandle? ready = null;
        Walk(Hero, 315, 400);
        ready = State(Hero, TakeMidUpLeft);
        MoveObj("LOC03Brotkasten1", 257, 280);
        if (Var(Vars.BreadBinOpen) == 0)
        {
            ObjAfter("LOC05Brotkasten1", true, ready, 3);
            Sound(@"wav\fx\brotauf.wav", ready);
            SetVar(Vars.BreadBinOpen, 1);
        }
        else if (Var(Vars.BreadBinOpen) == 1)
        {
            ObjAfter("LOC05Brotkasten1", false, ready, 3);
            Sound(@"wav\fx\brotzu.wav", ready);
            SetVar(Vars.BreadBinOpen, 0);
        }
    }

    void TakeTheToast()
    {
        TaskHandle? ready = null;
        SetVar(Vars.TakingToast, 1);
        Walk(Hero, 340, 400);
        ready = State(Hero, TakeMidRight);
        ObjAfter("LOC05Toast", false, ready, 4);
    }

    void TakeTheBottle()
    {
        TaskHandle? ready = null;
        SetVar(Vars.PlacingOliveOil, 0);
        Walk(Hero, 330, 400);
        ready = State(Hero, TakeLowUpLeft);
        ObjAfter("LOC05Flasche", false, ready, 4);
    }

    void Cupboard2()
    {
        Walk(Hero, 188, 433);
        State(Hero, LookUpLeft);
        if (Var(Vars.Cupboard2Seen) == 0)
        {
            SetVar(Vars.Cupboard3Open, 1);
        }
    }

    void Cupboard3()
    {
        TaskHandle? ready = null;
        if (Var(Vars.Cupboard3Open) == 0)
        {
            Walk(Hero, 140, 433);
            ready = State(Hero, TakeLowUpLeft);
            Sound(@"wav\fx\hsauf1.wav", ready, 3);
            Anim("DoOpenSchrank3", after: ready, delay: 5, endObject: "LOC05OpenSchrank3Obj", endObjectOn: true);
            SetVar(Vars.Cupboard3Open, 1);
        }
        else if (Var(Vars.Cupboard3Open) == 1)
        {
            Walk(Hero, 188, 433);
            ready = State(Hero, TakeLowUpLeft);
            Sound(@"wav\fx\hszu1.wav", ready, 3);
            ObjAfter("LOC05OpenSchrank3Obj", false, ready, 5);
            Anim("DoCloseSchrank3", after: ready, delay: 5);
            SetVar(Vars.Cupboard3Open, 0);
        }
    }

    void Cupboard4()
    {
        TaskHandle? ready = null;
        if (Var(Vars.Cupboard4Open) == 0)
        {
            Walk(Hero, 340, 400);
            ready = State(Hero, TakeLowUpLeft);
            Sound(@"wav\fx\hsauf2.wav", ready, 3);
            Anim("DoOpenSchrank4", after: ready, delay: 5, endObject: "LOC05OpenSchrank4Obj", endObjectOn: true);
            SetVar(Vars.Cupboard4Open, 1);
            if (Var(Vars.OliveOilTaken) == 0)
            {
                ObjAfter("LOC05Flasche", true, ready, 4);
            }
        }
        else if (Var(Vars.Cupboard4Open) == 1)
        {
            Walk(Hero, 360, 380);
            ready = State(Hero, TakeLowUpLeft);
            Sound(@"wav\fx\hszu2.wav", ready, 3);
            ObjAfter("LOC05OpenSchrank4Obj", false, ready, 5);
            Anim("DoCloseSchrank4", after: ready, delay: 5);
            if (Var(Vars.OliveOilTaken) == 0)
            {
                ObjAfter("LOC05Flasche", false, ready, 8);
            }
            SetVar(Vars.Cupboard4Open, 0);
        }
    }

    void Cupboard6()
    {
        TaskHandle? ready = null;
        if (Hero != George)
        {
            if (Var(Vars.Cupboard6Open) == 0)
            {
                Walk(Hero, 240, 400);
                ready = State(Hero, OpenDown);
                Sound(@"wav\fx\hsauf1.wav", ready, 3);
                Anim("DoOpenSchrank6", after: ready, delay: 1, endObject: "LOC05OpenSchrank6Obj", endObjectOn: true);
                SetVar(Vars.Cupboard6Open, 1);
            }
            else if (Var(Vars.Cupboard6Open) == 1)
            {
                Walk(Hero, 265, 400);
                ready = State(Hero, OpenDown);
                Sound(@"wav\fx\hszu1.wav", ready, 3);
                ObjAfter("LOC05OpenSchrank6Obj", false, ready, 5);
                Anim("DoCloseSchrank6", after: ready, delay: 5);
                SetVar(Vars.Cupboard6Open, 0);
            }
        }
    }

    void TakeGeorgesSandwich()
    {
        TaskHandle? ready = null;
        AnimPos("GeorgeTakeSandwich", 85, 290);
        AnimPos("GeorgeTakeSandwichArm", 130, 356);
        AnimScale("GeorgeTakeSandwich", 1.0);
        AnimScale("GeorgeTakeSandwichArm", 1.0);
        StopAnim("GeorgeSitDownRight");
        Obj("LOC05GeorgesSandwich", false);
        StopAnimAfter("GeorgesBreadStill", null, 3);
        ready = Anim("GeorgeTakeSandwichArm");
        Anim("GeorgeTakeSandwich");
        Give(Item.Sandwich);
        Anim("GeorgeSitDownRight", -1, ready);
    }

    void ToTheHall()
    {
        TaskHandle? ready = null;
        if (At(3, 3))
        {
            if (Var(Vars.Distracted) == 0)
            {
                // George starts to get up and sits down again.
                Depth(@"\bmp\obj\george\gesu\gesu06an.brg", 2200);
                AnimPos("GeorgeStandUpSitDown", 130, 252);
                AnimScale("GeorgeStandUpSitDown", 1.0);
                StopAnimAfter("GeorgeSitDownRight", null);
                TaskHandle? halfUp = Anim("GeorgeStandUpSitDown");
                Anim("GeorgeSitDownRight", -1, halfUp);
                Chain = null;
                if (Var(Vars.GeorgeLeavingTalk) == 0)
                {
                    Say(Who.Quentin, "KUECHE0005-05", 17);
                    Say(Who.George, "KUECHE0005-05", 18);
                    Say(Who.Quentin, "KUECHE0005-05", 19, last: true);
                    SetVar(Vars.GeorgeLeavingTalk, 1);
                }
                else if (Var(Vars.GeorgeLeavingTalk) == 1)
                {
                    Say(Who.Quentin, "KUECHE0005-05", 20);
                    Say(Who.George, "KUECHE0005-05", 21);
                    Say(Who.Quentin, "KUECHE0005-05", 22, last: true);
                    SetVar(Vars.GeorgeLeavingTalk, 2);
                }
                else if (Var(Vars.GeorgeLeavingTalk) == 2)
                {
                    Say(Who.Quentin, "KUECHE0005-05", 23, last: true);
                }
            }
        }
        else if (At(5, 0))
        {
            PointerOff();
            ready = null;
            Chain = null;
            SetText(StoryTexts.KitchenTopic, "George");
            ready = GeorgeGetsUp(ready, 0, "GeorgeSitDownRight", 500, 600);
            Say(Who.Dick, "KUECHE0009-05", 8, after: ready, delay: 60);
            Say(Who.George, "KUECHE0009-05", 9);
            Say(Who.George, "KUECHE0009-05", 15);
            Say(Who.Julian, "KUECHE0009-05", 10);
            Say(Who.George, "KUECHE0009-05", 11);
            Say(Who.Anne, "KUECHE0009-05", 12, last: true);
        }
        else
        {
            Walk(Hero, 570, 435);
            ready = State(Hero, TakeMidRight);
            if (At(7, 3) && Hero != George)
            {
                Say(Who.Julian, "KUECHE0027", 7, after: ready);
                ObjAfter("LOC05Zettel", false, ready);
                ObjAfter("LOC05ZettelObj", false, ready);
                PointerAfter(false, ready, x: 100, y: 200);
                Sound(@"wav\fx\hotueauf.wav", ready, 3);
                ready = Anim("DoOpenDoor", after: ready, delay: 5, endObject: "LOC05OpenDoorObj", endObjectOn: true);
                ChangeLocation("hand2", ready, 20);
            }
            else
            {
                ObjAfter("LOC05Zettel", false, ready);
                ObjAfter("LOC05ZettelObj", false, ready);
                PointerAfter(false, ready, x: 100, y: 200);
                Sound(@"wav\fx\hotueauf.wav", ready, 3);
                ready = Anim("DoOpenDoor", after: ready, delay: 5, endObject: "LOC05OpenDoorObj", endObjectOn: true);
                PointerAfter(true, ready, x: 100, y: 200);
                ChangeLocation("loc04", ready);
            }
        }
    }

    void TheTeapot()
    {
        TaskHandle? ready = null;
        if (At(3, 3))
        {
            PointerOff();
            SetText(StoryTexts.KitchenTopic, "Teekanne");
            Chain = null;
            if (Var(Vars.QuentinTeapotTalk) == 0)
            {
                Say(Who.George, "KUECHE0005-05", 43);
                Say(Who.Quentin, "KUECHE0005-05", 44, last: true);
            }
            else if (Var(Vars.QuentinTeapotTalk) == 1)
            {
                Say(Who.George, "KUECHE0005-05", 45);
                Say(Who.Quentin, "KUECHE0005-05", 46, last: true);
            }
            else if (Var(Vars.QuentinTeapotTalk) == 2)
            {
                Say(Who.George, "KUECHE0005-05", 47);
                Say(Who.Quentin, "KUECHE0005-05", 48, last: true);
            }
        }
        else
        {
            Walk(Hero, 60, 456);
            ready = State(Hero, TakeMidUpRight);
            if (Quest > 6)
            {
                SetVar(Vars.TakingTeapot, 1);
                ObjAfter("LOC05Kanne", false, ready, 5);
            }
        }
    }

    // ---- arriving --------------------------------------------------------------------------

    /// <summary>
    /// Lays out the kitchen: the rain in quest 5, the stove, the door, the cupboards, the teapot,
    /// the bread bin, the olive oil, the toast and Aunt Fanny, and the scene of the story step.
    /// </summary>
    /// <remarks>The original's FUN_00434e1c.</remarks>
    protected override void Setup()
    {
        StopAnim("Regen");
        MoveObj("LOC05Zettel", 397, 411);
        Obj("LOC05Zettel", false);
        Obj("LOC05ZettelObj", false);
        Obj("LOC05Anne", false);
        Obj("LOC05Dick", false);
        Obj("LOC05Julian", false);
        Obj("LOC05George", false);
        Obj("LOC05Timmy", false);
        Music("");
        if (Quest == 5)
        {
            Anim("Regen", -1);
            Music(@"wav\atmo\gewkueche.wav");
        }
        else
        {
            Music(@"wav\atmo\hausvoll.wav");
        }
        PointerOn();
        Obj("LOC05Quentin", false);
        MoveObj("LOC05Dosenoeffner", 540, 320);
        Obj("LOC05Spueltuch", false);
        PathEnable(2, false);
        PathEnable(6, false);
        PathEnable(4, false);
        SetVar(Vars.Distracted, 0);
        Obj("LOC05Tisch", false);
        ComeIn();

        SetVar(Vars.KitchenSandwich, 1);
        if (At(7, 4) && !Has(Item.Plate))
            SetVar(Vars.KitchenSandwich, 0);
        SetUpTheStove();
        SetUpTheDoor();
        SetUpCupboard(3, 80, 224, Vars.Cupboard3Available, Vars.Cupboard3Open);
        SetUpCupboard(4, 284, 189, Vars.Cupboard4Available, Vars.Cupboard4Open);
        SetUpCupboard(6, 267, 313, Vars.Cupboard6Available, Vars.Cupboard6Open);
        SetUpTheTeapot();
        SetUpTheBreadBin();
        SetUpTheOliveOil();
        SetUpTheToast();
        SetUpFanny();

        if (!IsShown(Hero))
        {
            // The hero sits at the table: George's chair is clickable and the paths round the table open.
            if (At(3, 3))
            {
                Hero.X = 246;
                Hero.Y = 547;
            }
            PathEnable(2, true);
            PathEnable(6, true);
            PathEnable(4, true);
            Obj("LOC05George", true);
        }
        else
        {
            Obj("LOC05George", false);
            SetVar(Vars.OriginalPlaceFlag, 1);
        }
        if (!At(5, 0) && !At(12, 1) && IsShown(Quentin))
        {
            Hide(Quentin);
            Obj("LOC05Quentin", false);
        }
    }

    /// <summary>
    /// The hero comes in: through the hall door (unless at the table in quest 6), from the garden
    /// (Timmy stops what he was doing) or from elsewhere, then the story step is set up.
    /// </summary>
    /// <remarks>The original's FUN_00436144.</remarks>
    void ComeIn()
    {
        switch (FromId)
        {
            case 4:
                // As in the original: any step outside episode 1 and quest 6.
                if (Episode != 1 && Quest != 6)
                    WalkInFromTheHall();
                break;
            case 8:
                ClearQueue(Timmy);
                break;
            case 57:
            case 58:
                break;
            default:
                if (!At(3, 3) && !At(5, 0) && !At(12, 1))
                {
                    Place(Hero, 300, 300, StandDownLeft);
                    Show(Hero);
                    ShowParty();
                }
                break;
        }
        SetUpStoryStep();
    }

    /// <summary>The hero comes through the hall door and walks into the room with the others.</summary>
    void WalkInFromTheHall()
    {
        Place(Hero, 570, 444, StandDownLeft);
        Show(Hero);
        ShowParty();
        Walk(Hero, 300, 300);
        State(Hero, StandDownLeft);
    }

    /// <summary>The hero comes through the hall door and walks up to the table; returns the hero's final turn.</summary>
    TaskHandle WalkInToTheTable()
    {
        Place(Hero, 570, 444, StandDownLeft);
        Show(Hero);
        ShowParty();
        Walk(Hero, 520, 438);
        return State(Hero, StandDownLeft);
    }

    /// <summary>The stove door is drawn open if it was left open.</summary>
    /// <remarks>The original's FUN_004356f1.</remarks>
    void SetUpTheStove()
    {
        MoveObj("LOC05OpenOfenObj", 97, 329);
        Obj("LOC05OpenOfenObj", false);
        StopAnim("DoOpenOfen");
        StopAnim("DoCloseOfen");
        if (Var(Vars.StoveAvailable) == 1 && Var(Vars.StoveOpen) == 1)
            Obj("LOC05OpenOfenObj", true);
    }

    /// <summary>
    /// The hall door: open in quest 5 step 0 (Uncle Quentin is about to leave), otherwise it
    /// swings shut behind someone coming in — but not in the scenes that start inside the kitchen.
    /// </summary>
    /// <remarks>The original's FUN_00435caa.</remarks>
    void SetUpTheDoor()
    {
        Depth(@"bmp\obj\locs\loc05\door.brg", 6000);
        Depth(@"bmp\obj\locs\loc05\door04.brg", 6000);
        MoveObj("LOC05OpenDoorObj", 599, 183);
        Obj("LOC05OpenDoorObj", false);
        StopAnim("DoOpenDoor");
        if (Var(Vars.KitchenDoorAvailable) != 1 || FromId == LocationId || At(3, 3) || At(6, 1) || At(12, 1))
            return;
        if (At(5, 0))
        {
            Obj("LOC05OpenDoorObj", true);
        }
        else
        {
            Anim("DoCloseDoor");
            Sound(@"wav\fx\hotuezu.wav", delay: 3);
        }
    }

    /// <summary>Cupboard <paramref name="number"/> is drawn open (at x, y) if it was left open.</summary>
    /// <remarks>The original's FUN_00435b03, FUN_00435b90 and FUN_00435c1d.</remarks>
    void SetUpCupboard(int number, int x, int y, string available, string open)
    {
        string openObject = $"LOC05OpenSchrank{number}Obj";
        MoveObj(openObject, x, y);
        Obj(openObject, false);
        StopAnim($"DoOpenSchrank{number}");
        StopAnim($"DoCloseSchrank{number}");
        if (Var(available) == 1 && Var(open) == 1)
            Obj(openObject, true);
    }

    /// <summary>The teapot stands on the stove unless someone has it (state 1 or 2).</summary>
    /// <remarks>The original's FUN_00435dd1.</remarks>
    void SetUpTheTeapot()
    {
        SetVar(Vars.TakingTeapot, 0);
        int teapot = Var(Vars.KitchenTeapot);
        if (Var(Vars.TeapotAvailable) == 1 && teapot <= 2)
        {
            MoveObj("LOC05Kanne", 81, 307);
            Obj("LOC05Kanne", teapot == 0);
        }
    }

    /// <summary>The bread bin is drawn open if it was left open.</summary>
    /// <remarks>The original's FUN_0043577e.</remarks>
    void SetUpTheBreadBin()
    {
        MoveObj("LOC03Brotkasten1", 257, 280); // (sic) the original moves a hall object here
        Obj("LOC05Brotkasten1", false);
        if (Var(Vars.BreadBinAvailable) == 1 && Var(Vars.BreadBinOpen) == 1)
            Obj("LOC05Brotkasten1", true);
    }

    /// <summary>The olive oil stands in cupboard 4 while the cupboard is open and the bottle not taken.</summary>
    /// <remarks>The original's FUN_0043587d.</remarks>
    void SetUpTheOliveOil()
    {
        // Switching the bottle off below must not count as taking it (see ObjectChanged).
        SetVar(Vars.PlacingOliveOil, 1);
        MoveObj("LOC05Flasche", 287, 225);
        Obj("LOC05Flasche", false);
        if (Var(Vars.OliveOilTaken) == 0 && Var(Vars.Cupboard4Open) == 1)
            Obj("LOC05Flasche", true);
    }

    /// <summary>The plate of toast is on the side unless someone has it (state 1 or 2).</summary>
    /// <remarks>The original's FUN_004357f7.</remarks>
    void SetUpTheToast()
    {
        SetVar(Vars.TakingToast, 0);
        int sandwich = Var(Vars.KitchenSandwich);
        if (Var(Vars.ToastAvailable) == 1 && sandwich <= 2)
            Obj("LOC05Toast", sandwich == 0);
    }

    /// <summary>
    /// Aunt Fanny stands at the stove (talk stages 0-3 and 5); in stages 0-3 Anne and Julian sit
    /// at the table with her, and Dick and Uncle Quentin are not there.
    /// </summary>
    /// <remarks>The original's FUN_00435134.</remarks>
    void SetUpFanny()
    {
        StopAnim("FannyStandRight");
        ObjAfter("LOC05FannyStand", false, null);
        if (Var(Vars.FannyKitchenAvailable) != 1)
            return;
        int stage = Var(Vars.FannyTalk);
        if (stage is >= 0 and <= 3 or 5)
        {
            AnimPos("FannyStandRight", 190, 237);
            AnimScale("FannyStandRight", 0.45);
            Anim("FannyStandRight", -1);
            ObjAfter("LOC05FannyStand", true, null);
        }
        if (stage is >= 0 and <= 3)
        {
            ObjAfter("LOC05Anne", true, null);
            ObjAfter("LOC05Julian", true, null);
            SeatAnneAndJulian();
            ObjAfter("LOC05Dick", false, null);
            ObjAfter("LOC05Quentin", false, null);
        }
    }

    /// <summary>Sets the scene of the current story step.</summary>
    /// <remarks>The original's FUN_0043968d.</remarks>
    void SetUpStoryStep()
    {
        if (At(3, 3))
        {
            // Breakfast, played as George: Timmy begs under the table while Uncle Quentin is watching.
            World.Hero = George;
            SetVar(Vars.StudyDoorState, 1);
            SetTheBreakfastTable(georgeSits: true, anneEats: true);
            QuentinAtTheTable();
            SetVar(Vars.FannyTalk, 5);
            Place(Timmy, 200, 564, SitRight);
            ShowParty();
            Obj("LOC05Timmy", true);
            Obj("LOC05George", true);
            Depth(@"\bmp\obj\george\gefe\gefe16a.brg", 1900);
            AnimPos("GeorgeFeed", 85, 290);
            AnimPos("GeorgeFeedArm", 130, 356);
            AnimScale("GeorgeFeed", 1.0);
            AnimScale("GeorgeFeedArm", 1.0);
            AnimPos("GeorgeTryFeed", 85, 290);
            AnimPos("GeorgeTryFeedArm", 130, 356);
            AnimScale("GeorgeTryFeed", 1.0);
            AnimScale("GeorgeTryFeedArm", 1.0);
            if (Has(Item.Sandwich))
            {
                StopAnim("GeorgesBreadStill");
                Obj("LOC05GeorgesSandwich", false);
            }
            else
            {
                Depth(@"\bmp\obj\george\geea\bread.brg", 1900);
                AnimPos("GeorgesBreadStill", 179, 434);
                AnimScale("GeorgesBreadStill", 0.9);
                Anim("GeorgesBreadStill", -1);
                Obj("LOC05GeorgesSandwich", true);
            }
            PointerOff();
            Chain = null;
            Say(Who.Fanny, "KUECHE0005-05", 1);
            Say(Who.Quentin, "KUECHE0005-05", 9, last: true);
        }
        else if (At(3, 4))
        {
            // Uncle Quentin's question at breakfast (answered from the menu, see AfterChoice).
            SetTheBreakfastTable(georgeSits: false, anneEats: true);
            QuentinAtTheTable();
            SetVar(Vars.FannyTalk, 5);
            Chain = null;
            if (Var(Vars.StudyDoorState) < 5)
            {
                PointerOff();
                SetText(StoryTexts.KitchenTopic, "DLG");
                Say(Who.Quentin, "KUECHE0006-05", 1, delay: 10);
                Say(Who.Julian, "KUECHE0006-05", 2, last: true);
            }
        }
        else if (At(4, 1))
        {
            SetTheBreakfastTable(georgeSits: false, anneEats: true);
            QuentinAtTheTable();
            SetVar(Vars.FannyTalk, 5);
            WalkInFromTheHall();
        }
        else if (At(5, 0))
        {
            // Uncle Quentin and George at breakfast; he leaves afterwards (see ConversationEnding).
            SetVar(Vars.AnneHallTalk, 5);
            SetVar(Vars.DickHallTalk, 5);
            SetVar(Vars.JulianHallTalk, 5);
            SetVar(Vars.GuestRoomKeyPlan, 0);
            SetVar(Vars.KeySeenInLock, 0);
            SetVar(Vars.FannyTalk, 4);
            SetVar(Vars.OriginalPlaceFlag, 0);
            Place(Quentin, 540, 440, StandDownLeft);
            Place(Timmy, 799, 440, StandDownLeft);
            Obj("LOC05ExitToLOC04", false);
            SetTheBreakfastTable(georgeSits: true, anneEats: true);
            Chain = null;
            PointerOff();
            Say(Who.Quentin, "KUECHE0009-05", 1);
            Say(Who.George, "KUECHE0009-05", 2);
            Say(Who.Quentin, "KUECHE0009-05", 3);
            Say(Who.George, "KUECHE0009-05", 4);
            Say(Who.Quentin, "KUECHE0009-05", 5);
            Say(Who.George, "KUECHE0009-05", 6);
            Say(Who.Quentin, "KUECHE0009-05", 7);
            Say(Who.Quentin, "KUECHE0009-05", 15, last: true);
            ShowParty();
        }
        else if (At(5, 1))
        {
            if (!IsShown(Hero))
            {
                Place(Hero, 570, 444, StandDownLeft);
                Show(Hero);
                ShowParty();
            }
            Walk(Hero, 300, 300);
            State(Hero, StandDownLeft);
            SetVar(Vars.BreakfastOver, 1);
            SetVar(Vars.FannyTalk, 4);
        }
        else if (At(5, 2))
        {
            SetVar(Vars.BreakfastOver, 1);
            SetVar(Vars.FannyTalk, 4);
        }
        else if (At(5, 4))
        {
            ChildrenAtTheTable();
        }
        else if (At(5, 5))
        {
            ChildrenAtTheTable();
            WalkInToTheTable();
            Chain = null;
            PointerOff();
            Say(Who.Anne, "KUECHE0014-05", 12);
            Say(Who.George, "KUECHE0014-05", 13);
            Say(Who.Dick, "KUECHE0014-05", 14, last: true);
        }
        else if (Quest == 5 && Episode > 5)
        {
            ChildrenAtTheTable();
            Chain = WalkInToTheTable();
            PointerOff();
            if (Var(Vars.UnderBedTalk) != 0)
            {
                Say(Who.Anne, "KUECHE0015-05", 11);
                Say(Who.George, "KUECHE0015-05", 12, last: true);
            }
            else if (Episode == 6)
            {
                Say(Who.Dick, "KUECHE0015-05", 1);
                Say(Who.George, "KUECHE0015-05", 2);
                Say(Who.Julian, "KUECHE0015-05", 3);
                Say(Who.George, "KUECHE0015-05", 4);
                Say(Who.Dick, "KUECHE0015-05", 5);
                Say(Who.George, "KUECHE0015-05", 6, last: true);
            }
            else
            {
                Say(Who.Julian, "KUECHE0015-05", 7);
                Say(Who.George, "KUECHE0015-05", 8);
                Say(Who.Dick, "KUECHE0015-05", 9);
                Say(Who.George, "KUECHE0015-05", 10, last: true);
            }
        }
        else if (At(6, 1))
        {
            // George reads out of a book at the table; the children then go (see ConversationEnding).
            ChildrenAtTheTable();
            Depth(@"\bmp\obj\george\gere\gere11an.brg", 3500);
            AnimPos("GeorgeWithBook", 155, 225);
            AnimScale("GeorgeWithBook", 1.0);
            AnimPos("GeorgeSpeakWithBook", 155, 225);
            AnimScale("GeorgeSpeakWithBook", 1.0);
            Anim("GeorgeWithBook", -1);
            Chain = null;
            PointerOff();
            Say(Who.Dick, "KUECHE0016-05", 1, delay: 2);
            Say(Who.George, "KUECHE0016-05", 2);
            Say(Who.Anne, "KUECHE0016-05", 3);
            Say(Who.George, "KUECHE0016-05", 4);
            Say(Who.George, "KUECHE0016-05", 13);
            Say(Who.Dick, "KUECHE0016-05", 5);
            Say(Who.George, "KUECHE0016-05", 6);
            Say(Who.George, "KUECHE0016-05", 14);
            Say(Who.Dick, "KUECHE0016-05", 7);
            Say(Who.George, "KUECHE0016-05", 8);
            Say(Who.Julian, "KUECHE0016-05", 9);
            Say(Who.Anne, "KUECHE0016-05", 10);
            Say(Who.Julian, "KUECHE0016-05", 11);
            Say(Who.Dick, "KUECHE0016-05", 12, last: true);
        }
        else if (At(7, 4))
        {
            TeaTime();
        }
        else if (At(7, 5))
        {
            PutBackTheTeaThings();
            SetVar(Vars.FannyTalk, 4);
            SetTheBreakfastTable(georgeSits: false, anneEats: false);
            AnneMakesSandwiches();
            Place(Timmy, 799, 440, StandDownLeft);
            Show(Timmy);
            NoQuentinTimmyOrGeorge();
            Chain = null;
            if (Var(Vars.TeaPlanStage) == 0)
            {
                PointerOff();
                SetVar(Vars.TeaPlanStage, 1);
                Say(Who.Anne, "KUECHE0033", 1, delay: 12);
                Say(Who.George, "KUECHE0033", 2, last: true);
            }
        }
        else if (At(7, 6))
        {
            ClearTheTable(alsoQuentinAndTimmy: true);
            WalkInFromTheHall();
        }
        else if (At(12, 1))
        {
            PoliceAtBreakfast();
        }
        else if (Quest < 6)
        {
            ShowParty();
        }
        else if (Quest == 6)
        {
            ClearTheTable(alsoQuentinAndTimmy: false);
            WalkInFromTheHall();
        }
        else if (Quest == 7)
        {
            if (Episode < 4)
            {
                Obj("LOC05Zettel", true);
                Obj("LOC05ZettelObj", true);
            }
            if (Var(Vars.KitchenSandwich) == 1)
                SetVar(Vars.KitchenSandwich, 0);
            if (Var(Vars.QuentinTeapotTalk) == 1)
                SetVar(Vars.QuentinTeapotTalk, 0);
            ClearTheTable(alsoQuentinAndTimmy: false);
            if (Episode != 2 && Episode != 3)
                WalkInFromTheHall();
        }
        else
        {
            ClearTheTable(alsoQuentinAndTimmy: true);
            WalkInFromTheHall();
        }
    }

    /// <summary>
    /// Quest 7 step 4: tea time. Anne makes sandwiches; what the children say depends on how far
    /// the plan to bring Uncle Quentin his tea (and to look at the study fire) has got.
    /// </summary>
    void TeaTime()
    {
        SetVar(Vars.FannyTalk, 4);
        SetTheBreakfastTable(georgeSits: false, anneEats: false);
        AnneMakesSandwiches();
        Place(Timmy, 799, 440, StandDownLeft);
        Show(Timmy);
        if (Var(Vars.StudyTeaVisit) == 1)
        {
            if (Var(Vars.TeaPlanStage) == 2 || Var(Vars.StudyFireOut) != 0)
                BackFromTheStudy();
            else if (Var(Vars.TeaNotBrought) == 0)
            {
                ChildrenTalkFrom(6);
                Chain = null;
                PointerOff();
                Say(Who.Anne, "KUECHE0032", 10, delay: 8);
                Say(Who.George, "KUECHE0032", 11);
                Say(Who.Dick, "KUECHE0032", 12);
                Say(Who.George, "KUECHE0032", 13, last: true);
            }
            else
            {
                ChildrenTalkFrom(4);
                Chain = null;
                PointerOff();
                Say(Who.Dick, "KUECHE0032", 1, delay: 8);
                Say(Who.George, "KUECHE0032", 2);
                Say(Who.Julian, "KUECHE0032", 3);
                Say(Who.Anne, "KUECHE0032", 4, last: true);
            }
            SetVar(Vars.TeaPlanStage, 2);
        }
        else if (Var(Vars.TeaPlanStage) == 0)
        {
            SetVar(Vars.TeaPlanStage, 1);
            Chain = null;
            PointerOff();
            SetText(StoryTexts.KitchenTopic, "start");
            Say(Who.Julian, "KUECHE0031", 1, delay: 8);
            Say(Who.George, "KUECHE0031", 2);
            Say(Who.Dick, "KUECHE0031", 3);
            Say(Who.Anne, "KUECHE0031", 4, last: true);
        }
        else if (Var(Vars.TeaPlanStage) == 1)
        {
            ChildrenTalkFrom(2);
        }
        NoQuentinTimmyOrGeorge();
        PutBackTheTeaThings();
    }

    /// <summary>Tea time, back from the study: what the children make of the fire and the window.</summary>
    void BackFromTheStudy()
    {
        Chain = null;
        PointerOff();
        Say(Who.Julian, "KUECHE0032", 25, last: true, delay: 4);
        if (Var(Vars.FirewoodTalk) == 2 && Var(Vars.StudyWindowTalk) < 2)
        {
            Say(Who.Dick, "KUECHE0032", 44, delay: 8);
            Say(Who.George, "KUECHE0032", 45);
            Say(Who.Julian, "KUECHE0032", 46, last: true);
            SetVar(Vars.AnneTalk, 11);
            SetVar(Vars.DickTalk, 11);
            SetVar(Vars.JulianTalk, 11);
        }
        else if (Var(Vars.StudyFire) == 0 && Var(Vars.FirewoodTalk) == 0)
        {
            Say(Who.George, "KUECHE0032", 40, delay: 10);
            Say(Who.Dick, "KUECHE0032", 41);
            Say(Who.George, "KUECHE0032", 42);
            Say(Who.Anne, "KUECHE0032", 43, last: true);
            SetVar(Vars.AnneTalk, 11);
            SetVar(Vars.DickTalk, 11);
            SetVar(Vars.JulianTalk, 11);
        }
        else if (Var(Vars.StudyWindowTalk) == 1)
        {
            Say(Who.George, "KUECHE0032", 26, delay: 10);
            Say(Who.Julian, "KUECHE0032", 27);
            Say(Who.Dick, "KUECHE0032", 28, last: true);
            if (Var(Vars.FirewoodTalk) == 1)
                Say(Who.George, "KUECHE0032", 29, last: true);
            else
                SetText(StoryTexts.KitchenTopic, "change");
            ChildrenTalkFrom(9);
        }
    }

    /// <summary>Quest 12 step 1: the policeman brings news at breakfast, and the story ends (see ConversationEnding).</summary>
    void PoliceAtBreakfast()
    {
        PointerOff();
        SetVar(Vars.Cupboard1Seen, 0);
        SetVar(Vars.Cupboard2Seen, 0);
        SetVar(Vars.Cupboard3Open, 0);
        SetVar(Vars.Cupboard4Open, 0);
        SetVar(Vars.Cupboard5Seen, 0);
        SetVar(Vars.Cupboard6Open, 0);
        SetTheBreakfastTable(georgeSits: true, anneEats: true);
        SetVar(Vars.FannyTalk, 5);
        Depth(@"bmp\obj\polizist\post\post.brg", 6000);
        AnimPos("PolizistStandRight", 100, 230);
        AnimScale("PolizistStandRight", 1.0);
        AnimPos("PolizistSpeakRight", 100, 230);
        AnimScale("PolizistSpeakRight", 1.0);
        Anim("PolizistStandRight", -1);
        Place(Quentin, 540, 440, StandDownLeft);
        Place(Timmy, 799, 440, StandDownLeft);
        Chain = null;
        SetText(StoryTexts.KitchenTopic, "Hund");
        Say(Who.Policeman, "KUECHE0054", 1, delay: 10);
        Say(Who.George, "KUECHE0054", 2, last: true);
        Say(Who.Anne, "KUECHE0054", 3);
        Say(Who.Fanny, "KUECHE0054", 4);
        Say(Who.Julian, "KUECHE0054", 5);
        Say(Who.Quentin, "KUECHE0054", 7);
        Say(Who.Policeman, "KUECHE0054", 8);
        Say(Who.Julian, "KUECHE0054", 9);
        Say(Who.Quentin, "KUECHE0054", 10);
        Say(Who.Anne, "KUECHE0054", 11);
        Say(Who.Julian, "KUECHE0054", 12);
        Say(Who.George, "KUECHE0054", 13);
        Say(Who.Fanny, "KUECHE0054", 15);
        Say(Who.Fanny, "KUECHE0054", 17);
        Say(Who.George, "KUECHE0054", 16, last: true);
        ShowParty();
    }

    // ---- the table ---------------------------------------------------------------------------

    /// <summary>
    /// Anne, Julian, Dick and George at the breakfast table, eating (Anne only when
    /// <paramref name="anneEats"/>; George is drawn sitting only when <paramref name="georgeSits"/>).
    /// </summary>
    /// <remarks>The original's FUN_0043c1b9.</remarks>
    void SetTheBreakfastTable(bool georgeSits, bool anneEats)
    {
        Depth(@"\bmp\obj\anne\anea\bread.brg", 1900);
        Depth(@"\bmp\obj\anne\anea\anea11.brg", 2200);
        Depth(@"\bmp\obj\julian\jusp\jusp11.brg", 2200);
        Depth(@"\bmp\obj\dick\disp\disp16.brg", 2200);
        Depth(@"\bmp\obj\george\gesp\gesp16an.brg", 2200);
        Depth(@"\bmp\obj\anne\ansp\ansp11.brg", 2200);
        Depth(@"\bmp\obj\anne\anea\anea11a.brg", 1900);
        Depth(@"\bmp\obj\julian\juea\juea11.brg", 2300);
        Depth(@"\bmp\obj\julian\juea\juea11a.brg", 1900);
        Depth(@"\bmp\obj\julian\juea\bread.brg", 1900);
        Depth(@"\bmp\obj\dick\diea\diea16.brg", 2300);
        Depth(@"\bmp\obj\dick\diea\diea16a.brg", 1900);
        Depth(@"\bmp\obj\dick\diea\bread.brg", 1900);
        Depth(@"\bmp\obj\george\geea\geea16.brg", 2300);
        Depth(@"\bmp\obj\george\geea\geea16a.brg", 1900);
        Depth(@"\bmp\obj\george\gefe\gefe16.brg", 2300);
        AnimPos("AnneSitSpeakDownLeft", 423, 296);
        AnimPos("AnnesBread", 458, 420);
        AnimPos("AnnesBreadStill", 458, 420);
        AnimPos("AnneEat", 423, 296);
        AnimPos("AnneEatArm", 425, 352);
        AnimPos("JulianSitSpeakDownLeft", 330, 270);
        AnimPos("JuliansBread", 365, 394);
        AnimPos("JuliansBreadStill", 365, 394);
        AnimPos("JulianEat", 330, 270);
        AnimPos("JulianEatArm", 330, 330);
        AnimPos("DickSitSpeakDownRight", 236, 270);
        AnimPos("DicksBread", 286, 400);
        AnimPos("DicksBreadStill", 286, 400);
        AnimPos("DickEat", 236, 270);
        AnimPos("DickEatArm", 278, 328);
        AnimPos("GeorgeSitSpeakDownRight", 138, 290);
        AnimPos("GeorgeEat", 138, 290);
        AnimPos("GeorgeEatArm", 168, 330);
        AnimPos("GeorgeSitDownRight", 86, 290);
        foreach (string anim in new[] { "AnneSitSpeakDownLeft", "AnneEat", "AnnesBread", "AnnesBreadStill", "AnneEatArm",
                     "JulianSitSpeakDownLeft", "JuliansBread", "JuliansBreadStill", "JulianEat", "JulianEatArm",
                     "DickSitSpeakDownLeft" /* (sic) */, "DicksBread", "DicksBreadStill", "DickEat", "DickEatArm" })
            AnimScale(anim, 0.9);
        foreach (string anim in new[] { "GeorgeSitSpeakDownRight", "GeorgeEat", "GeorgeEatArm", "GeorgeSitDownRight" })
            AnimScale(anim, 1.0);
        if (anneEats)
        {
            Anim("AnnesBread", -1);
            Anim("AnneEat", -1);
            Anim("AnneEatArm", -1);
        }
        Anim("JuliansBread", -1, delay: 1);
        Anim("JulianEat", -1, delay: 1);
        Anim("JulianEatArm", -1, delay: 1);
        Anim("DicksBread", -1);
        Anim("DickEat", -1);
        Anim("DickEatArm", -1);
        if (georgeSits)
            Anim("GeorgeSitDownRight", -1);
        Obj("LOC05Anne", true);
        Obj("LOC05Dick", true);
        Obj("LOC05Julian", true);
        Obj("LOC05George", true);
        Chain = null;
        Obj("LOC05Tisch", true);
    }

    /// <summary>Anne and Julian sit at the table (not eating).</summary>
    /// <remarks>The original's FUN_0043cdab.</remarks>
    void SeatAnneAndJulian()
    {
        Depth(@"\bmp\obj\julian\jusp\jusp11.brg", 2200);
        Depth(@"\bmp\obj\anne\ansp\ansp11.brg", 2200);
        AnimPos("AnneSitDownLeft", 423, 296);
        AnimPos("JulianSitDownLeft", 330, 270);
        AnimPos("AnneSitSpeakDownLeft", 423, 296);
        AnimPos("JulianSitSpeakDownLeft", 330, 270);
        AnimScale("AnneSitDownLeft", 0.9);
        AnimScale("JulianSitDownLeft", 0.9);
        AnimScale("AnneSitSpeakDownLeft", 0.9);
        AnimScale("JulianSitSpeakDownLeft", 0.9);
        Anim("AnneSitDownLeft", -1);
        Anim("JulianSitDownLeft", -1);
        Obj("LOC05Anne", true);
        Obj("LOC05Julian", true);
    }

    /// <summary>Dick sits at the table (not eating).</summary>
    /// <remarks>The original's FUN_0043cede.</remarks>
    void SeatDick()
    {
        Depth(@"\bmp\obj\dick\disp\disp16.brg", 2200);
        AnimPos("DickSitSpeakDownRight", 236, 270);
        AnimScale("DickSitSpeakDownRight", 0.9);
        AnimPos("DickSitDownRight", 236, 270);
        AnimScale("DickSitDownRight", 0.9);
        Anim("DickSitDownRight", -1);
        Obj("LOC05Dick", true);
    }

    /// <summary>Anne, Julian and Dick sit at the table; Uncle Quentin, Timmy and George are not there.</summary>
    void ChildrenAtTheTable()
    {
        SetVar(Vars.FannyTalk, 4);
        SetVar(Vars.KitchenTeapot, 1);
        SeatAnneAndJulian();
        SeatDick();
        NoQuentinTimmyOrGeorge();
    }

    /// <summary>Uncle Quentin, Timmy and George cannot be clicked.</summary>
    void NoQuentinTimmyOrGeorge()
    {
        Obj("LOC05Quentin", false);
        Obj("LOC05Timmy", false);
        Obj("LOC05George", false);
    }

    /// <summary>Nobody sits at the table (Uncle Quentin and Timmy are gone too when <paramref name="alsoQuentinAndTimmy"/>).</summary>
    void ClearTheTable(bool alsoQuentinAndTimmy)
    {
        Obj("LOC05Anne", false);
        Obj("LOC05Dick", false);
        Obj("LOC05George", false);
        Obj("LOC05Julian", false);
        if (alsoQuentinAndTimmy)
        {
            Obj("LOC05Quentin", false);
            Obj("LOC05Timmy", false);
        }
        StopAnim("AnneSitDownLeft");
        StopAnim("JulianSitDownLeft");
        StopAnim("DickSitDownLeft");
        SetVar(Vars.FannyTalk, 4);
    }

    /// <summary>The teapot and the plate go back to their places unless the hero carries them.</summary>
    void PutBackTheTeaThings()
    {
        if (!Has(Item.Teapot))
            SetVar(Vars.KitchenTeapot, 0);
        if (!Has(Item.Plate))
            SetVar(Vars.KitchenSandwich, 0);
    }

    /// <summary>Anne, Dick and Julian's talk stages move on to at least <paramref name="stage"/>.</summary>
    void ChildrenTalkFrom(int stage)
    {
        foreach (string talk in new[] { Vars.AnneTalk, Vars.DickTalk, Vars.JulianTalk })
            if (Var(talk) < stage)
                SetVar(talk, stage);
    }

    /// <summary>Uncle Quentin sits at the table with his cup of tea.</summary>
    /// <remarks>The original's FUN_0043c86a.</remarks>
    void QuentinAtTheTable()
    {
        Depth(@"\bmp\obj\quentin\qudr\qudr11.brg", 1900);
        AnimPos("QuentinDrink", 500, 300);
        AnimScale("QuentinDrink", 0.9);
        AnimPos("QuentinDrinkAll", 500, 300);
        AnimScale("QuentinDrinkAll", 0.9);
        Obj("LOC05Quentin", true);
        Depth(@"\bmp\obj\quentin\qusp\qusp11an.brg", 1900);
        AnimPos("QuentinSpeak", 480, 292);
        AnimScale("QuentinSpeak", 0.9);
        Obj("LOC05Quentin", true);
        Anim("QuentinDrink", -1);
    }

    /// <summary>Anne stands at the side making sandwiches.</summary>
    /// <remarks>The original's FUN_0043c959.</remarks>
    void AnneMakesSandwiches()
    {
        Depth(@"bmp\obj\anne\anbu\anbu04.brg", 5000);
        AnimPos("AnneMakeBread", 450, 250);
        AnimScale("AnneMakeBread", 0.8);
        MoveObj("LOC05Anne", 460, 250);
        Anim("AnneMakeBread", -1);
        Obj("LOC05Anne", true);
    }

    /// <summary>
    /// George gets up from the table <paramref name="delay"/> ticks after <paramref name="after"/>
    /// (stopping <paramref name="sitting"/>) and walks to (x, y); returns the getting-up animation.
    /// </summary>
    /// <remarks>The original's FUN_0043c744.</remarks>
    TaskHandle? GeorgeGetsUp(TaskHandle? after, int delay, string sitting, int x, int y)
    {
        Depth(@"\bmp\obj\george\gesu\gesu06an.brg", 2200);
        AnimPos("GeorgeStandUp", 130, 252);
        AnimScale("GeorgeStandUp", 1.0);
        AnimPos("GeorgeSitDown", 130, 252);
        AnimScale("GeorgeSitDown", 1.0);
        StopAnimAfter(sitting, after, delay);
        TaskHandle? standUp = Anim("GeorgeStandUp", after: after, delay: delay);
        Sound(@"wav\fx\sscharr.wav", standUp, delay);
        PlaceAfter(Hero, 246, 547, StandDownRight, standUp);
        WalkAfter(Hero, x, y, standUp, 2);
        return standUp;
    }

    // ---- talking poses -----------------------------------------------------------------------

    /// <summary>Anne, Julian or Dick for their speaker codes; null for anyone else.</summary>
    static string? ChildAtTheTable(int speaker) => speaker switch
    {
        Who.Anne => "Anne",
        Who.Julian => "Julian",
        Who.Dick => "Dick",
        _ => null,
    };

    /// <summary>A child's sitting animation (Dick sits facing right, the others left).</summary>
    static string Sitting(string child) => child == "Dick" ? "DickSitDownRight" : child + "SitDownLeft";

    /// <summary>A child's talking-while-sitting animation.</summary>
    static string TalkingAtTheTable(string child) => child == "Dick" ? "DickSitSpeakDownRight" : child + "SitSpeakDownLeft";

    /// <summary>Stops one looping animation and starts another in its place, once <paramref name="after"/> is released.</summary>
    void SwapAnim(string stop, string start, TaskHandle? after = null)
    {
        StopAnimAfter(stop, after);
        Anim(start, -1, after);
    }

    /// <summary>A child at the table puts down their bread to talk.</summary>
    void PutDownBread(string child)
    {
        StopAnimAfter(child + "sBread", null);
        StopAnimAfter(child + "Eat", null);
        StopAnimAfter(child + "EatArm", null);
        Anim(TalkingAtTheTable(child), -1, null);
        Anim(child + "sBreadStill", -1, null);
    }

    /// <summary>A child at the table goes on eating once <paramref name="end"/> is released.</summary>
    void GoOnEating(string child, TaskHandle end)
    {
        StopAnimAfter(child + "sBreadStill", end);
        StopAnimAfter(TalkingAtTheTable(child), end);
        Anim(child + "sBread", -1, end);
        Anim(child + "Eat", -1, end);
        Anim(child + "EatArm", -1, end);
    }

    /// <summary>Aunt Fanny turns from the stove to talk.</summary>
    void FannyStartsTalking()
    {
        StopAnimAfter("FannyStandRight", null);
        AnimPos("FannyStandRightTalk", 190, 240);
        AnimScale("FannyStandRightTalk", 0.89);
        Anim("FannyStandRightTalk", -1, null);
    }

    /// <summary>Uncle Quentin puts his cup down to talk.</summary>
    void QuentinPutsDownHisCup()
    {
        SwapAnim("QuentinDrink", "QuentinSpeak");
        ObjAfter("LOC05Tasse", true, null);
    }

    /// <summary>Uncle Quentin drinks his tea again once <paramref name="end"/> is released.</summary>
    void QuentinDrinksAgain(TaskHandle end)
    {
        StopAnimAfter("QuentinSpeak", end);
        ObjAfter("LOC05Tasse", false, end);
        Anim("QuentinDrink", -1, end);
    }

    /// <summary>
    /// The speaker takes their talking pose: Aunt Fanny turns from the stove, the children at the
    /// table put down their bread or look up, Uncle Quentin puts down his cup, the hero faces the room.
    /// </summary>
    /// <remarks>The original's FUN_004362d9.</remarks>
    public override void LineStarting(GameWorld world, int speaker)
    {
        int who = CurrentSpeaker;
        string topic = Text(StoryTexts.KitchenTopic);
        string? child = ChildAtTheTable(who);
        if (Episode < 4 && Quest == 2)
        {
            if (who == Who.Fanny)
                FannyStartsTalking();
            else if (who == Who.Dick)
                StateAfter(Hero, topic == "Fanny" ? SpeakDownLeft : SpeakDownRight, null);
            else if (who is Who.Anne or Who.Julian)
                SwapAnim(Sitting(child!), TalkingAtTheTable(child!));
        }
        else if (Episode < 5 && Quest == 3)
        {
            if (Episode == 4)
            {
                // The hero (George) may still be in a talking pose from the last line.
                if (Hero.State == SpeakDownLeft)
                    Hero.State = StandDownLeft;
                else if (Hero.State == SpeakDownRight)
                    Hero.State = StandDownRight;
            }
            if (who == Who.Fanny)
                FannyStartsTalking();
            else if (who == Who.Quentin)
                QuentinPutsDownHisCup();
            else if (who == Who.George && Episode == 3)
                SwapAnim("GeorgeSitDownRight", "GeorgeSitSpeakDownRight");
            else if (who == Who.George)
                StateAfter(Hero, SpeakDownLeft, null);
            else if (child != null)
                PutDownBread(child);
        }
        else if (At(5, 0))
        {
            if (who == Who.Quentin)
                StateAfter(Quentin, SpeakDownLeft, null);
            else if (child != null)
                PutDownBread(child);
            else if (who == Who.George && topic != "George")
                SwapAnim("GeorgeSitDownRight", "GeorgeSitSpeakDownRight");
            else if (who == Who.George)
                StateAfter(Hero, SpeakDownLeft, null);
        }
        else if (Episode < 8 && Quest == 5)
        {
            if (Episode == 4 && Hero.State == SpeakDownLeft)
                Hero.State = StandDownLeft;
            if (child != null)
                SwapAnim(Sitting(child), TalkingAtTheTable(child));
            else if (who == Who.George)
                StateAfter(Hero, SpeakDownLeft, null);
        }
        else if (At(6, 1))
        {
            if (child != null)
                SwapAnim(Sitting(child), TalkingAtTheTable(child));
            else if (who == Who.George)
                SwapAnim("GeorgeWithBook", "GeorgeSpeakWithBook");
        }
        else if (Episode < 4 && Quest == 7)
        {
            // "nobody": the next line is not spoken by anyone in the room, so the hero takes no pose.
            if (topic == "nobody")
                SetText(StoryTexts.KitchenTopic, "");
            else
                StateAfter(Hero, SpeakDownLeft, null);
        }
        else if (At(7, 4) || At(7, 5))
        {
            // Anne is making sandwiches and keeps at it.
            if (who is Who.Julian or Who.Dick)
                PutDownBread(child!);
            else if (who == Who.George && At(7, 4))
            {
                StateAfter(Hero, topic == "start" ? SpeakDownLeft : SpeakDownRight, null);
                SetText(StoryTexts.KitchenTopic, "");
            }
            else if (who == Who.George)
                StateAfter(Hero, SpeakDownRight, null);
        }
        else if (At(12, 1))
        {
            if (who == Who.Fanny)
                FannyStartsTalking();
            else if (who == Who.Quentin)
                StateAfter(Quentin, SpeakDownLeft, null);
            else if (who == Who.Policeman)
                SwapAnim("PolizistStandRight", "PolizistSpeakRight");
            else if (who == Who.George)
                SwapAnim("GeorgeSitDownRight", "GeorgeSitSpeakDownRight");
            else if (child != null)
                PutDownBread(child);
        }
    }

    /// <summary>
    /// The speaker leaves their talking pose once <paramref name="end"/> is released (and some
    /// lines move a character's talk stage on).
    /// </summary>
    /// <remarks>The original's FUN_004371db.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end)
    {
        int who = CurrentSpeaker;
        string? child = ChildAtTheTable(who);
        if (Episode < 4 && Quest == 2)
        {
            if (who == Who.Fanny)
            {
                StopAnimAfter("FannyStandRightTalk", end);
                AnimPos("FannyStandRight", 190, 237);
                AnimScale("FannyStandRight", 0.45);
                Anim("FannyStandRight", -1, end);
            }
            else if (who == Who.Dick)
                StateAfter(Hero, Text(StoryTexts.KitchenTopic) == "Fanny" ? StandDownLeft : StandDownRight, end);
            else if (who is Who.Anne or Who.Julian)
                SwapAnim(TalkingAtTheTable(child!), Sitting(child!), end);
        }
        else if (Episode < 5 && Quest == 3)
        {
            if (who == Who.Dick)
                SetVar(Vars.DickTalk, 1);
            if (who == Who.Quentin)
            {
                QuentinDrinksAgain(end);
                if (Var(Vars.StudyDoorState) == 5)
                    SetVar(Vars.QuentinKitchenTalk, 1);
            }
            else if (who == Who.Fanny)
                SwapAnim("FannyStandRightTalk", "FannyStandRight", end);
            else if (who == Who.George && Episode == 3)
                SwapAnim("GeorgeSitSpeakDownRight", "GeorgeSitDownRight", end);
            else if (who == Who.George)
                StateAfter(Hero, StandDownLeft, end);
            else if (child != null)
                GoOnEating(child, end);
        }
        else if (At(5, 0))
        {
            if (who == Who.Quentin)
                StateAfter(Quentin, StandDownLeft, end);
            else if (child != null)
                GoOnEating(child, end);
            else if (who == Who.George && Text(StoryTexts.KitchenTopic) != "George")
                SwapAnim("GeorgeSitSpeakDownRight", "GeorgeSitDownRight", end);
            else if (who == Who.George)
                StateAfter(Hero, StandDownLeft, end);
        }
        else if (Episode < 8 && Quest == 5)
        {
            if (child != null)
                SwapAnim(TalkingAtTheTable(child), Sitting(child), end);
            else if (who == Who.George)
                StateAfter(Hero, StandDownLeft, end);
        }
        else if (At(6, 1))
        {
            if (child != null)
                SwapAnim(TalkingAtTheTable(child), Sitting(child), end);
            else if (who == Who.George)
                SwapAnim("GeorgeSpeakWithBook", "GeorgeWithBook", end);
        }
        else if (At(7, 4) || At(7, 5))
        {
            if (who is Who.Julian or Who.Dick)
                GoOnEating(child!, end);
            else if (who == Who.George)
                StateAfter(Hero, StandDownRight, end);
            if (At(7, 5))
                SetText(StoryTexts.KitchenTopic, "");
        }
        else if (At(12, 1))
        {
            if (who == Who.Dick)
                SetVar(Vars.DickTalk, 1);
            if (who == Who.Quentin)
                StateAfter(Quentin, StandDownLeft, end);
            else if (who == Who.Policeman)
                SwapAnim("PolizistSpeakRight", "PolizistStandRight", end);
            else if (who == Who.Fanny)
                SwapAnim("FannyStandRightTalk", "FannyStandRight", end);
            else if (who == Who.George)
                SwapAnim("GeorgeSitSpeakDownRight", "GeorgeSitDownRight", end);
            else if (child != null)
                GoOnEating(child, end);
        }
    }

    /// <summary>
    /// The last line of a conversation is over: the speaker leaves their talking pose and the
    /// story moves on — Uncle Quentin leaves the breakfast table, George pours his tea or slips
    /// out with the sandwich, the children go off after George's book, the story ends.
    /// </summary>
    /// <remarks>The original's FUN_0043800c.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        int who = CurrentSpeaker;
        string topic = Text(StoryTexts.KitchenTopic);
        string? child = ChildAtTheTable(who);
        if (At(5, 0))
            AfterQuentinAndGeorge(end, who, topic, child);
        else if (Episode < 4 && Quest == 2)
        {
            StateAfter(Hero, topic == "Fanny" ? StandDownLeft : StandDownRight, end);
            StopAnimAfter("FannyStandRightTalk", end);
            if (who is Who.Anne or Who.Julian)
                SwapAnim(TalkingAtTheTable(child!), Sitting(child!), end);
            AnimPos("FannyStandRight", 190, 237);
            AnimScale("FannyStandRight", 0.45);
            Anim("FannyStandRight", -1, end);
        }
        else if (Episode < 5 && Quest == 3)
            AfterBreakfastTalk(end, who, topic, child);
        else if (Episode < 8 && Quest == 5)
        {
            if (who == Who.George)
            {
                StateAfter(Hero, StandDownLeft, end);
                return;
            }
            if (child == null)
                return;
            SwapAnim(TalkingAtTheTable(child), Sitting(child), end);
            string talk = child == "Anne" ? Vars.AnneTalk : child == "Dick" ? Vars.DickTalk : Vars.JulianTalk;
            if (Episode < 5 && Var(talk) == 0)
            {
                PointerAfter(false, end, x: 100, y: 200);
                Ask(Who.George, "KUECHE0014-05", 4, after: end);
            }
            bool underBedTalked = Var(Vars.UnderBedTalk) > 0;
            if (child == "Anne" && underBedTalked)
                SetVar(Vars.AnneTalk, 4);
            else if (child == "Dick")
            {
                if (underBedTalked)
                    SetVar(Vars.DickTalk, 4);
                if (Var(Vars.DickTalk) == 1)
                    SetVar(Vars.DickTalk, 2);
            }
            else if (child == "Julian" && underBedTalked && Var(Vars.JulianTalk) is 3 or 4)
                SetVar(Vars.JulianTalk, Var(Vars.JulianTalk) + 1);
        }
        else if (At(6, 1))
        {
            // The children get up from the table and leave for the garden.
            SwapAnim("DickSitSpeakDownRight", "DickSitDownRight", end);
            PointerAfter(false, end, x: 100, y: 200);
            StopAnimAfter("DickSitDownRight", end, 1);
            StopAnimAfter("AnneSitDownLeft", end, 1);
            StopAnimAfter("JulianSitDownLeft", end, 1);
            StopAnimAfter("GeorgeSpeakWithBook", end, 1);
            Obj("LOC05Anne", false);
            Obj("LOC05Dick", false);
            Obj("LOC05Julian", false);
            Obj("LOC05George", false);
            ChangeLocation("loc09", end);
            Episode = 2;
            SetVar(Vars.QuentinWontTalk, 0);
        }
        else if (Episode < 4 && Quest == 7)
            StateAfter(Hero, StandDownLeft, end);
        else if (At(7, 4) || At(7, 5))
        {
            if (who is Who.Julian or Who.Dick)
                GoOnEating(child!, end);
            else if (who == Who.George)
                StateAfter(Hero, StandDownRight, end);
            if (who == Who.Dick && At(7, 4) && topic == "change")
            {
                // OpenFive never sets the rucksack pointer position, so this walks the pointer to 0, 0.
                PointerAfter(false, end, x: Var(Vars.RucksackPointerX), y: Var(Vars.RucksackPointerY));
                ChangeLocation("loc04", end, 4);
            }
            SetText(StoryTexts.KitchenTopic, "");
        }
        else if (At(12, 1))
        {
            if (topic != "Hund" && who != Who.George)
                return;
            SwapAnim("GeorgeSitSpeakDownRight", "GeorgeSitDownRight", end);
            Sound(@"wav\timmy\timmy01.wav", end);
            Sound(@"wav\timmy\timmy01.wav", end, 2);
            if (topic == "Hund")
                SetText(StoryTexts.KitchenTopic, "");
            else
                ChangeLocation("locextro", end, 6);
        }
    }

    /// <summary>
    /// Quest 5 step 0, after Uncle Quentin and George's talk: a talk with one of the children ends
    /// breakfast (on to the hall); otherwise Uncle Quentin walks out and shuts the door.
    /// </summary>
    void AfterQuentinAndGeorge(TaskHandle end, int who, string topic, string? child)
    {
        if (topic is "Dick" or "Julian" or "Anne")
        {
            if (child != null)
                GoOnEating(child, end);
            else if (who == Who.George)
                SwapAnim("GeorgeSitSpeakDownRight", "GeorgeSitDownRight", end);
            ChangeLocation("loc04", end, 4);
            SetStep(5, 1);
            SetText(StoryTexts.KitchenTopic, "");
        }
        else if (who == Who.Anne)
        {
            StopAnimAfter("AnneSitSpeakDownLeft", end);
            Anim("AnneEat", -1, end);
            Anim("AnneEatArm", -1, end);
            ChangeLocation("loc04", end, 4);
            SetStep(5, 1);
        }
        else
        {
            SetVar(Vars.OriginalPlaceFlag, 1);
            PointerAfter(false, end, x: 100, y: 200);
            WalkAfter(Quentin, 647, 426, end);
            StateAfter(Quentin, StandUpRight, end);
            HideAfter(Quentin, end, 16);
            ObjAfter("LOC05OpenDoorObj", false, end, 26);
            Anim("DoCloseDoor", after: end, delay: 26);
            Sound(@"wav\fx\hotuezu.wav", end, 29);
            PointerAfter(true, end, 28, 100, 200);
            Obj("LOC05ExitToLOC04", true);
        }
    }

    /// <summary>Quest 3, the end of a talk at breakfast: talk stages move on, and George may act on the topic.</summary>
    void AfterBreakfastTalk(TaskHandle end, int who, string topic, string? child)
    {
        if (who == Who.Dick)
        {
            if (Episode == 3 && topic == "Quentin")
            {
                SetText(StoryTexts.KitchenTopic, "");
                SetVar(Vars.QuentinKitchenTalk, 2);
                SetVar(Vars.DickTalk, 2);
                SetVar(Vars.FannyTalk, 7);
                SetVar(Vars.AnneTalk, 2);
                SetVar(Vars.JulianTalk, 2);
            }
            else if (Episode == 3 && Var(Vars.DickTalk) is 0 or 2 or 4)
                SetVar(Vars.DickTalk, Var(Vars.DickTalk) + 1);
            GoOnEating("Dick", end);
        }
        else if (who == Who.Quentin)
        {
            QuentinDrinksAgain(end);
            if (Episode == 4)
            {
                if (Var(Vars.StudyDoorState) == 5)
                    SetVar(Vars.QuentinKitchenTalk, 1);
            }
            else if (Episode == 3)
            {
                if (topic == "Teekanne")
                    GeorgePoursTheTea(end);
                else if (topic == "Sandwich")
                    GeorgeSlipsOut(end);
                else if (topic == "Timmy")
                    SetText(StoryTexts.KitchenTopic, "");
                else if (Var(Vars.QuentinKitchenTalk) is 2 or 4)
                    SetVar(Vars.QuentinKitchenTalk, Var(Vars.QuentinKitchenTalk) + 1);
            }
        }
        else if (who == Who.Fanny)
        {
            SwapAnim("FannyStandRightTalk", "FannyStandRight", end);
            if (Episode == 3 && Var(Vars.FannyTalk) == 5)
                SetVar(Vars.FannyTalk, 6);
        }
        else if (who == Who.Anne)
        {
            GoOnEating("Anne", end);
            if (Episode == 3 && Var(Vars.AnneTalk) == 2)
                SetVar(Vars.AnneTalk, 3);
        }
        else if (who == Who.George)
        {
            StateAfter(Hero, StandDownLeft, end);
        }
        else if (who == Who.Julian)
        {
            GoOnEating("Julian", end);
            if (topic == "DLG")
            {
                // Uncle Quentin's question: George answers from the menu (see AfterChoice).
                Chain = null;
                SetVar(Vars.OriginalMenuEntry, 0x64e1d4);
                SetVar(Vars.OriginalMenuEntryShown, 1);
                Ask(Who.George, "KUECHE0006-05", 3, after: end);
                SetText(StoryTexts.KitchenTopic, "");
            }
            if (Episode == 3 && Var(Vars.JulianTalk) is 0 or 2)
                SetVar(Vars.JulianTalk, Var(Vars.JulianTalk) + 1);
        }
    }

    /// <summary>
    /// George gets up, takes the teapot from the stove, fills Uncle Quentin's cup, puts the pot
    /// back and sits down again; Uncle Quentin then drinks it all, which is the moment Timmy can
    /// be fed under the table.
    /// </summary>
    void GeorgePoursTheTea(TaskHandle end)
    {
        PointerAfter(false, end, x: 400, y: 300);
        SetVar(Vars.GrumbleTimer, 600);
        GeorgeGetsUp(end, 1, "GeorgeSitDownRight", 60, 456);
        TaskHandle? step = State(Hero, TakeMidUpRight);
        step = ObjAfter("LOC05Kanne", false, step, 4);
        WalkAfter(Hero, 608, 456, step);
        step = State(Hero, StandDownLeft);
        AnimPos("Tee", 502, 243);
        Depth(@"bmp\obj\george\gept\geptan.brg", 1900);
        Anim("Tee", after: step, delay: 7);
        WalkAfter(Hero, 50, 456, end, 12);
        step = State(Hero, TakeMidUpRight);
        step = ObjAfter("LOC05Kanne", true, step, 4);
        WalkAfter(Hero, 200, 557, end, 6);
        step = State(Hero, StandDownRight);
        HideAfter(Hero, step);
        step = Anim("GeorgeSitDown", after: step);
        Anim("GeorgeSitDownRight", -1, step);
        StopAnimAfter("QuentinDrink", step);
        PointerAfter(true, step, x: 400, y: 300);
        step = Anim("QuentinDrinkAll", after: step);
        // Quentin is busy with his tea until 10 ticks after he has finished it.
        SetVar(Vars.Distracted, 1);
        SetAfter(Vars.Distracted, 0, step, 10);
        Anim("QuentinDrink", -1, step);
        if (Var(Vars.QuentinTeapotTalk) == 0)
        {
            SetVar(Vars.QuentinTeapotTalk, 1);
            SetVar(Vars.FannyTalk, 8);
            SetVar(Vars.DickTalk, 4);
            SetVar(Vars.JulianTalk, 4);
            SetVar(Vars.AnneTalk, 4);
            SetVar(Vars.QuentinKitchenTalk, 4);
        }
        else if (Var(Vars.QuentinTeapotTalk) == 1)
        {
            SetVar(Vars.QuentinTeapotTalk, 2);
        }
        SetText(StoryTexts.KitchenTopic, "");
    }

    /// <summary>George gets up with the sandwich and slips out into the hall (on to step 4).</summary>
    void GeorgeSlipsOut(TaskHandle end)
    {
        SetText(StoryTexts.KitchenTopic, "");
        PointerAfter(false, end, x: 400, y: 300);
        Episode = 4;
        GeorgeGetsUp(end, 4, "GeorgeSitDownRight", 570, 435);
        TaskHandle? door = State(Hero, TakeMidRight);
        door = Anim("DoOpenDoor", after: door, delay: 5, endObject: "LOC05OpenDoorObj", endObjectOn: true);
        ChangeLocation("loc04", door);
    }

    // ---- objects, the menu and the clock -------------------------------------------------------

    /// <summary>
    /// An object was switched off: the olive oil (unless it is only being placed), the toast and
    /// the teapot (quest 7) go into the rucksack when the hero has taken them.
    /// </summary>
    /// <remarks>The original's FUN_00435e9e.</remarks>
    public override void ObjectChanged(GameWorld world, string name, bool on)
    {
        if (on)
            return;
        if (Same(name, "LOC05Flasche") && Var(Vars.PlacingOliveOil) == 0)
        {
            SetVar(Vars.PlacingOliveOil, 1);
            Give(Item.OliveOil);
            SetVar(Vars.OliveOilTaken, 1);
        }
        else if (Same(name, "LOC05Toast"))
        {
            if (Var(Vars.TakingToast) == 0)
                return;
            Give(Item.Plate);
            SetVar(Vars.KitchenSandwich, 2);
            SetVar(Vars.TakingToast, 0);
            if (At(7, 4))
            {
                Chain = null;
                Say(Who.George, "KUECHE0031", 5, last: true);
            }
        }
        else if (Same(name, "LOC05Kanne") && Quest == 7 && Var(Vars.TakingTeapot) != 0)
        {
            Give(Item.Teapot);
            SetVar(Vars.KitchenTeapot, 2);
            SetVar(Vars.TakingTeapot, 0);
            if (Episode == 4)
            {
                Chain = null;
                Say(Who.George, "KUECHE0031", 6, last: true);
            }
        }
    }

    /// <summary>
    /// The answer to the breakfast menu: in quest 5 step 4 the children's plan for the rainy day,
    /// otherwise George's answer to Uncle Quentin's question. The menu is then put back.
    /// </summary>
    /// <remarks>The original's FUN_0043c9ee.</remarks>
    protected override void AfterChoice(string scene, int line, int option)
    {
        if (Var(ChoicePending) == 0)
            return;
        if (At(5, 4))
        {
            switch (option)
            {
                case 1:
                    SetVar(Vars.KitchenMenuAnswer, 1);
                    PointerAfter(true, null, 30, 300, 400);
                    break;
                case 2:
                    Say(Who.Dick, "KUECHE0014-05", 9);
                    Say(Who.Anne, "KUECHE0014-05", 10);
                    Say(Who.Julian, "KUECHE0014-05", 11);
                    Say(Who.George, "KUECHE0014-05", 15, last: true);
                    SetVar(Vars.KitchenMenuAnswer, 2);
                    break;
                case 3:
                    SetVar(Vars.KitchenMenuAnswer, -1);
                    PointerAfter(true, null, 30, 300, 400);
                    break;
            }
            SetVar(Vars.AnneTalk, 1);
            SetVar(Vars.DickTalk, 1);
            SetVar(Vars.JulianTalk, 1);
        }
        else
        {
            switch (option)
            {
                case 1:
                    Say(Who.Julian, "KUECHE0006-05", 4, last: true);
                    SetVar(Vars.KitchenMenuAnswer, 1);
                    break;
                case 2:
                    Say(Who.Quentin, "KUECHE0006-05", 5, last: true);
                    SetVar(Vars.KitchenMenuAnswer, 2);
                    break;
                case 3:
                    SetVar(Vars.KitchenMenuAnswer, -1);
                    PointerOn();
                    PointerAfter(true, null, x: 200, y: 300);
                    break;
            }
            SetVar(Vars.AnneTalk, 0);
            SetVar(Vars.DickTalk, 0);
            SetVar(Vars.QuentinKitchenTalk, 0);
            SetVar(Vars.JulianTalk, 0);
            SetVar(Vars.FannyTalk, 0);
        }
        // The kitchen never takes options off its menu, so every option is always on offer.
        ReopenMenu();
        SetVar(ChoicePending, 0);
    }

    /// <summary>
    /// Counts down the grumble timer (clicks wind it up). When it runs out at breakfast in quest 3
    /// step 3, Timmy begs at the table and Uncle Quentin grumbles.
    /// </summary>
    /// <remarks>The original's FUN_0043d12b.</remarks>
    public override void EveryTick()
    {
        int timer = Var(Vars.GrumbleTimer);
        if (timer != 0)
        {
            SetVar(Vars.GrumbleTimer, timer - 1);
            return;
        }
        SetVar(Vars.GrumbleTimer, 300);
        if (!At(3, 3))
            return;
        StateAfter(Timmy, EatRight, Chain);
        SetText(StoryTexts.KitchenTopic, "Timmy");
        Sound(@"wav\timmy\betteln.wav");
        int begged = Var(Vars.TimmyBegsTalk);
        if (begged is 0 or 1)
            SetVar(Vars.TimmyBegsTalk, begged + 1);
        Say(Who.Quentin, "KUECHE0005-05", begged switch { 0 => 10, 1 => 11, _ => 12 }, last: true, delay: 6);
    }
}
