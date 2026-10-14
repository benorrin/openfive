namespace OpenFive.Engine.Story;

/// <summary>
/// Remembered texts shared between scripts (see <c>StoryScript.SetText</c>); saved with the
/// game. The keys are the original's addresses. Texts used by one script only are named there.
/// </summary>
public static class StoryTexts
{
    /// <summary>"Tim" once Timmy is in the bucket in the well shaft: the next conversation ends the rescue.</summary>
    public const string WellRescue = "t63be9c";

    /// <summary>
    /// The scene at the treasure door (quest 9): "geh" George goes for the others, "geh2" the hero
    /// breaks the door with the axe, "Tuer" the others arrive.
    /// </summary>
    public const string TreasureDoorScene = "t63becc";
}
