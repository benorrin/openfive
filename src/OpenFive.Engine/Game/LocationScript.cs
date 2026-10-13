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

    /// <summary>The location has been loaded; <paramref name="from"/> is the location the hero came from.</summary>
    public virtual void Enter(string from) { }

    /// <summary>The player clicked an object; return true if the script handled it.</summary>
    public virtual bool Click(string objectName) => false;

    /// <summary>The player used <paramref name="item"/> on <paramref name="target"/> (object or character name).</summary>
    public virtual bool UseItem(int item, string target) => false;

    public virtual void ObjectChanged(GameWorld world, string name, bool on) { }
    public virtual void LineQueued(GameWorld world, TaskHandle end) { }
    public virtual void ConversationEnding(GameWorld world, TaskHandle end) { }
    public virtual void ChoiceOpened(GameWorld world) { }
    public virtual void ChoiceMade(GameWorld world, string scene, int line, int option) { }
}

/// <summary>A location without special behaviour: objects show their names, exits do nothing.</summary>
public sealed class PlainLocation : LocationScript
{
    readonly string _name;
    public PlainLocation(string name) => _name = name;
    public override string Location => _name;
}
