using OpenFive.Engine.World;

namespace OpenFive.Engine.Game;

/// <summary>
/// The story logic of one location. OpenFive reimplements each location's behaviour in a
/// subclass; the base class does nothing, so unknown locations can still be explored.
/// </summary>
public abstract class LocationScript : ILocationHooks, IDialogueHooks
{
    /// <summary>The ".res" name of the location ("loc05").</summary>
    public abstract string Location { get; }

    protected GameSession Session { get; private set; } = null!;
    protected GameWorld World => Session.World;

    internal void Attach(GameSession session) => Session = session;
    protected void AttachTo(GameSession session) => Session = session;

    /// <summary>The location has been loaded; <paramref name="from"/> is the location the hero came from.</summary>
    public virtual void Enter(string from) { }

    /// <summary>The player clicked an object; return true if the script handled it.</summary>
    public virtual bool Click(string objectName) => false;

    /// <summary>The player used <paramref name="item"/> on <paramref name="target"/> (object or character name).</summary>
    public virtual bool UseItem(int item, string target) => false;

    /// <summary>Raw click before the normal handling (menus use this); return true to stop there.</summary>
    public virtual bool HandleClick(int x, int y) => false;

    /// <summary>Pointer position, every tick.</summary>
    public virtual void Hover(int x, int y) { }

    /// <summary>A character was typed; return true if the script used it.</summary>
    public virtual bool KeyTyped(char c) => false;

    /// <summary>The mouse button was released.</summary>
    public virtual void ButtonReleased() { }

    /// <summary>
    /// Default exit behaviour: an object "...ExitToLOCnn" with a change-location command walks the
    /// hero to it and enters "locnn". Story scripts override <see cref="Click"/> where exits are
    /// conditional.
    /// </summary>
    protected bool DefaultExit(string objectName)
    {
        var o = World.FindObject(objectName);
        if (o == null || o.Command != Formats.ObjectCommand.ChangeLocation)
            return false;
        string? target = ExitTarget(o.Name);
        if (target == null)
            return false;
        int cx = (int)o.Click.Average(p => p.X), cy = (int)o.Click.Average(p => p.Y);
        var arrive = World.WalkTo(World.Hero, cx, cy);
        Session.ChangeLocation(target, arrive);
        return true;
    }

    /// <summary>The location an exit object leads to ("LOC14ExitToLoc17" -> "loc17"), if it names one.</summary>
    public static string? ExitTarget(string objectName)
    {
        int i = objectName.IndexOf("ExitTo", StringComparison.OrdinalIgnoreCase);
        if (i < 0)
            return null;
        string t = objectName[(i + 6)..].ToLowerInvariant();
        return t.StartsWith("loc") ? t : null;
    }

    /// <summary>Default arrival: the hero (and party) appear by the exit leading back where they came from.</summary>
    protected void DefaultArrival(string from)
    {
        if (World.Scene == null)
            return;
        var back = World.Scene.Script.Objects.FirstOrDefault(o =>
            ExitTarget(o.Name) is { } t && t.Equals(from, StringComparison.OrdinalIgnoreCase) && o.Click.Length > 0);
        int x = 400, y = 450;
        if (back != null)
            (x, y) = ((int)back.Click.Average(p => p.X), (int)back.Click.Average(p => p.Y));
        World.PlaceActor(World.Hero, x, y, ActorStates.StandDownRight);
    }

    public virtual void ObjectChanged(GameWorld world, string name, bool on) { }
    public virtual void LineStarting(GameWorld world, int speaker) { }

    /// <summary>Called every game tick while the location is shown.</summary>
    public virtual void EveryTick() { }
    public virtual void LineQueued(GameWorld world, TaskHandle end) { }
    public virtual void ConversationEnding(GameWorld world, TaskHandle end) { }
    public virtual void ChoiceOpened(GameWorld world) { }
    public virtual void ChoiceMade(GameWorld world, string scene, int line, int option) { }
    public virtual IReadOnlyList<int>? AvailableOptions(GameWorld world, string scene, int line, int count) => null;
}

/// <summary>A location without story logic yet: objects show their names, exits lead to the next place.</summary>
public sealed class PlainLocation : LocationScript
{
    readonly string _name;
    public PlainLocation(string name) => _name = name;
    public override string Location => _name;

    public override void Enter(string from)
    {
        if (!GameSession.IsMenu(_name))
            DefaultArrival(from);
    }

    public override bool Click(string objectName) => DefaultExit(objectName);
}
