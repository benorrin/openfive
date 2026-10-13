namespace OpenFive.Engine.World;

/// <summary>Something a script can wait for: a global task, or an entry in an actor's queue.</summary>
public abstract class TaskHandle
{
    /// <summary>True once a task waiting on this handle may run.</summary>
    public abstract bool Released { get; }
}

/// <summary>
/// A task in the global list. It waits for <see cref="WaitFor"/>, then for <see cref="Delay"/>
/// ticks, then runs <see cref="Run"/> every tick until that returns true (finished).
/// </summary>
public abstract class GameTask : TaskHandle
{
    public string Name = "";
    public TaskHandle? WaitFor;
    public int Delay;
    public bool Finished { get; internal set; }
    public override bool Released => Finished;

    /// <summary>Called each tick once ready; return true when the task is complete.</summary>
    protected internal abstract bool Run(GameWorld world);

    /// <summary>Called every tick before the task is ready (rarely needed).</summary>
    protected internal virtual void Waiting(GameWorld world) { }
}

/// <summary>
/// The global task list. Like the original it has a fixed number of slots, new tasks take the
/// lowest free slot and tasks are advanced in slot order each tick, which decides whether a task
/// released by another one starts in the same tick or the next.
/// </summary>
public sealed class TaskScheduler
{
    public const int Capacity = 600;
    readonly GameTask?[] _slots = new GameTask?[Capacity];

    public IEnumerable<GameTask> Active => _slots.Where(t => t != null)!;

    public T Add<T>(T task) where T : GameTask
    {
        for (int i = 0; i < Capacity; i++)
        {
            if (_slots[i] == null)
            {
                _slots[i] = task;
                return task;
            }
        }
        throw new InvalidOperationException("task list is full");
    }

    public void Remove(GameTask task)
    {
        for (int i = 0; i < Capacity; i++)
            if (_slots[i] == task)
            {
                _slots[i] = null;
                task.Finished = true;
            }
    }

    public void Clear(Func<GameTask, bool>? keep = null)
    {
        for (int i = 0; i < Capacity; i++)
            if (_slots[i] is { } t && (keep == null || !keep(t)))
            {
                t.Finished = true;
                _slots[i] = null;
            }
    }

    public IEnumerable<T> OfType<T>() where T : GameTask => _slots.OfType<T>();

    public void Tick(GameWorld world)
    {
        for (int i = 0; i < Capacity; i++)
        {
            var t = _slots[i];
            if (t == null)
                continue;
            if (t.WaitFor != null)
            {
                if (!t.WaitFor.Released)
                {
                    t.Waiting(world);
                    continue;
                }
                t.WaitFor = null;
            }
            if (t.Delay > 0)
            {
                t.Delay--;
                continue;
            }
            if (t.Run(world))
            {
                t.Finished = true;
                if (_slots[i] == t)
                    _slots[i] = null;
            }
        }
    }
}

/// <summary>A task that performs one action and finishes.</summary>
public sealed class ActionTask : GameTask
{
    readonly Action<GameWorld> _action;
    public ActionTask(string name, Action<GameWorld> action)
    {
        Name = name;
        _action = action;
    }

    protected internal override bool Run(GameWorld world)
    {
        _action(world);
        return true;
    }
}
