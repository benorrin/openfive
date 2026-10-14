using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>A fork in the dungeon passages (loc26): ways to the stairs passage, the cells and the treasure door.</summary>
public sealed class Fork26 : StoryScript
{
    /// <summary>Where the hero comes in from each neighbouring location (they then walk to the middle).</summary>
    static readonly Dictionary<int, (int X, int Y, int Facing)> Entrances = new()
    {
        [23] = (30, 450, StandRight),
        [25] = (400, 600, StandLeft),
        [29] = (650, 450, StandLeft),
    };

    /// <inheritdoc/>
    public override string Location => "loc26";

    /// <summary>The hero comes in from where they came from; on quest 9 George is here.</summary>
    /// <remarks>The original's FUN_00458ca9 and FUN_00458d35.</remarks>
    protected override void Setup()
    {
        Hero.Follower = null;
        if (Entrances.TryGetValue(FromId, out var entrance))
        {
            Place(Hero, entrance.X, entrance.Y, entrance.Facing);
            Show(Hero);
            Walk(Hero, 452, 411);
        }
        else
        {
            Place(Hero, 452, 411, StandRight);
            Show(Hero);
        }
        if (At(9, 1))
        {
            Place(George, 360, 417, StandLeft);
            // As in the original, the hero is made their own follower, so nobody follows.
            Hero.Follower = Hero;
        }
        ShowParty();
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>The three ways out.</summary>
    /// <remarks>The original's FUN_00458ba7.</remarks>
    public override bool Click(string name)
    {
        if (Same(name, "LOC26ExitToLoc23"))
        {
            Walk(Hero, 10, 450);
            ChangeLocation("loc23");
        }
        else if (Same(name, "LOC26ExitToLoc29"))
        {
            Walk(Hero, 650, 450);
            ChangeLocation("loc29");
        }
        else if (Same(name, "LOC26ExitToLoc25"))
        {
            Walk(Hero, Var(Vars.RucksackPointerX), 600);
            ChangeLocation("loc25");
        }
        return true;
    }

    /// <inheritdoc/>
    protected override TalkPoses? PosesOf(int speaker) => DungeonPoses(speaker);

    /// <summary>The speaker takes their talking pose.</summary>
    /// <remarks>The original's FUN_00458ec6.</remarks>
    public override void LineStarting(GameWorld world, int speaker) => StartTalking();

    /// <summary>The speaker stands again after their line.</summary>
    /// <remarks>The original's FUN_00458f88.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end) => StopTalking(end);

    /// <summary>On quest 9, the last speaker stands again too.</summary>
    /// <remarks>The original's FUN_0045904a.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (At(9, 1))
            StopTalking(end);
    }
}
