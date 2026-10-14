using OpenFive.Engine.Game;

namespace OpenFive.Engine.Story;

/// <summary>
/// Base of the locations whose conversations are about the island and share lines: the wreck,
/// the island beach and the mainland beach.
/// </summary>
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

    /// <summary>
    /// Quest 6, step 3, back from the wreck: a remark that depends on whether the rope has been
    /// found — or, if <paramref name="otherWay"/> was chosen on the first landing, <paramref name="otherWayRemark"/>.
    /// </summary>
    /// <param name="otherWay">The first-landing choice that leads to <paramref name="otherWayRemark"/>.</param>
    /// <param name="speaker">Who speaks.</param>
    /// <param name="talk">The speaker's talk counter here (first remark at 0, second at 1).</param>
    /// <param name="withRope">The lines of "ALL0023" with the rope found.</param>
    /// <param name="withoutRope">The lines without it.</param>
    /// <param name="otherWayRemark">What is said instead.</param>
    protected void AfterTheWreck(string otherWay, int speaker, string talk, (int First, int Then) withRope,
        (int First, int Then) withoutRope, Action otherWayRemark)
    {
        if (Var(Vars.WreckVisit) != 2)
            return;
        if (Var(otherWay) != 0)
        {
            otherWayRemark();
            return;
        }
        var (first, then) = Has(Item.Rope) ? withRope : withoutRope;
        if (Var(talk) == 0)
            Say(speaker, "ALL0023", first, last: true);
        else if (Var(talk) == 1)
            Say(speaker, "ALL0023", then, last: true);
    }
}
