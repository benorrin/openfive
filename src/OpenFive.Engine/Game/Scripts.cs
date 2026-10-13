namespace OpenFive.Engine.Game;

/// <summary>Finds the script for a location: menus, story locations, or a plain explorable place.</summary>
public static class Scripts
{
    static readonly Dictionary<string, Func<LocationScript>> Registry = new(StringComparer.OrdinalIgnoreCase)
    {
        ["logo"] = () => new LogoScreen(),
        ["menue"] = () => new MainMenu(),
        ["load"] = () => new SaveLoadScreen(false),
        ["save"] = () => new SaveLoadScreen(true),
        ["options"] = () => new OptionsScreen(),
    };

    /// <summary>Registers a story script (called by the story assembly at start-up).</summary>
    public static void Register(string location, Func<LocationScript> factory) => Registry[location] = factory;

    public static LocationScript For(string location)
    {
        Story.StoryRegistry.RegisterAll();
        return Registry.TryGetValue(location, out var f) ? f() : new PlainLocation(location);
    }

    public static IEnumerable<string> Implemented => Registry.Keys;
}
