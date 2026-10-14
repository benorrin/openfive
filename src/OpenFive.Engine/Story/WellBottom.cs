using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The bottom of the well (loc20). On quest 9 the hero climbs down and goes on into the dungeon;
/// on quest 10 the hero crawls out of the dungeon here and climbs back up to the island beach.
/// </summary>
public sealed class WellBottom : StoryScript
{
    /// <inheritdoc/>
    public override string Location => "loc20";

    /// <summary>The hero arrives alone; the scene of the story step plays.</summary>
    /// <remarks>The original's FUN_0045552e and FUN_004555ba.</remarks>
    protected override void Setup()
    {
        Hero.Follower = null;
        if (At(9, 1))
        {
            Chain = null;
            Say(Who.Dick, "BRUNNEN0046", 1);
            Say(Who.George, "BRUNNEN0046", 2, last: true);
            // As in the original, the hero is made their own follower, so nobody follows.
            Hero.Follower = Hero;
        }
        else if (At(10, 3))
        {
            // Escaped from the dungeon: the hero crawls in (see EveryTick).
            Place(Hero, 716, 448, CrawlRight);
            Show(Hero);
            ShowParty();
            PointerOff();
            Chain = null;
            Say(Who.Dick, "BRUNNEN0050", 1, last: true, delay: 10);
            SetVar(Vars.AnneIslandTalk, 0);
            SetVar(Vars.DickIslandTalk, 0);
            SetVar(Vars.JulianIslandTalk, 0);
            SetVar(Vars.GeorgeIslandTalk, 0);
        }
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>The way on into the dungeon (the hole, loc22).</summary>
    /// <remarks>The original's FUN_004554d2.</remarks>
    public override bool Click(string name)
    {
        if (Same(name, "LOC20ExitToLoc22"))
            ChangeLocation("loc22");
        return true;
    }

    /// <summary>After the lines: on into the dungeon (quest 9), or up to the island beach and quest 11.</summary>
    /// <remarks>The original's FUN_00455773.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        if (At(9, 1))
        {
            PointerAfter(false, end, x: 400, y: 400);
            ChangeLocation("loc22", end);
        }
        else if (At(10, 3))
        {
            PointerAfter(false, end, x: 400, y: 400);
            ChangeLocation("loc14", end, 4);
            SetVar(Vars.V683260, 0);
            SetAfter(Vars.EpisodeNumber, 1, end);
            SetAfter(Vars.QuestNumber, 11, end);
        }
    }

    /// <summary>A crawling hero moves ten pixels right every tick.</summary>
    /// <remarks>The original's FUN_00455877.</remarks>
    public override void EveryTick()
    {
        if (Hero.State == CrawlRight)
            Hero.X += 10;
    }
}
