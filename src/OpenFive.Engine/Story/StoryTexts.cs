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

    /// <summary>What the conversation in the kitchen is about (a character's name, "Sandwich", "nobody"...).</summary>
    public const string KitchenTopic = "t63bdf0";

    /// <summary>The scene running in the hall ("Koffer" after the suitcase has been opened...).</summary>
    public const string HallScene = "t63bddc";

    /// <summary>The scene running in the study ("Schiff" when the key has been tried on the model ship...).</summary>
    public const string StudyScene = "t63bdb4";

    /// <summary>The scene running in the castle courtyard ("Axt" the bush cut down, "EfeuMitSeil" the rope on the ivy...).</summary>
    public const string CourtyardScene = "t63be5c";
}
