using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The cabin of the wreck (loc16): the cupboard Uncle Quentin's tin lies in once it has been
/// unscrewed, and things to look at.
/// </summary>
public sealed class WreckCabin : StoryScript
{
    /// <summary>Things that are only looked at: where the hero stands and which way they face.</summary>
    static readonly Dictionary<string, (int X, int Y, int Facing)> LookAt = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LOC16Tisch"] = (294, 515, StandLeft),
        ["LOC16Loch"] = (309, 527, StandUpRight),
        ["LOC16Algen"] = (308, 531, StandDownRight),
    };

    /// <inheritdoc/>
    public override string Location => "loc16";

    /// <summary>
    /// The hero comes down the ladder; the cupboard is shown open or shut and the tin in it
    /// until taken. The first visit moves the story on (quest 6, step 4).
    /// </summary>
    /// <remarks>The original's FUN_0044c6d6 and FUN_0044c7a6.</remarks>
    protected override void Setup()
    {
        SetVar(Vars.OriginalPlaceFlag, 1);
        Depth(@"bmp\obj\locs\loc16\schrank.brg", 3000);
        Place(Hero, 206, 578, StandDownRight);
        Show(Hero);
        ShowParty();
        WalkAndFace(347, 546, StandDownLeft);
        MoveObj("LOC16SchrankZu", 629, 136);
        MoveObj("LOC16SchrankAuf", 675, 131);
        bool open = Var(Vars.CabinCupboardOpen) != 0;
        Obj("LOC16SchrankZu", !open);
        Obj("LOC16Scharnier", !open);
        Obj("LOC16SchrankAuf", open);
        Obj("LOC16Dose", Var(Vars.TinTaken) == 0);
        if (At(6, 3))
        {
            Episode = 4;
            SetVar(Vars.AnneBeachTalk, 0);
            SetVar(Vars.AnneIslandTalk, 0);
            SetVar(Vars.AnneWreckTalk, 0);
            SetVar(Vars.JulianBeachTalk, 0);
            SetVar(Vars.JulianIslandTalk, 0);
            SetVar(Vars.JulianWreckTalk, 0);
            SetVar(Vars.GeorgeBeachTalk, 0);
            SetVar(Vars.GeorgeIslandTalk, 0);
            SetVar(Vars.GeorgeWreckTalk, 0);
            SetVar(Vars.CabinVisited, 1);
        }
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>Looking around, taking the tin, the cupboard, and the ladder back up to the deck.</summary>
    /// <remarks>The original's FUN_0044c3b5.</remarks>
    public override bool Click(string name)
    {
        if (LookAt.TryGetValue(name, out var spot))
            WalkAndFace(spot.X, spot.Y, spot.Facing);
        else if (Same(name, "LOC16Dose"))
        {
            Walk(Hero, 605, 449);
            var reached = Chain = State(Hero, TakeLowRight);
            Say(Who.Dick, "KAJUETE0024", 6, last: true);
            ObjAfter("LOC16Dose", false, reached, 5);
            IncAfter(Vars.TinTaken, reached);
        }
        else if (Same(name, "LOC16Schrank"))
        {
            Walk(Hero, 547, 464);
            Chain = State(Hero, TakeLowRight);
            if (At(6, 4))
                Say(Who.Dick, "KAJUETE0024", 1, last: true);
        }
        else if (Same(name, "LOC16ExitToLoc15"))
        {
            Walk(Hero, 206, 578);
            var reached = Chain = State(Hero, StandDownRight);
            int delay = 0;
            if (At(6, 4))
            {
                // Dick remarks on the cupboard before they leave.
                PointerAfter(false, reached, x: 400, y: 400);
                Say(Who.Dick, "KAJUETE0024", 9, last: true, after: reached);
                delay = 30;
            }
            ChangeLocation("loc15", reached, delay);
        }
        return true;
    }

    /// <summary>Taking the tin (its object switched off while being picked up) puts it into the rucksack.</summary>
    /// <remarks>The original's FUN_0044c721.</remarks>
    public override void ObjectChanged(GameWorld world, string name, bool on)
    {
        if (!on && Same(name, "LOC16Dose") && Var(Vars.TinTaken) == 1)
        {
            Give(Item.ClosedTin);
            SetVar(Vars.TinTaken, 2);
        }
    }
}
