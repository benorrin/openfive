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
    /// <summary>A name for diagnostics (the original's task type names, such as "TALK").</summary>
    public string Name = "";

    /// <summary>The task does not start before this is released (null: no wait).</summary>
    public TaskHandle? WaitFor;

    /// <summary>Ticks to wait after <see cref="WaitFor"/> is released.</summary>
    public int Delay;

    /// <summary>The task has completed (or was removed).</summary>
    public bool Finished { get; internal set; }

    /// <summary>True once the task has begun acting (its wait and delay are over).</summary>
    public bool Started { get; internal set; }

    /// <summary>A task releases what waits for it when it finishes.</summary>
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
    /// <summary>The number of slots.</summary>
    public const int Capacity = 600;

    readonly GameTask?[] _slots = new GameTask?[Capacity];

    /// <summary>The tasks in the list, in slot order.</summary>
    public IEnumerable<GameTask> Active => _slots.OfType<GameTask>();

    /// <summary>Puts a task into the lowest free slot and returns it.</summary>
    /// <exception cref="InvalidOperationException">All slots are taken.</exception>
    public T Add<T>(T task) where T : GameTask
    {
        int free = Array.IndexOf(_slots, null);
        if (free < 0)
            throw new InvalidOperationException("task list is full");
        _slots[free] = task;
        return task;
    }

    /// <summary>Takes a task off the list; it counts as finished.</summary>
    public void Remove(GameTask task)
    {
        for (int i = 0; i < Capacity; i++)
            if (_slots[i] == task)
            {
                _slots[i] = null;
                task.Finished = true;
            }
    }

    /// <summary>Takes every task (except those <paramref name="keep"/> accepts) off the list.</summary>
    public void Clear(Func<GameTask, bool>? keep = null)
    {
        for (int i = 0; i < Capacity; i++)
            if (_slots[i] is { } t && (keep == null || !keep(t)))
            {
                t.Finished = true;
                _slots[i] = null;
            }
    }

    /// <summary>The tasks of one type, in slot order.</summary>
    public IEnumerable<T> OfType<T>() where T : GameTask => _slots.OfType<T>();

    /// <summary>Advances every task by one tick, in slot order.</summary>
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
            t.Started = true;
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

    /// <summary>Creates a task named <paramref name="name"/> that runs <paramref name="action"/>.</summary>
    public ActionTask(string name, Action<GameWorld> action)
    {
        Name = name;
        _action = action;
    }

    /// <summary>Runs the action; the task is then finished.</summary>
    protected internal override bool Run(GameWorld world)
    {
        _action(world);
        return true;
    }
}
