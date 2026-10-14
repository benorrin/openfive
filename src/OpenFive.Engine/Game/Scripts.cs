namespace OpenFive.Engine.Game;

/// <summary>Finds the script for a location: menus, story locations, or a plain explorable place.</summary>
public static class Scripts
{
    static readonly Dictionary<string, Func<LocationScript>> Menus = new(StringComparer.OrdinalIgnoreCase)
    {
        ["logo"] = () => new LogoScreen(),
        ["menue"] = () => new MainMenu(),
        ["load"] = () => new SaveLoadScreen(false),
        ["save"] = () => new SaveLoadScreen(true),
        ["options"] = () => new OptionsScreen(),
    };

    /// <summary>A new script for <paramref name="location"/>.</summary>
    public static LocationScript For(string location) =>
        Menus.TryGetValue(location, out var menu) ? menu()
        : Story.StoryRegistry.Create(location) ?? new PlainLocation(location);
}
