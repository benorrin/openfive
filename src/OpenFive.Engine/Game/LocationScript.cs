using OpenFive.Engine.Formats;
using OpenFive.Engine.World;

namespace OpenFive.Engine.Game;

/// <summary>
/// The logic of one location (a place in the game, or a menu screen). The session calls it for
/// clicks and input, the world for object changes, the dialogue system for lines and menus. The
/// base class does nothing, so a location without story logic can still be explored.
/// </summary>
public abstract class LocationScript : ILocationHooks, IDialogueHooks
{
    /// <summary>The ".res" name of the location ("loc05").</summary>
    public abstract string Location { get; }

    /// <summary>The game this script runs in.</summary>
    protected GameSession Session { get; private set; } = null!;

    /// <summary>The game world (shortcut for <c>Session.World</c>).</summary>
    protected GameWorld World => Session.World;

    /// <summary>Connects the script to the session (done before <see cref="Enter"/>).</summary>
    internal void Attach(GameSession session) => Session = session;

    /// <summary>Connects a script that is used outside the normal location flow (item uses).</summary>
    protected void AttachTo(GameSession session) => Session = session;

    /// <summary>The location has been loaded; <paramref name="from"/> is the location the hero came from.</summary>
    public virtual void Enter(string from) { }

    /// <summary>The player clicked an object or character; returns true if the script handled it.</summary>
    public virtual bool Click(string objectName) => false;

    /// <summary>The player used <paramref name="item"/> on <paramref name="target"/> (object or character name).</summary>
    public virtual bool UseItem(int item, string target) => false;

    /// <summary>Raw click before the normal handling (menus use this); returns true to stop there.</summary>
    public virtual bool HandleClick(int x, int y) => false;

    /// <summary>The pointer position, every tick.</summary>
    public virtual void Hover(int x, int y) { }

    /// <summary>A character was typed; returns true if the script used it.</summary>
    public virtual bool KeyTyped(char c) => false;

    /// <summary>The mouse button was released.</summary>
    public virtual void ButtonReleased() { }

    /// <summary>Called every game tick while the location is shown (after the characters have moved).</summary>
    public virtual void EveryTick() { }

    /// <inheritdoc/>
    public virtual void ObjectChanged(GameWorld world, string name, bool on) { }

    /// <inheritdoc/>
    public virtual void LineStarting(GameWorld world, int speaker) { }

    /// <inheritdoc/>
    public virtual void LineQueued(GameWorld world, TaskHandle end) { }

    /// <inheritdoc/>
    public virtual void ConversationEnding(GameWorld world, TaskHandle end) { }

    /// <inheritdoc/>
    public virtual void ChoiceOpened(GameWorld world) { }

    /// <inheritdoc/>
    public virtual void ChoiceMade(GameWorld world, string scene, int line, int option) { }

    /// <inheritdoc/>
    public virtual IReadOnlyList<int>? AvailableOptions(GameWorld world, string scene, int line, int count) => null;

    /// <summary>
    /// Default exit behaviour: an object "...ExitToLOCnn" with a change-location command walks the
    /// hero to it and enters "locnn". Story scripts override <see cref="Click"/> where exits are
    /// conditional.
    /// </summary>
    protected bool DefaultExit(string objectName)
    {
        var o = World.FindObject(objectName);
        if (o == null || o.Command != ObjectCommand.ChangeLocation || ExitTarget(o.Name) is not { } target)
            return false;
        var (x, y) = Centre(o);
        var arrive = World.WalkTo(World.Hero, x, y);
        Session.ChangeLocation(target, arrive);
        return true;
    }

    /// <summary>The location an exit object leads to ("LOC14ExitToLoc17" -> "loc17"), if it names one.</summary>
    public static string? ExitTarget(string objectName)
    {
        const string marker = "ExitTo";
        int i = objectName.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (i < 0)
            return null;
        string target = objectName[(i + marker.Length)..].ToLowerInvariant();
        return target.StartsWith("loc") ? target : null;
    }

    /// <summary>Default arrival: the hero appears by the exit leading back where they came from.</summary>
    protected void DefaultArrival(string from)
    {
        if (World.Scene == null)
            return;
        var back = World.Scene.Script.Objects.FirstOrDefault(o =>
            o.Click.Length > 0 && ExitTarget(o.Name) is { } t && t.Equals(from, StringComparison.OrdinalIgnoreCase));
        var (x, y) = back != null ? Centre(back) : (400, 450);
        World.PlaceActor(World.Hero, x, y, ActorStates.StandDownRight);
    }

    /// <summary>The average of an object's click-area corners.</summary>
    static (int X, int Y) Centre(ObjectDef o) => ((int)o.Click.Average(p => p.X), (int)o.Click.Average(p => p.Y));
}

/// <summary>A location without story logic: exits lead to the next place, everything else does nothing.</summary>
public sealed class PlainLocation : LocationScript
{
    readonly string _name;

    /// <summary>Creates the script for location <paramref name="name"/>.</summary>
    public PlainLocation(string name) => _name = name;

    /// <inheritdoc/>
    public override string Location => _name;

    /// <summary>Places the hero by the exit they came through (menus have no hero).</summary>
    public override void Enter(string from)
    {
        if (!GameSession.IsMenu(_name))
            DefaultArrival(from);
    }

    /// <summary>Exits work; other objects do nothing.</summary>
    public override bool Click(string objectName) => DefaultExit(objectName);
}
