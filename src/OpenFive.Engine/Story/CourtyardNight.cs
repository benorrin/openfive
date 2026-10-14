using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The castle courtyard at night (loc17b). On quest 8, step 5 the children push the big stone
/// aside together, uncovering the dungeon stairs (loc21). On quest 11 the stairs fall in.
/// </summary>
public sealed class CourtyardNight : StoryScript
{
    /// <summary>The conversation running: "start" (arriving at night) or "Jul" (Julian helps push).</summary>
    const string Scene = "t63be78";

    /// <inheritdoc/>
    public override string Location => "loc17b";

    /// <summary>The characters and the scene of the story step, then the big stone.</summary>
    /// <remarks>The original's FUN_0045323f.</remarks>
    protected override void Setup()
    {
        SetVar(Vars.BigStoneX, 84);
        SetVar(Vars.BigStoneY, 427);
        PlaceCharacters();
        SetUpStone();
        SetVar(Vars.BigStoneMoving, 0);
    }

    /// <summary>The stone over the stairs, or pushed aside (the stairs open except on quest 11).</summary>
    /// <remarks>The original's FUN_00453299.</remarks>
    void SetUpStone()
    {
        Obj("LOC17bbigStone", true);
        if (Var(Vars.BigStonePushed) == 0)
        {
            MoveObj("LOC17bbigStone", 84, 427);
            Obj("LOC17bExitToLOC21", false);
        }
        else
        {
            MoveObj("LOC17bbigStone", -56, 497);
            Obj("LOC17bExitToLOC21", Quest != 11);
        }
    }

    /// <summary>Where everyone is, for the story step; the arrival, quest 9 and quest 11 scenes.</summary>
    /// <remarks>The original's FUN_00453366.</remarks>
    void PlaceCharacters()
    {
        AreaEnable(0, false);
        if (At(8, 5))
        {
            if (Var(Vars.NightSceneSeen) == 0)
            {
                // Anne comes over; she and Julian talk. The others follow after it (ConversationEnding).
                Place(Anne, 820, 500, StandDownLeft);
                ShowParty();
                Walk(Anne, 165, 566);
                Chain = State(Anne, StandDownRight);
                PointerOff();
                SetVar(Vars.NightSceneSeen, 1);
                SetText(Scene, "start");
                Say(Who.Anne, "RUINE0043", 20);
                Say(Who.Julian, "RUINE0043", 21);
                Say(Who.Anne, "RUINE0043", 22, last: true);
            }
            else if (Var(Vars.BigStonePushed) == 0)
            {
                Place(Anne, 165, 566, StandDownLeft);
                PlaceAfter(Hero, 492, 442, StandDownLeft, null);
                PlaceAfter(George, 750, 510, StandDownLeft, null, 8);
                PlaceAfter(Julian, 720, 570, StandDownLeft, null, 8);
                Show(Hero);
                ShowParty();
            }
            else
                PlaceEveryone();
        }
        else if (Quest == 11)
        {
            // The hero comes up the stairs, and stones fall in behind them.
            AreaEnable(0, true);
            Place(Hero, 330, 505, StandDownRight);
            Show(Hero);
            ShowParty();
            Walk(Hero, 643, 549);
            PointerOff();
            var arrived = Chain = State(Hero, StandDownLeft);
            Say(Who.Dick, "RUINE0053", 16, last: true, delay: 10);
            var falling = Anim("Loc17bFallendeSteine", after: arrived, delay: 8);
            Anim("Loc17bbegrabendeSteine", -1, falling);
        }
        else if (Quest > 8)
        {
            PlaceEveryone();
            if (At(9, 1))
            {
                Chain = null;
                Say(Who.Julian, "LABYRINTH0044", 11);
                Say(Who.Dick, "LABYRINTH0044", 13);
                Say(Who.Anne, "LABYRINTH0044", 14);
                Say(Who.George, "LABYRINTH0044", 15, last: true);
            }
        }
        Obj("LOC17bStein", true);
        Obj("LOC17bExitToLOC21", false);
        if (Quest > 8)
        {
            Obj("LOC17bStein", false);
            Obj("LOC17bExitToLOC21", true);
        }
    }

