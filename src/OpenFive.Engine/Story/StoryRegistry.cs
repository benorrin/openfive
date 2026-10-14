using OpenFive.Engine.Game;

namespace OpenFive.Engine.Story;

/// <summary>The story scripts, by location name.</summary>
public static class StoryRegistry
{
    static readonly Dictionary<string, Func<LocationScript>> Locations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["loc02"] = () => new Bedroom(),
        ["loc08"] = () => new Cave(),
        ["loc17"] = () => new Courtyard(),
        ["loc17b"] = () => new CourtyardNight(),
        ["loc28"] = () => new Dungeon28(),
        ["loc29"] = () => new Dungeon29(),
        ["loc22"] = () => new DungeonHole(),
        ["loc21"] = () => new DungeonStairs(),
        ["locextro"] = () => new Ending(),
        ["loc26"] = () => new Fork26(),
        ["loc06"] = () => new Garden(),
        ["loc04"] = () => new Hall(),
        ["loc14"] = () => new IslandBeach(),
        ["loc13"] = () => new IslandLanding(),
        ["loc05"] = () => new Kitchen(),
        ["loc01"] = () => new Loc01(),
        ["loc23"] = () => new Passage23(),
        ["loc25"] = () => new Passage25(),
        ["loc07"] = () => new Shed(),
        ["loc09"] = () => new Shore(),
        ["loc03"] = () => new Study(),
        ["loc27"] = () => new TreasureChamber(),
        ["loc24"] = () => new TreasureDoor(),
        ["loc20"] = () => new WellBottom(),
        ["loc19"] = () => new WellShaft(),
        ["loc18"] = () => new WellTop(),
        ["loc15"] = () => new Wreck(),
        ["loc16"] = () => new WreckCabin(),
        ["schiff"] = () => new ShipPicture(),
        ["morgen"] = () => new NextMorning("morgen"),
        ["morning"] = () => new NextMorning("morning"),
        ["hand"] = () => new HandCloseUp("hand"),
        ["hand2"] = () => new HandCloseUp("hand2"),
        ["schiffc1"] = () => new ShipOpening(1),
        ["schiffc2"] = () => new ShipOpening(2),
        ["schiffc3"] = () => new ShipOpening(3),
        ["locKarte"] = () => new MapCloseUp(),
    };

    /// <summary>A new script for a story location, or null if the location has none.</summary>
    public static LocationScript? Create(string location) => Locations.TryGetValue(location, out var create) ? create() : null;
}
