using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The foot of the dungeon stairs below the castle courtyard (loc21). On quest 9 Dick and George
/// come down here together for the first time.
/// </summary>
public sealed class DungeonStairs : StoryScript
{
    /// <summary>The exits: where the hero walks to, which way they turn, and where it leads.</summary>
    static readonly Dictionary<string, (int X, int Y, int Facing, string Target)> Exits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LOC21ExitToVerlies2"] = (701, 597, StandDownRight, "loc29"),
        ["LOC21ExitToLoc23"] = (22, 401, StandUpLeft, "loc23"),
        ["LOC21ExitToLoc17b"] = (554, 189, StandUpLeft, "loc17b"),
    };

    /// <inheritdoc/>
    public override string Location => "loc21";

    /// <summary>The hero comes in from where they came from; on quest 9 George is here and they talk.</summary>
    /// <remarks>The original's FUN_00455b6b and FUN_00455c0b.</remarks>
    protected override void Setup()
    {
        ClearQueue(George);
        Hero.Follower = null;
        TaskHandle? arrived = null;
        switch (FromId)
        {
            case 23:
                Place(Hero, 22, 401, StandUpLeft);
                Show(Hero);
                Walk(Hero, 100, 462);
                break;
            case 29:
                Place(Hero, 701, 597, StandUpLeft);
                Show(Hero);
                Walk(Hero, 500, 540);
                break;
            case 117: // down the stairs from the courtyard at night
                Place(Hero, 554, 189, StandDownLeft);
                Show(Hero);
                Walk(Hero, 300, 500);
                arrived = Chain = State(Hero, StandDownLeft);
                break;
            default:
                Place(Hero, 300, 500, StandDownRight);
                Show(Hero);
                break;
        }
        if (At(9, 1))
        {
            Place(George, 40, 579, StandDownRight);
            if (Var(Vars.StairsSceneSeen) == 0)
            {
                PointerOff();
                SetVar(Vars.StairsSceneSeen, 1);
                // As in the original, both lines wait for the hero to arrive (when coming down the stairs).
                Say(Who.Dick, "LABYRINTH0044", 1, after: arrived);
                Say(Who.George, "LABYRINTH0044", 2, last: true, after: arrived);
            }
        }
        ShowParty();
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>Talking to George, or one of the three ways out.</summary>
    public override bool Click(string name)
    {
        if (World.Actors.FirstOrDefault(a => a.Name == name) is { } actor)
            ClickActor(actor);
        else if (Exits.TryGetValue(name, out var exit))
            ChangeLocation(exit.Target, WalkAndFace(exit.X, exit.Y, exit.Facing));
        return true;
    }

    /// <summary>George has two things to say on quest 9.</summary>
    /// <remarks>The original's FUN_00455a1b (FUN_004558ea handles the exits).</remarks>
    void ClickActor(Actor actor)
    {
        if (actor != George)
            return;
        Walk(Hero, 300, 500);
        Chain = State(Hero, StandDownRight);
        if (!At(9, 1))
            return;
        if (Var(Vars.GeorgeStairsTalk) == 0)
        {
            IncAfter(Vars.GeorgeStairsTalk, Chain);
            Say(Who.George, "LABYRINTH0044", 3, last: true);
        }
        else if (Var(Vars.GeorgeStairsTalk) == 1)
            Say(Who.George, "LABYRINTH0044", 4, last: true);
    }

    /// <inheritdoc/>
    protected override TalkPoses? PosesOf(int speaker) => DungeonPoses(speaker);

    /// <summary>The speaker takes their talking pose.</summary>
    /// <remarks>The original's FUN_00455e34.</remarks>
    public override void LineStarting(GameWorld world, int speaker) => StartTalking();

    /// <summary>The speaker stands again after their line.</summary>
    /// <remarks>The original's FUN_00455ef6.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end) => StopTalking(end);

    /// <summary>On quest 9, the last speaker stands again too.</summary>
    /// <remarks>The original's FUN_00455fb8.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (!At(9, 1))
            return;
        StopTalking(end);
        SetText("t63beac", "");
    }
}
