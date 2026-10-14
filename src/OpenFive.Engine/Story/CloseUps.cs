using OpenFive.Engine.Game;
using OpenFive.Engine.World;

namespace OpenFive.Engine.Story;

/// <summary>
/// The close-up pictures between scenes: the model ship in the study, the next morning, the
/// hand at the keyhole and the treasure map. None of them shows the characters; most move on
/// to the next place by themselves after a moment.
/// </summary>
public abstract class CloseUp : StoryScript
{
    /// <summary>The characters stay where they were; nothing is placed here.</summary>
    protected override void Arrive() { }

    /// <summary>Goes to <paramref name="location"/> after <paramref name="delay"/> ticks.</summary>
    protected void MoveOnAfter(string location, int delay) =>
        World.Tasks.Add(new ActionTask("CHANGELOC", _ => Session.ChangeLocation(location)) { Delay = delay });
}

/// <summary>The model ship, shown after the professor's visit to the study; then the next morning.</summary>
public sealed class ShipPicture : CloseUp
{
    public override string Location => "schiff";

    protected override void Setup()
    {
        PointerOff();
        MoveOnAfter(German ? "morgen" : "morning", 30);
    }
}

/// <summary>"The next morning": leads to breakfast in the kitchen (quest 5).</summary>
public sealed class NextMorning : CloseUp
{
    readonly string _name;
    public NextMorning(string name) => _name = name;
    public override string Location => _name;

    protected override void Setup()
    {
        PointerOff();
        Episode = 0;
        Quest = 5;
        MoveOnAfter("loc05", 30);
    }
}

/// <summary>A hand at the guest-room door; returns to the hall after a moment or on a click.</summary>
public sealed class HandCloseUp : CloseUp
{
    readonly string _name;
    public HandCloseUp(string name) => _name = name;
    public override string Location => _name;

    public override bool Click(string name)
    {
        if (Same(name, "HANDExitToLoc04"))
            Session.ChangeLocation("loc04");
        return true;
    }

    protected override void Setup()
    {
        if (_name == "hand")
        {
            SetVar(Vars.V64f994, 7);
            SetVar("v64fa04", 7);
            SetVar(Vars.V64f9cc, 7);
        }
        PointerOff();
        MoveOnAfter("loc04", 40);
    }
}

/// <summary>Opening the model ship: three pictures in turn, then back to the study.</summary>
public sealed class ShipOpening : CloseUp
{
    readonly int _step;
    public ShipOpening(int step) => _step = step;
    public override string Location => $"schiffc{_step}";

    protected override void Setup()
    {
        PointerOff();
        switch (_step)
        {
            case 1:
                Sound(@"wav\fx\aufschlu.wav", delay: 1);
                Sound(@"wav\fx\schirm.wav", delay: 3);
                MoveOnAfter("schiffc2", 10);
                break;
            case 2:
                Sound(@"wav\fx\blatter.wav", delay: 1);
                MoveOnAfter("schiffc3", 10);
                break;
            default:
                MoveOnAfter("loc03", 10);
                break;
        }
    }
}

/// <summary>The treasure map, looked at from the rucksack; a click goes back.</summary>
public sealed class MapCloseUp : CloseUp
{
    public override string Location => "locKarte";

    public override bool Click(string name)
    {
        if (Same(name, "KARTEStartClick") && From.Length > 0)
            MoveOnAfter(From, 5);
        return true;
    }

    protected override void Setup() => PointerOn();
}
