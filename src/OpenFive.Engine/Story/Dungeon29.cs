using OpenFive.Engine.Game;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>A dungeon cell (loc29) reached from the stairs or the fork; its only way out leads back.</summary>
public sealed class Dungeon29 : StoryScript
{
    /// <inheritdoc/>
    public override string Location => "loc29";

    /// <summary>The hero comes in on their own (nobody follows) and steps into the cell.</summary>
    /// <remarks>The original's FUN_0045b134.</remarks>
    protected override void Setup()
    {
        Hero.Follower = null;
        Place(Hero, 150, 446, StandDownLeft);
        Show(Hero);
        ShowParty();
        Walk(Hero, 300, 500);
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>The exit leads back to where the hero came from: the stairs (loc21) or the fork (loc26).</summary>
    /// <remarks>The original's FUN_0045b09a.</remarks>
    public override bool Click(string name)
    {
        if (Same(name, "LOC29ExitVerlies2"))
        {
            Walk(Hero, 209, 417);
            ChangeLocation(FromId == 21 ? "loc21" : "loc26");
        }
        return true;
    }
}