    /// <summary>Everyone at their places around the stone (Dick and George swap when George is the hero).</summary>
    void PlaceEveryone()
    {
        Place(Anne, 224, 542, StandDownRight);
        Place(Hero, 492, 442, StandDownLeft);
        Place(Julian, 720, 570, StandDownLeft);
        Place(Hero == Dick ? George : Dick, 750, 510, StandDownLeft);
        Show(Hero);
        ShowParty();
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>Talking to the others, the stone, or the ways out.</summary>
    public override bool Click(string name)
    {
        if (World.Actors.FirstOrDefault(a => a.Name == name) is { } actor)
            ClickActor(actor);
        else
            ClickObject(name);
        return true;
    }

    /// <summary>
    /// Back to the courtyard (not while the stone waits, nor on quest 9), down the stairs (quest 9
    /// begins), and the stone, which the hero cannot move alone.
    /// </summary>
    /// <remarks>The original's FUN_004523f4.</remarks>
    void ClickObject(string name)
    {
        if (Same(name, "LOC17bExitToLOC17"))
        {
            Walk(Hero, 780, 500);
            Chain = State(Hero, StandLeft);
            if (At(8, 5))
            {
                PointerAfter(false, Chain, x: 400, y: 400);
                Say(Who.George, "RUINE0043", 5, last: true);
            }
            else if (!At(9, 1))
                ChangeLocation("loc17", Chain);
        }
        else if (Same(name, "LOC17bExitToLOC21"))
        {
            Walk(Hero, 339, 530);
            var reached = Chain = State(Hero, StandLeft);
            int delay = 0;
            if (At(8, 5))
            {
                delay = 30;
                PointerAfter(false, reached, x: 400, y: 400);
                Say(Who.Dick, "RUINE0043", 35, last: true);
                SetAfter(Vars.EpisodeNumber, 1, reached);
                SetAfter(Vars.QuestNumber, 9, reached);
            }
            ChangeLocation("loc21", delay: delay);
        }
        else if (Same(name, "LOC17bStein"))
        {
            if (!At(8, 5))
            {
                Walk(Hero, 339, 507);
                Chain = State(Hero, StandDownLeft);
            }
            else if (Var(Vars.StoneTriedAlone) == 0)
            {
                Walk(Hero, 339, 570);
                var pushing = Chain = State(Hero, PushBoatLeft);
                PointerAfter(false, pushing, x: 400, y: 400);
                IncAfter(Vars.StoneTriedAlone, pushing);
                IncAfter(Vars.AnneNightTalk, pushing);
                IncAfter(Vars.GeorgeNightTalk, pushing);
                IncAfter(Vars.JulianNightTalk, pushing);
                Say(Who.Dick, "RUINE0043", 27, last: true, delay: 10);
            }
        }
    }

    /// <summary>What Anne, George and Julian say on quests 8 and 9; Julian's second remark leads to pushing the stone.</summary>
    /// <remarks>The original's FUN_00452784.</remarks>
    void ClickActor(Actor actor)
    {
        if (actor == Anne)
            TalkToAnne();
        else if (actor == George)
            TalkToGeorge();
        else if (actor == Julian)
            TalkToJulian();
    }

    void TalkToAnne()
    {
        if (At(8, 5))
        {
            switch (Var(Vars.AnneNightTalk))
            {
                case 0:
                    TurnTo(339, 507, StandDownLeft);
                    Say(Who.Anne, "RUINE0043", 26, last: true);
                    break;
                case 1:
                    TurnTo(339, 507, StandDownLeft);
                    if (Var(Vars.JulianNightTalk) < 2)
                        Say(Who.Anne, "RUINE0043", 29, last: true);
                    break;
                case 2:
                    TurnTo(604, 487, StandDownLeft);
                    IncAfter(Vars.AnneNightTalk, Chain);
                    Say(Who.Anne, "RUINE0043", 36);
                    Say(Who.Dick, "RUINE0043", 37);
                    Say(Who.Anne, "RUINE0043", 38, last: true);
                    break;
                case 3:
                    TurnTo(604, 487, StandDownLeft);
                    Say(Who.Anne, "RUINE0043", 42, last: true);
                    break;
            }
        }
        else if (At(9, 1))
        {
            TurnTo(339, 507, StandDownLeft);
            Say(Who.Anne, "LABYRINTH0044", 16, last: true);
        }
    }

    void TalkToGeorge()
    {
        TurnTo(604, 487, StandDownRight);
        if (At(8, 5))
        {
            switch (Var(Vars.GeorgeNightTalk))
            {
                case 0:
                    Say(Who.George, "RUINE0043", 25, last: true);
                    break;
                case 1:
                    Say(Who.George, "RUINE0043", Var(Vars.JulianNightTalk) == 2 ? 31 : 30, last: true);
                    break;
                case 2:
                    IncAfter(Vars.GeorgeNightTalk, Chain);
                    Say(Who.George, "RUINE0043", 43, last: true);
                    break;
                case 3:
                    Say(Who.George, "RUINE0043", 44, last: true);
                    break;
            }
        }
        else if (At(9, 1))
        {
            TurnTo(604, 487, StandDownRight);
            Say(Who.George, "LABYRINTH0044", 17, last: true);
        }
    }

    void TalkToJulian()
    {
        if (At(8, 5))
        {
            switch (Var(Vars.JulianNightTalk))
            {
                case 0:
                    TurnTo(604, 487, StandDownRight);
                    Say(Who.Julian, "RUINE0043", 25, last: true);
                    break;
                case 1:
                    // Julian will help push: after this line he and the hero push together.
                    TurnTo(339, 590, StandDownRight);
                    IncAfter(Vars.JulianNightTalk, Chain);
                    Say(Who.Julian, "RUINE0043", 28, last: true);
                    SetText(Scene, "Jul");
                    break;
                case 2:
                    Chain = null;
                    IncAfter(Vars.JulianNightTalk, null);
                    Say(Who.Julian, "RUINE0043", 39);
                    Say(Who.George, "RUINE0043", 40);
                    Say(Who.Anne, "RUINE0043", 41, last: true);
                    break;
                case 3:
                    Chain = null;
                    Say(Who.Julian, "RUINE0043", 42, last: true);
                    break;
            }
        }
        else if (At(9, 1))
        {
            TurnTo(604, 487, StandDownRight);
            Say(Who.Julian, "LABYRINTH0044", 16, last: true);
        }
    }

    /// <summary>The hero walks to (x, y) and turns; the next line waits for that.</summary>
    void TurnTo(int x, int y, int facing)
    {
        Walk(Hero, x, y);
        Chain = State(Hero, facing);
    }

    /// <summary>Once the stone has moved (quest 8, Anne's talk past 1) the speakers face their new places.</summary>
    bool StoneMoved => At(8, 5) && Var(Vars.AnneNightTalk) > 1;

    /// <inheritdoc/>
    protected override TalkPoses? PosesOf(int speaker)
    {
        if (StoneMoved)
        {
            return speaker switch
            {
                Who.Anne => new TalkPoses(Anne, SpeakDownLeft, StandDownLeft),
                Who.Julian => new TalkPoses(Julian, StandUpRight, StandUpRight),
                Who.George => new TalkPoses(George, SpeakDownLeft, StandDownLeft),
                Who.Dick => new TalkPoses(Hero, SpeakDownRight, StandDownRight),
                _ => null,
            };
        }
        return speaker switch
        {
            Who.Anne => new TalkPoses(Anne, SpeakDownRight, StandDownRight),
            Who.Julian => new TalkPoses(Julian, SpeakDownLeft, StandDownLeft),
            Who.George => new TalkPoses(George, SpeakDownLeft, StandDownLeft),
            Who.Dick => new TalkPoses(Hero, SpeakDownLeft, StandDownLeft),
            _ => null,
        };
    }

    /// <summary>The speaker takes their talking pose.</summary>
    /// <remarks>The original's FUN_00453996.</remarks>
    public override void LineStarting(GameWorld world, int speaker) => StartTalking();

    /// <summary>The speaker stands again after their line.</summary>
    /// <remarks>The original's FUN_00453b1c.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end) => StopTalking(end);

    /// <summary>
    /// The last speaker stands again. On quest 8 the arrival talk brings the others in, and
    /// Julian's offer starts the push; on quest 11 the hero goes back out to the courtyard.
    /// </summary>
    /// <remarks>The original's FUN_00453ca2.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (At(8, 5))
        {
            StopTalking(end);
            if (StoneMoved)
                return;
            switch (Text(Scene))
            {
                case "start":
                    TheOthersArrive(end);
                    break;
                case "Jul":
                    JulianHelps(end);
                    break;
            }
            SetText(Scene, "");
        }
        else if (At(9, 1))
        {
            if (CurrentSpeaker != Who.Dick)
                StopTalking(end);
        }
        else if (Quest == 11)
        {
            Obj("LOC17bExitToLOC21", false);
            var standing = StateAfter(Hero, StandDownLeft, end);
            Sound(@"wav\fx\mauerein.wav", standing, 3);
            PointerAfter(false, end, x: 400, y: 400);
            ChangeLocation("loc17", end, 2);
        }
    }

