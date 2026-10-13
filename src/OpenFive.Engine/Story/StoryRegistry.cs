using OpenFive.Engine.Game;

namespace OpenFive.Engine.Story;

/// <summary>Registers the story locations with the engine.</summary>
public static class StoryRegistry
{
    static bool _done;

    public static void RegisterAll()
    {
        if (_done)
            return;
        _done = true;
        Scripts.Register("loc02", () => new Bedroom());
        Scripts.Register("loc08", () => new Cave());
        Scripts.Register("loc17", () => new Courtyard());
        Scripts.Register("loc17b", () => new CourtyardNight());
        Scripts.Register("loc28", () => new Dungeon28());
        Scripts.Register("loc29", () => new Dungeon29());
        Scripts.Register("loc22", () => new DungeonHole());
        Scripts.Register("loc21", () => new DungeonStairs());
        Scripts.Register("locextro", () => new Ending());
        Scripts.Register("loc26", () => new Fork26());
        Scripts.Register("loc06", () => new Garden());
        Scripts.Register("loc04", () => new Hall());
        Scripts.Register("loc14", () => new IslandBeach());
        Scripts.Register("loc13", () => new IslandLanding());
        Scripts.Register("loc05", () => new Kitchen());
        Scripts.Register("loc01", () => new Loc01());
        Scripts.Register("loc23", () => new Passage23());
        Scripts.Register("loc25", () => new Passage25());
        Scripts.Register("loc07", () => new Shed());
        Scripts.Register("loc09", () => new Shore());
        Scripts.Register("loc03", () => new Study());
        Scripts.Register("loc27", () => new TreasureChamber());
        Scripts.Register("loc24", () => new TreasureDoor());
        Scripts.Register("loc20", () => new WellBottom());
        Scripts.Register("loc19", () => new WellShaft());
        Scripts.Register("loc18", () => new WellTop());
        Scripts.Register("loc15", () => new Wreck());
        Scripts.Register("loc16", () => new WreckCabin());
    }
}
