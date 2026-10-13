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
        Scripts.Register("loc06", () => new Garden());
    }
}
