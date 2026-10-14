using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The dungeon cell (loc28). On quest 9 Dick and George pass through it; on quest 10 the
/// professor has locked the Five in, and Timmy gets out through a crack to fetch help.
/// </summary>
public sealed class Dungeon28 : StoryScript
{
    /// <summary>"Tuer" while the conversation at the cell door runs (they leave after it).</summary>
    const string LeavingCell = "t63bef0";

    /// <inheritdoc/>
    public override string Location => "loc28";

    /// <summary>The door is shown open, or (quest 10, step 2) shut with everyone locked in.</summary>
    /// <remarks>The original's FUN_0045aa2f and FUN_0045aabb.</remarks>
    protected override void Setup()
    {
        Hero.Follower = null;
        StopAnim("CloseDoor");
        StopAnim("OpenDoor");
        StopAnim("Tuere");
        Depth(@"bmp\obj\locs\loc28\tuer.brg", 8000);
        PathEnable(0, true);
        if (At(10, 2))
        {
            LockedIn();
            return;
        }
        Place(Hero, 722, 375, StandDownLeft);
        if (At(9, 1))
        {
            Place(George, 722, 375, StandUpLeft);
            // As in the original, the hero is made their own follower, so nobody follows.
            Hero.Follower = Hero;
        }
        Show(Hero);
        ShowParty();
        Walk(Hero, 391, 458);
        Anim("OpenDoor", -1);
    }

