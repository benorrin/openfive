using OpenFive.Engine.Formats;
using OpenFive.Engine.Graphics;

namespace OpenFive.Engine.World;

/// <summary>
/// A loaded location: its animated background, depth mask, walkable shapes and objects.
/// </summary>
public sealed class Scene
{
    readonly GameData _data;
    readonly Smacker? _background;
    readonly ushort[] _backgroundPixels = new ushort[Framebuffer.Width * Framebuffer.Height];
    double _backgroundClock;

    public string Name { get; }
    public ResScript Script { get; }
    public byte[]? Mask { get; }
    public Dictionary<string, bool> ObjectVisible { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Scene(GameData data, string name)
    {
        _data = data;
        Name = name;
        Script = data.Location(name);
        if (Script.Background >= 0 && Script.Background < Script.Resources.Count)
        {
            var bg = Script.Resources[Script.Background];
            if (bg.Kind == ResourceKind.Smk)
            {
                _background = data.Video(bg.Path);
                _background.NextFrame();
                _background.ToRgb565(_backgroundPixels);
            }
        }
        if (Script.MaskPath != null && data.Exists(Script.MaskPath))
            Mask = Brg.LoadMask(data.Read(Script.MaskPath)).mask;
        foreach (var o in Script.Objects)
            ObjectVisible[o.Name] = o.On;
    }

    /// <summary>Advances the background animation by <paramref name="ms"/> milliseconds.</summary>
    public void Update(double ms)
    {
        if (_background == null || _background.Frames <= 1)
            return;
        _backgroundClock += ms;
        while (_backgroundClock >= _background.FrameDelay)
        {
            _backgroundClock -= _background.FrameDelay;
            _background.NextFrame();
            _background.ToRgb565(_backgroundPixels);
        }
    }

    /// <summary>
    /// Draws the sky (a fixed 800x400 picture) and the background over it; pure green pixels of the
    /// background are transparent so the sky shows through.
    /// </summary>
    public void DrawBackground(Framebuffer fb)
    {
        fb.Clear();
        if (_data.Exists(SkyPath))
            fb.Blit(_data.Image(SkyPath), 0, 0, -1, 0, 0, Framebuffer.Width, 400);
        var px = fb.Pixels;
        for (int i = 0; i < px.Length; i++)
        {
            ushort p = _backgroundPixels[i];
            if (p != ChromaKey)
                px[i] = p;
        }
    }

    const string SkyPath = "bmp/sky/skyback.brg";
    const ushort ChromaKey = 0x07E0;

    /// <summary>Draws the visible objects that have graphics, furthest (largest depth) first.</summary>
    public void DrawObjects(Framebuffer fb)
    {
        foreach (var o in Script.Objects.Where(o => o.Gfx >= 0 && IsVisible(o)).OrderByDescending(o => o.Depth))
            DrawGfx(fb, o.Gfx, o.GfxX, o.GfxY);
    }

    public void DrawGfx(Framebuffer fb, int resource, int x, int y, int frame = 0)
    {
        if (resource < 0 || resource >= Script.Resources.Count)
            return;
        var r = Script.Resources[resource];
        if (r.Kind != ResourceKind.Gfx || !_data.Exists(r.Path))
            return;
        if (r.Packed || _data.IsPacked(r.Path))
        {
            var sheet = _data.Packed(r.Path);
            var buf = new byte[sheet.FrameWidth * sheet.FrameHeight];
            int key = sheet.Decode(frame, buf);
            fb.BlitIndexed(buf, sheet.FrameWidth, sheet.FrameHeight, sheet.Palette, key, x, y);
        }
        else
        {
            var img = _data.Image(r.Path);
            fb.Blit(img, x, y, img.Pixels.Length > 0 ? img.Pixels[0] : -1);
        }
    }

    public bool IsVisible(ObjectDef o) => ObjectVisible.TryGetValue(o.Name, out bool v) ? v : o.On;

    /// <summary>The visible, clickable object under (x, y), preferring the nearest one.</summary>
    public ObjectDef? ObjectAt(int x, int y)
    {
        ObjectDef? best = null;
        foreach (var o in Script.Objects)
        {
            if (!IsVisible(o) || o.Click.Length < 3 || !Geometry.Contains(o.Click, x, y))
                continue;
            if (best == null || o.Depth < best.Depth)
                best = o;
        }
        return best;
    }
}

public static class Geometry
{
    /// <summary>Even-odd point-in-polygon test.</summary>
    public static bool Contains((int X, int Y)[] poly, int x, int y)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            var a = poly[i];
            var b = poly[j];
            if ((a.Y > y) != (b.Y > y) && x < (double)(b.X - a.X) * (y - a.Y) / (b.Y - a.Y) + a.X)
                inside = !inside;
        }
        return inside;
    }
}
