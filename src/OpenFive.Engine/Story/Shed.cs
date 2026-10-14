using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The garden shed (loc07): tools to look at, and four things to take — the axe, the rope, the
/// oil can and the screwdriver.
/// </summary>
public sealed class Shed : StoryScript
{
    /// <summary>
    /// A thing lying in the shed that can be taken. While <paramref name="Placing"/> is set the
    /// object is being put in place, so switching it off does not count as taking it.
    /// </summary>
    /// <param name="Object">The object showing the thing.</param>
    /// <param name="X">Where it lies.</param>
    /// <param name="Y">Where it lies.</param>
    /// <param name="Placing">Set while the object is being placed.</param>
    /// <param name="Available">Its story record's "available" flag.</param>
    /// <param name="Taken">Its story record's "taken" counter.</param>
    /// <param name="Item">The rucksack item it becomes.</param>
    sealed record Pickup(string Object, int X, int Y, string Placing, string Available, string Taken, int Item);

    /// <summary>The things to take, in the order the original places them.</summary>
    static readonly Pickup[] Pickups =
    {
        new("LOC07Axt", 95, 259, Vars.ShedPlacingAxe, Vars.AxeAvailable, Vars.AxeTaken, Item.Axe),
        new("LOC07Seil", 136, 170, Vars.ShedPlacingRope, Vars.RopeAvailable, Vars.RopeTaken, Item.Rope),
        new("LOC07Schraubenzieher", 13, 427, Vars.ShedPlacingScrewdriver, Vars.ScrewdriverAvailable,
            Vars.ScrewdriverTaken, Item.Screwdriver),
        new("LOC07Oelkanne", 35, 378, Vars.ShedPlacingOilCan, Vars.OilCanAvailable, Vars.OilCanTaken, Item.EmptyOilCan),
    };

    /// <summary>Things that are only looked at: where the hero stands and which way they face.</summary>
    static readonly Dictionary<string, (int X, int Y, int Facing)> LookAt = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LOC07Sichel"] = (229, 580, StandLeft),
        ["LOC07Säge"] = (281, 566, StandLeft),
        ["LOC07Kisten2"] = (328, 567, StandLeft),
        ["LOC07Zange"] = (309, 536, StandLeft),
        ["LOC07Hammer"] = (338, 522, StandLeft),
        ["LOC07Farbe"] = (338, 522, StandLeft),
        ["LOC07Farbe2"] = (379, 536, StandUpLeft),
        ["LOC07Gerümpel"] = (336, 526, StandUpLeft),
        ["LOC07Lacke"] = (336, 526, StandUpLeft),
        ["LOC07Kisten1"] = (468, 538, StandUpRight),
        ["LOC07Faesser"] = (546, 548, StandUpRight),
        ["LOC07Gartengeräte"] = (354, 522, StandUpRight),
        ["LOC07Ruder"] = (398, 532, StandUpRight),
    };

    /// <inheritdoc/>
    public override string Location => "loc07";

    /// <summary>Puts the things in place, and brings the hero in through the door, which closes behind them.</summary>
    /// <remarks>The original's FUN_0043f82c.</remarks>
    protected override void Setup()
    {
        foreach (var pickup in Pickups)
            PutInPlace(pickup);
        Place(Hero, 689, 597, StandDownLeft);
        Show(Hero);
        WalkAndFace(571, 590, StandDownLeft);
        ShowParty();
        AnimPos("DoCloseDoor", 665, 53);
        AnimPos("DoOpenDoor", 665, 53);
        MoveObj("LOC07ClosedDoor", 665, 53);
        Obj("LOC07ClosedDoor", false);
        Anim("DoCloseDoor");
        Sound(@"wav\fx\stuerzu.wav", delay: 3);
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>Looks at a tool, takes a thing, or goes out of the door into the garden.</summary>
    /// <remarks>The original's FUN_0043f248.</remarks>
    public override bool Click(string name)
    {
        if (LookAt.TryGetValue(name, out var spot))
            WalkAndFace(spot.X, spot.Y, spot.Facing);
        else if (Same(name, "LOC07Axt"))
            PickUp(200, 561, TakeLowUpLeft, "LOC07Axt", "axthaken");
        else if (Same(name, "LOC07Seil"))
        {
            var reached = WalkAndFace(286, 561, TakeLowUpLeft);
            Sound(@"wav\fx\seilnehm.wav", reached, 3);
            ObjAfter("LOC07Seil", false, reached, 4);
        }
        else if (Same(name, "LOC07Oelkanne"))
            PickUp(150, 561, TakeMidLeft, "LOC07Oelkanne", "oeltisch");
        else if (Same(name, "LOC07Schraubenzieher"))
            PickUp(150, 561, TakeMidLeft, "LOC07Schraubenzieher", "holzholz");
        else if (Same(name, "LOC07ExitToLOC06"))
        {
            var reached = WalkAndFace(634, 597, TakeMidRight);
            Sound(@"wav\fx\stuerauf.wav", reached);
            var opened = Anim("DoOpenDoor", after: reached, delay: 3, endObject: "LOC07ClosedDoor", endObjectOn: true);
            ChangeLocation("loc06", opened);
        }
        return true;
    }

    /// <summary>
    /// Taking a thing switches its object off; that puts it into the rucksack (unless the object
    /// is only being placed).
    /// </summary>
    /// <remarks>The original's FUN_0043fb9a.</remarks>
    public override void ObjectChanged(GameWorld world, string name, bool on)
    {
        if (on || Pickups.FirstOrDefault(p => Same(name, p.Object) && Var(p.Placing) == 0) is not { } taken)
            return;
        Give(taken.Item);
        SetVar(taken.Taken, 1);
    }

    /// <summary>Walks up to a thing, reaches for it, and takes it with a sound.</summary>
    void PickUp(int x, int y, int reach, string obj, string sound)
    {
        var reached = WalkAndFace(x, y, reach);
        ObjAfter(obj, false, reached, 4);
        Sound($@"wav\fx\{sound}.wav", reached, 3);
    }

    /// <summary>Moves a thing to where it lies, and shows it while it is available and not taken.</summary>
    /// <remarks>The original's FUN_0043f97e, FUN_0043fa05, FUN_0043fb13 and FUN_0043fa8c.</remarks>
    void PutInPlace(Pickup p)
    {
        SetVar(p.Placing, 1);
        MoveObj(p.Object, p.X, p.Y);
        Obj(p.Object, false);
        SetVar(p.Placing, 0);
        if (Var(p.Available) == 1 && Var(p.Taken) == 0)
            Obj(p.Object, true);
    }
}
