using OpenFive.Engine.Formats;

namespace OpenFive.Engine.World;

/// <summary>An animation definition together with the resource it draws from.</summary>
public sealed class AnimInfo
{
    /// <summary>The animation as described in the ".res" file.</summary>
    public required AnimDef Def;

    /// <summary>The picture it draws from.</summary>
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
    /// <summary>The animation played.</summary>
    public readonly AnimInfo Anim;

    /// <summary>Times still to play (-1: forever).</summary>
    public int Loops;

    /// <summary>The frame shown.</summary>
    public int Frame;

    /// <summary>Object to switch when the animation ends (null = none).</summary>
    public string? EndObject;

    /// <summary>Whether <see cref="EndObject"/> is switched on (true) or off.</summary>
    public bool EndObjectOn;

    /// <summary>Creates the task that plays <paramref name="anim"/> <paramref name="loops"/> times.</summary>
    public AnimTask(AnimInfo anim, int loops)
    {
        Anim = anim;
        Name = anim.Def.Name;
        Loops = loops;
    }

    /// <summary>Shows the next frame; after the last one, loops or ends.</summary>
    protected internal override bool Run(GameWorld world)
    {
        var sprite = world.Sprites.For(Anim.Resource);
        int n = Anim.Def.Frames.Length;
        if (sprite == null || n == 0)
            return true;
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

    /// <summary>Points the sprite at the current frame, at the animation's position and size.</summary>
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
