using OpenFive.Engine.Game;
using OpenFive.Engine.World;

namespace OpenFive.Engine.Story;

/// <summary>The end of the game: the credits scroll up; a click ends the game.</summary>
public sealed class Ending : StoryScript
{
    public override string Location => "locextro";

    int _scroll;
    bool _done;

    public override bool Click(string name)
    {
        if (Same(name, "LOCEXTROClick"))
            Session.QuitRequested = true;
        return true;
    }

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

    protected override void Arrive() { }

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
        _scroll += 2;
        ShowScroll();
    }
}
