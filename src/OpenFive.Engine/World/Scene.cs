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

    const string SkyPath = "bmp/sky/skyback.brg";

    /// <summary>Background pixels of this colour (pure green) are transparent: the sky shows through.</summary>
    const ushort ChromaKey = 0x07E0;

    /// <summary>The location's name ("loc05").</summary>
    public string Name { get; }

    /// <summary>The location's ".res" description.</summary>
    public ResScript Script { get; }

    /// <summary>The depth mask: a region number per screen pixel (null if the location has none).</summary>
    public byte[]? Mask { get; }

    /// <summary>The on/off state of the location's objects.</summary>
    public Dictionary<string, bool> ObjectVisible { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Loads location <paramref name="name"/>: description, background movie, mask, object states.</summary>
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

    /// <summary>Is the object switched on (its state in this location, else its default)?</summary>
    public bool IsVisible(ObjectDef o) => ObjectVisible.TryGetValue(o.Name, out bool v) ? v : o.On;
}
