using OpenFive.Engine.Game;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>A dungeon passage (loc23) between the stairs (loc21) and the fork (loc26).</summary>
public sealed class Passage23 : StoryScript
{
    /// <inheritdoc/>
    public override string Location => "loc23";

    /// <summary>The hero comes in from the side they came from; on quest 9 George is with them.</summary>
    /// <remarks>The original's FUN_00456e00 and FUN_00456e96.</remarks>
    protected override void Setup()
    {
        Hero.Follower = null;
        if (FromId == 21)
        {
            Place(Hero, 384, 284, StandUpLeft);
            Show(Hero);
            WalkAndFace(227, 311, StandDownLeft);
        }
        else if (FromId == 26)
        {
            Place(Hero, 280, 600, StandUpLeft);
            Show(Hero);
            WalkAndFace(227, 500, StandUpLeft);
        }
        else
        {
            Place(Hero, 227, 311, StandDownLeft);
            Show(Hero);
        }
        if (At(9, 1))
        {
            Place(George, 372, 285, StandDownLeft);
            // As in the original, the hero is made their own follower, so nobody follows.
            Hero.Follower = Hero;
        }
        ShowParty();
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>The two exits: up to the stairs, or down to the fork.</summary>
    /// <remarks>The original's FUN_00456d48.</remarks>
    public override bool Click(string name)
    {
        if (Same(name, "LOC23ExitToLoc21"))
        {
            Walk(Hero, 341, 272);
            ChangeLocation("loc21");
        }
        else if (Same(name, "LOC23ExitToLoc26"))
        {
            Walk(Hero, Var(Vars.RucksackPointerX), 600);
            ChangeLocation("loc26");
        }
        return true;
    }
}