    /// <summary>Everyone in the cell, the door shut, and the way to it blocked.</summary>
    void LockedIn()
    {
        Place(Hero, 391, 458, StandDownRight);
        Place(Timmy, 520, 388, StandLeft);
        Place(Anne, 457, 553, StandUpLeft);
        Place(Julian, 271, 488, StandDownRight);
        Place(George, 538, 530, StandUpLeft);
        Show(Hero);
        ShowParty();
        PathEnable(0, false);
        Anim("CloseDoor", -1);
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>Talking to the others, the door, or the crack Timmy can squeeze through.</summary>
    public override bool Click(string name)
    {
        if (World.Actors.FirstOrDefault(a => a.Name == name) is { } actor)
            ClickActor(actor);
        else
            ClickObject(name);
        return true;
    }

    /// <summary>The cell door and the crack.</summary>
    /// <remarks>The original's FUN_00459e3e.</remarks>
    void ClickObject(string name)
    {
        if (Same(name, "LOC28ExitToLoc25"))
            TryTheDoor();
        else if (Same(name, "LOC28Spalte") && At(10, 2) && Var(Vars.CellCrackUsed) == 0)
            SendTimmyForHelp();
    }

    /// <summary>
    /// The hero pushes the door. Locked in (quest 10), it does not give until Timmy has gone for
    /// help; then Anne and Dick talk and they leave. Otherwise it opens and the hero goes out.
    /// </summary>
    void TryTheDoor()
    {
        Walk(Hero, 717, 381);
        var reached = Chain = State(Hero, TakeMidRight);
        Sound(@"wav\fx\tuerruet.wav", reached);
        if (At(10, 2))
        {
            if (Var(Vars.CellCrackUsed) == 0)
            {
                Say(Who.Dick, "VERLIES0048", 2, last: true);
                return;
            }
            Say(Who.Anne, "VERLIES0048", 20);
            Say(Who.Dick, "VERLIES0048", 21, last: true);
            SetText(LeavingCell, "Tuer");
        }
        var opened = reached;
        if (IsAnimShown("CloseDoor"))
        {
            StopAnimAfter("CloseDoor", reached);
            opened = Anim("Tuere", after: reached, delay: 4);
            Anim("OpenDoor", -1, opened);
        }
        if (Text(LeavingCell) != "Tuer")
            ChangeLocation("loc25", opened);
    }

    /// <summary>Timmy sniffs at the crack and squeezes through; the others talk about it.</summary>
    void SendTimmyForHelp()
    {
        Walk(Timmy, 552, 399);
        var sniffing = Chain = State(Timmy, SniffRight);
        PointerAfter(false, sniffing, x: 400, y: 400);
        IncAfter(Vars.CellCrackUsed, sniffing);
        SetAfter(Vars.GeorgeDungeonTalk, 3, sniffing);
        SetAfter(Vars.JulianCellTalk, 3, sniffing);
        SetAfter(Vars.AnneCellTalk, 3, sniffing);
        Say(Who.Dick, "VERLIES0048", 14, delay: 6);
        Say(Who.Anne, "VERLIES0048", 15);
        Say(Who.Julian, "VERLIES0048", 16);
        Say(Who.George, "VERLIES0048", 17);
        Say(Who.George, "VERLIES0048", 18);
        Say(Who.Julian, "VERLIES0048", 19, last: true);
    }

    /// <summary>George (quests 9 and 10), Anne and Julian (quest 10) each have a few things to say.</summary>
    /// <remarks>The original's FUN_0045a321.</remarks>
    void ClickActor(Actor actor)
    {
        if (actor == George)
        {
            var turned = Chain = State(Hero, StandDownRight);
            int talk = Var(Vars.GeorgeDungeonTalk);
            if (At(9, 1))
            {
                if (talk == 0)
                {
                    IncAfter(Vars.GeorgeDungeonTalk, turned);
                    Say(Who.George, "LABYRINTH0044", 8);
                    Say(Who.Dick, "LABYRINTH0044", 9, last: true);
                }
                else if (talk < 2)
                {
                    IncAfter(Vars.GeorgeDungeonTalk, turned);
                    Say(Who.George, "LABYRINTH0044", 10, last: true);
                }
                else if (talk == 2)
                    Say(Who.George, "LABYRINTH0044", 11, last: true);
            }
            else if (At(10, 2))
            {
                if (talk == 0)
                {
                    IncAfter(Vars.GeorgeDungeonTalk, turned);
                    // As in the original, both lines wait for the hero to turn.
                    Say(Who.George, "VERLIES0048", 10);
                    Say(Who.Dick, "VERLIES0048", 11, last: true, after: turned);
                }
                else if (talk < 2)
                {
                    IncAfter(Vars.GeorgeDungeonTalk, turned);
                    Say(Who.George, "VERLIES0048", 12, last: true);
                }
                else if (talk == 2)
                    Say(Who.George, "VERLIES0048", 13, last: true);
            }
        }
        else if (actor == Anne)
        {
            var turned = Chain = State(Hero, StandDownRight);
            if (!At(10, 2))
                return;
            if (Var(Vars.AnneCellTalk) == 0)
            {
                IncAfter(Vars.AnneCellTalk, turned);
                Say(Who.Anne, "VERLIES0048", 3);
                Say(Who.Dick, "VERLIES0048", 4, last: true);
            }
            else if (Var(Vars.AnneCellTalk) == 1)
                Say(Who.Anne, "VERLIES0048", 5, last: true);
        }
        else if (actor == Julian)
        {
            var turned = Chain = State(Hero, StandDownLeft);
            if (!At(10, 2))
                return;
            if (Var(Vars.JulianCellTalk) == 0)
            {
                IncAfter(Vars.JulianCellTalk, turned);
                Say(Who.Julian, "VERLIES0048", 6);
                Say(Who.Dick, "VERLIES0048", 7);
                // As in the original, this line waits for the hero to turn, not for Dick's line.
                Say(Who.Julian, "VERLIES0048", 8, last: true, after: turned);
            }
            else if (Var(Vars.JulianCellTalk) == 1)
                Say(Who.Julian, "VERLIES0048", 9, last: true);
        }
    }

    /// <summary>Locked in (quest 10, step 2) Julian and Dick face down-right; otherwise each speaker has a pose.</summary>
    protected override TalkPoses? PosesOf(int speaker)
    {
        if (At(10, 2))
        {
            return speaker switch
            {
                Who.Julian => new TalkPoses(Julian, SpeakDownRight, StandDownRight),
                Who.Dick => new TalkPoses(Hero, SpeakDownRight, StandDownRight),
                _ => null,
            };
        }
        return speaker switch
        {
            Who.Anne => new TalkPoses(Anne, SpeakDownRight, StandDownRight),
            Who.Julian => new TalkPoses(Julian, SpeakDownLeft, StandDownLeft),
            Who.George => new TalkPoses(George, SpeakDownLeft, StandDownLeft),
            Who.Dick => new TalkPoses(Hero, SpeakDownRight, StandDownRight),
            _ => null,
        };
    }

    /// <summary>The speaker takes their talking pose.</summary>
    /// <remarks>The original's FUN_0045ac96.</remarks>
    public override void LineStarting(GameWorld world, int speaker) => StartTalking();

    /// <summary>The speaker stands again after their line.</summary>
    /// <remarks>The original's FUN_0045adbf.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end) => StopTalking(end);

    /// <summary>The last speaker stands again; after the talk at the door, they leave the cell (quest 10, step 3).</summary>
    /// <remarks>The original's FUN_0045aee8.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (At(9, 1))
            StopTalking(end);
        else if (At(10, 2))
        {
            StopTalking(end);
            if (Text(LeavingCell) == "Tuer")
            {
                PointerAfter(false, end, x: 400, y: 400);
                ChangeLocation("loc25", end, 4);
                SetAfter(Vars.EpisodeNumber, 3, end);
            }
            SetText(LeavingCell, "");
        }
    }
}
