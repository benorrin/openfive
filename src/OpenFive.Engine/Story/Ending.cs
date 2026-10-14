using OpenFive.Engine.Game;
using OpenFive.Engine.World;

namespace OpenFive.Engine.Story;

/// <summary>The end of the game: the credits scroll up; a click ends the game.</summary>
public sealed class Ending : StoryScript
{
    const int ScrollSpeed = 2;

    /// <summary>How far the credits have scrolled (pixels).</summary>
    int _scroll;

    /// <summary>The credits have scrolled to their end.</summary>
    bool _done;

    /// <inheritdoc/>
    public override string Location => "locextro";

    /// <summary>A click on the credits ends the game.</summary>
    public override bool Click(string name)
    {
        if (Same(name, "LOCEXTROClick"))
            Session.QuitRequested = true;
        return true;
    }

    /// <summary>Shows the credits of the game's language, without the pointer at first.</summary>
    protected override void Setup()
    {
        PointerOff();
        _scroll = 0;
        _done = false;
        Obj("LOCEXTROScrollE", !German);
        Obj("LOCEXTROScrollD", German);
        ObjAfter("COMMON_SYMBOL_D", false, null, 1);
        ObjAfter("COMMON_SYMBOL_E", false, null, 1);
        ShowScroll();
    }

    /// <summary>Nobody is shown.</summary>
    protected override void Arrive() { }

    /// <summary>Shows the credits pictures from the scroll position down.</summary>
    void ShowScroll()
    {
        foreach (string name in new[] { "LOCEXTROScrollE", "LOCEXTROScrollD" })
        {
            var o = World.FindObject(name);
            if (o == null || o.Gfx < 0 || World.Scene == null)
                continue;
            if (World.Sprites.For(World.Scene.Script.Resources[o.Gfx]) is { } s)
            {
                int height = World.Data.Image(s.Resource!.Path).Height;
                s.SrcY = _scroll;
                s.SrcH = Math.Max(0, height - _scroll);
            }
        }
    }

    /// <summary>Scrolls the credits up until their end; the pointer comes back early on.</summary>
    public override void EveryTick()
    {
        if (_done)
            return;
        int length = German ? 2700 : 2450;
        if (_scroll > length)
        {
            _done = true;
            return;
        }
        // The pointer comes back after a while so the player can leave.
        if (_scroll == 100)
        {
            PointerOn();
            World.CursorWarp = (400, 300);
        }
        _scroll += ScrollSpeed;
        ShowScroll();
    }
}
