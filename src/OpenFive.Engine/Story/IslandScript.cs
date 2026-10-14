using OpenFive.Engine.Game;

namespace OpenFive.Engine.Story;

/// <summary>Base of the locations on Kirrin Island that share conversations (the wreck and the island beach).</summary>
public abstract class IslandScript : StoryScript
{
    /// <summary>
    /// Quest 6, step 4: George on the locked cupboard in the cabin; then Dick and Julian on what
    /// to try next — whether something has been tried on the lock already, and whether the knife
    /// broke or the screwdriver is at hand.
    /// </summary>
    protected void TalkAboutTheCupboard()
    {
        Say(Who.George, "ALL0024", 10, last: true);
        if (Var(Vars.CupboardLockTried) == 0)
        {
            Say(Who.Dick, "ALL0024", 16);
            Say(Who.Julian, "ALL0024", 17, last: true);
            Say(Who.Dick, "ALL0024", Has(Item.Screwdriver) ? 19 : 18, last: true);
        }
        else
        {
            Say(Who.Dick, "ALL0024", 11);
            Say(Who.Julian, "ALL0024", 12, last: true);
            if (Has(Item.BrokenKnife))
                Say(Who.Dick, "ALL0024", 13, last: true);
            Say(Who.Dick, "ALL0024", Has(Item.Screwdriver) ? 15 : 14, last: true);
        }
    }
}
