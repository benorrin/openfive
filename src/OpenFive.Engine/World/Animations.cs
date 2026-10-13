using OpenFive.Engine.Formats;

namespace OpenFive.Engine.World;

/// <summary>An animation definition together with the resource it draws from.</summary>
public sealed class AnimInfo
{
    public required AnimDef Def;
    public required ResourceEntry Resource;
    /// <summary>Current position (scripts may move an animation).</summary>
    public int X, Y;
    /// <summary>Drawing scale (scripts may scale an animation).</summary>
    public double Scale = 1;
}

/// <summary>
/// Plays a named animation on its resource's sprite. Each tick shows the next frame; after the
/// last frame it starts over while loops remain (-1 = forever), otherwise it hides the sprite,
/// optionally switches an object on/off, and finishes.
/// </summary>
public sealed class AnimTask : GameTask
{
    public readonly AnimInfo Anim;
    public int Loops;
    public int Frame;
    /// <summary>Object to switch when the animation ends (null = none).</summary>
    public string? EndObject;
    public bool EndObjectOn;

    public AnimTask(AnimInfo anim, int loops)
    {
        Anim = anim;
        Name = anim.Def.Name;
        Loops = loops;
    }

    protected internal override bool Run(GameWorld world)
    {
        var sprite = world.Sprites.For(Anim.Resource);
        int n = Anim.Def.Frames.Length;
        if (sprite == null || n == 0)
            return true;
        world.OnAnimFrame(this);
        if (Frame < n - 1)
        {
            Frame++;
            Show(sprite);
            return false;
        }
        if (Loops == -1 || Loops > 1)
        {
            if (Loops != -1)
                Loops--;
            Frame = 0;
            Show(sprite);
            return false;
        }
        sprite.Visible = false;
        if (EndObject != null)
            world.ShowObject(EndObject, EndObjectOn);
        // Another animation still running on the same image keeps it on screen.
        foreach (var other in world.Tasks.OfType<AnimTask>())
            if (other != this && other.Anim.Resource == Anim.Resource && other.WaitFor == null && other.Delay < 2 &&
                (other.Loops > 0 || other.Loops == -1))
                sprite.Visible = true;
        return true;
    }

    void Show(Sprite s)
    {
        var f = Anim.Def.Frames[Frame];
        s.Visible = true;
        if (s.Packed)
        {
            s.Frame = f.Frame;
        }
        else
        {
            s.SrcX = Anim.Def.Width * f.Frame;
            s.SrcY = 0;
            s.SrcW = Anim.Def.Width;
            s.SrcH = Anim.Def.Height;
        }
        s.X = Anim.X + f.X;
        s.Y = Anim.Y + f.Y;
        s.Scale = Anim.Scale;
    }
}

/// <summary>Ends a running animation of the given name when it runs.</summary>
public sealed class StopAnimTask : GameTask
{
    readonly string _anim;
    public StopAnimTask(string anim)
    {
        Name = "STOP_ANIM";
        _anim = anim;
    }

    protected internal override bool Run(GameWorld world)
    {
        world.StopAnim(_anim, onlyStarted: true);
        return true;
    }
}

/// <summary>Switches an object on or off when it runs.</summary>
public sealed class ObjectTask : GameTask
{
    readonly string _object;
    readonly bool _on;
    public ObjectTask(string obj, bool on)
    {
        Name = "OBJECT";
        _object = obj;
        _on = on;
    }

    protected internal override bool Run(GameWorld world)
    {
        world.ShowObject(_object, _on);
        return true;
    }
}

/// <summary>Plays a sound effect when it runs.</summary>
public sealed class SoundTask : GameTask
{
    readonly string _path;
    readonly int _volume, _pan;
    readonly bool _loop;
    public int Voice { get; private set; }

    public SoundTask(string path, int volume, int pan, bool loop)
    {
        Name = "SFX_TASK";
        _path = path;
        _volume = volume;
        _pan = pan;
        _loop = loop;
    }

    protected internal override bool Run(GameWorld world)
    {
        Voice = world.Audio.PlayEffect(_path, _volume, _pan, _loop);
        return true;
    }
}
