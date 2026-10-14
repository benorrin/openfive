using OpenFive.Engine.Game;
using OpenFive.Engine.World;

namespace OpenFive.Engine.Story;

/// <summary>
/// Where the boat lands on Kirrin Island (loc13), with exits to the island beach, the wreck and
/// back to the mainland. On the first landing (quest 6, step 3) the children talk about where to
/// go, and each exit is announced by one of them before they go.
/// </summary>
public sealed class IslandLanding : StoryScript
{
    /// <summary>An exit: where it leads, and who announces it on the first landing with which line.</summary>
    /// <param name="Target">The location it leads to.</param>
    /// <param name="Speaker">Who announces it.</param>
    /// <param name="Line">The line of "WRACK0022" they say.</param>
    /// <param name="ChoiceFlag">Set when this way is chosen first (null: not recorded).</param>
    sealed record Exit(string Target, int Speaker, int Line, string? ChoiceFlag);

    static readonly Dictionary<string, Exit> Exits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LOC13ExitToLoc14"] = new("loc14", Who.Anne, 7, Vars.ChoseIslandBeach),
        ["LOC13ExitToLoc15"] = new("loc15", Who.Julian, 6, null),
        ["LOC13ExitToLoc09"] = new("loc09", Who.Julian, 5, Vars.ChoseMainland),
    };

    /// <summary>The exit chosen on the first landing ("LOC14"...), taken once its line has been queued.</summary>
    const string LeavingFor = "t63be38";

    /// <inheritdoc/>
    public override string Location => "loc13";

    /// <summary>The characters stop; on the first landing they talk about where to go.</summary>
    /// <remarks>The original's FUN_0044738e.</remarks>
    protected override void Setup()
    {
        SetVar(Vars.V683260, 1);
        foreach (var a in new[] { Hero, Dick, Anne, Julian, George, Timmy })
            ClearQueue(a);
        if (At(6, 3))
        {
            if (Var(Vars.V64d468) != 0)
            {
                Chain = null;
                Say(Who.Anne, "WRACK0022", 1);
                Say(Who.George, "WRACK0022", 2);
                Say(Who.Anne, "WRACK0022", 3);
                Say(Who.Julian, "WRACK0022", 4, last: true);
                SetVar(Vars.V64d468, 0);
            }
        }
        else if (At(7, 1) && Has(Item.ClosedTin))
            Episode = 2;
        if (Quest > 7)
            Obj("LOC13ExitToLoc15", false);
        Anim("Moewen", -1);
    }

    /// <summary>The characters are where the boat landed.</summary>
    protected override void Arrive() { }

    /// <summary>An exit: on the first landing it is announced first (and taken after the line), otherwise taken at once.</summary>
    /// <remarks>The original's FUN_0044708e.</remarks>
    public override bool Click(string name)
    {
        if (!Exits.TryGetValue(name, out var exit))
            return true;
        Chain = null;
        if (!At(6, 3))
        {
            ChangeLocation(exit.Target);
            return true;
        }
        SetText(LeavingFor, exit.Target.ToUpperInvariant());
        Say(exit.Speaker, "WRACK0022", exit.Line);
        if (exit.ChoiceFlag != null && Var(Vars.WreckVisit) == 1)
        {
            SetVar(Vars.WreckVisit, 2);
            SetVar(exit.ChoiceFlag, 1);
            SetVar(Vars.DickBeachTalk, 0);
            SetVar(Vars.GeorgeBeachTalk, 0);
            SetVar(Vars.AnneBeachTalk, 0);
            SetVar(Vars.DickIslandTalk, 0);
            SetVar(Vars.GeorgeIslandTalk, 0);
            SetVar(Vars.AnneIslandTalk, 0);
        }
        return true;
    }

    /// <summary>Once an exit's announcement is queued, the children go that way.</summary>
    /// <remarks>The original's FUN_004475cd.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end)
    {
        string target = Text(LeavingFor);
        if (target is not ("LOC09" or "LOC14" or "LOC15"))
            return;
        SetText(LeavingFor, "");
        ChangeLocation(target.ToLowerInvariant(), end);
    }
}
