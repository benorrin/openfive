using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// A dungeon passage (loc25) joining the hole, the treasure door, the fork and the cells. On
/// quest 10, having escaped from the cell, Dick meets Anne and Timmy here.
/// </summary>
public sealed class Passage25 : StoryScript
{
    /// <summary>The exits Dick will not take while Anne is with him on quest 10: where he walks to and where they lead.</summary>
    static readonly Dictionary<string, (int X, int Y, string Target)> Exits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LOC25ExitToLoc26"] = (202, 601, "loc26"),
        ["LOC25ExitToLoc28"] = (420, 291, "loc28"),
        ["LOC25ExitToLoc24"] = (782, 256, "loc24"),
    };

    /// <summary>Where the hero comes in from each neighbouring location (they then walk to the middle).</summary>
    static readonly Dictionary<int, (int X, int Y)> Entrances = new()
    {
        [22] = (712, 406),
        [24] = (774, 296),
        [26] = (202, 501),
        [28] = (483, 337),
    };

    /// <inheritdoc/>
    public override string Location => "loc25";

    /// <summary>
    /// The hero comes in from where they came from. On quest 9 George is here; on quest 10 Anne
    /// and Timmy are, and they talk.
    /// </summary>
    /// <remarks>The original's FUN_004583af and FUN_00458445.</remarks>
    protected override void Setup()
    {
        Hero.Follower = null;
        TaskHandle? arrived = null;
        if (Entrances.TryGetValue(FromId, out var entrance))
        {
            Place(Hero, entrance.X, entrance.Y, StandDownLeft);
            Show(Hero);
            Walk(Hero, 617, 407);
            if (FromId == 28)
                arrived = State(Hero, StandDownLeft);
        }
        else
        {
            Place(Hero, 617, 407, StandDownLeft);
            Show(Hero);
        }
        if (At(9, 1))
        {
            Place(George, 360, 417, StandLeft);
            if (Var(Vars.PassageSceneSeen) == 0)
            {
                SetVar(Vars.PassageSceneSeen, 1);
                PointerOff();
                Chain = arrived;
                Say(Who.Dick, "LABYRINTH0044", 5, delay: 40);
                Say(Who.George, "LABYRINTH0044", 6);
                Say(Who.Dick, "LABYRINTH0044", 7, last: true);
            }
            // As in the original, the hero is made their own follower, so nobody follows.
            Hero.Follower = Hero;
        }
        else if (At(10, 3))
        {
            Place(Timmy, 483, 493, StandRight);
            Place(Anne, 360, 417, StandLeft);
            WalkAfter(Anne, 517, 367, arrived);
            Chain = State(Anne, StandDownRight);
            PointerOff();
            Say(Who.Dick, "LABYRINTH0049", 1, delay: 2);
            Say(Who.Anne, "LABYRINTH0049", 2);
            Say(Who.Dick, "LABYRINTH0049", 3, last: true);
        }
        ShowParty();
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>The ways out; on quest 10 Dick will only go back the way to the hole, with Anne.</summary>
    /// <remarks>The original's FUN_00457fc0.</remarks>
    public override bool Click(string name)
    {
        if (Exits.TryGetValue(name, out var exit))
        {
            if (At(10, 3))
                StayWithAnne();
            else
            {
                Walk(Hero, exit.X, exit.Y);
                ChangeLocation(exit.Target);
            }
        }
        else if (Same(name, "LOC25ExitToLoc22"))
        {
            Walk(Hero, Var(Vars.RucksackPointerX), 600);
            ChangeLocation("loc22");
        }
        return true;
    }

    /// <summary>Dick says he will not go that way without Anne (a different line after the first time).</summary>
    void StayWithAnne()
    {
        Chain = null;
        if (Var(Vars.DickStaysWithAnne) == 0)
        {
            IncAfter(Vars.DickStaysWithAnne, null);
            Say(Who.Dick, "LABYRINTH0049", 4, last: true);
        }
        else
            Say(Who.Dick, "LABYRINTH0049", 5, last: true);
    }

    /// <inheritdoc/>
    protected override TalkPoses? PosesOf(int speaker) => DungeonPoses(speaker);

    /// <summary>The speaker takes their talking pose.</summary>
    /// <remarks>The original's FUN_00458899.</remarks>
    public override void LineStarting(GameWorld world, int speaker) => StartTalking();

    /// <summary>The speaker stands again after their line.</summary>
    /// <remarks>The original's FUN_0045895b.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end) => StopTalking(end);

    /// <summary>On quests 9 and 10, the last speaker stands again too.</summary>
    /// <remarks>The original's FUN_00458a1d.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (At(9, 1) || At(10, 3))
            StopTalking(end);
    }
}