    /// <summary>The hero, George and Julian come into the courtyard; Dick and Anne talk.</summary>
    void TheOthersArrive(TaskHandle end)
    {
        PlaceAfter(Hero, 810, 500, StandLeft, end);
        PlaceAfter(George, 790, 510, StandLeft, end, 8);
        PlaceAfter(Julian, 790, 570, StandLeft, end, 8);
        Show(Hero);
        Show(George);
        Show(Julian);
        WalkAfter(Hero, 492, 442, end, 4);
        WalkAfter(George, 750, 510, end, 10);
        WalkAfter(Julian, 720, 570, end, 10);
        Chain = end;
        PointerAfter(false, end, 1, 400, 400);
        Say(Who.Dick, "RUINE0043", 23, delay: 30);
        Say(Who.Anne, "RUINE0043", 24, last: true);
    }

    /// <summary>Julian takes the other side of the stone and starts pushing (see <see cref="EveryTick"/>); Anne steps aside.</summary>
    void JulianHelps(TaskHandle end)
    {
        WalkAfter(Julian, 294, 600, end);
        WalkAfter(Anne, 740, 700, end);
        PointerAfter(false, end, x: 400, y: 400);
        State(Julian, PushBoatLeft);
        Chain = State(Anne, StandLeft);
        ObjAfter("LOC17bStein", false, end);
        ObjAfter("LOC17bExitToLOC21", true, end);
        Say(Who.George, "RUINE0043", 32);
        Say(Who.Julian, "RUINE0043", 33);
        Say(Who.Anne, "RUINE0043", 34, last: true);
    }

    /// <summary>
    /// Pushing the stone (quest 8, step 5): once Julian pushes, the hero joins in; while both
    /// push, they and the stone move a little every tick; when they stop, Julian steps back.
    /// </summary>
    /// <remarks>The original's FUN_004542b6.</remarks>
    public override void EveryTick()
    {
        if (!At(8, 5))
            return;
        if (Julian.State == PushBoatLeft && Hero.State == PushBoatLeft)
        {
            SetVar(Vars.BigStonePushed, 1);
            Hero.X -= 10;
            Julian.X -= 10;
            Hero.Y += 5;
            Julian.Y += 5;
            SetVar(Vars.BigStoneX, Var(Vars.BigStoneX) - 10);
            SetVar(Vars.BigStoneY, Var(Vars.BigStoneY) + 5);
            MoveObj("LOC17bbigStone", Var(Vars.BigStoneX), Var(Vars.BigStoneY));
            SetVar(Vars.BigStoneMoving, 1);
        }
        else if (Var(Vars.BigStoneMoving) == 0)
        {
            if (Julian.State == PushBoatLeft)
            {
                State(Hero, PushBoatLeft);
                Sound(@"wav\fx\steinbod.wav", delay: 3);
                SetAfter(Vars.AnneNightTalk, 2, null);
                SetAfter(Vars.GeorgeNightTalk, 2, null);
                SetAfter(Vars.JulianNightTalk, 2, null);
            }
        }
        else
        {
            SetVar(Vars.BigStoneMoving, 0);
            Walk(Julian, 600, 550);
        }
    }
}
