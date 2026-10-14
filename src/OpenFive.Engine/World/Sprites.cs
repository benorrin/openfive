using OpenFive.Engine.Formats;
using OpenFive.Engine.Graphics;

namespace OpenFive.Engine.World;

/// <summary>
/// A drawable layer bound to one image resource. Objects and animations that use the same image
/// share its sprite: objects place it, animations change which frame (source rectangle) it shows.
/// </summary>
public sealed class Sprite
{
    /// <summary>The picture shown (null: nothing yet).</summary>
    public ResourceEntry? Resource;

    /// <summary>The picture is a packed sprite sheet (frames by number) rather than a raw picture.</summary>
    public bool Packed;

    /// <summary>Is the sprite drawn?</summary>
    public bool Visible;

    /// <summary>Larger depth = further away (drawn earlier).</summary>
    public int Depth;

    /// <summary>The part of a raw picture shown.</summary>
    public int SrcX, SrcY, SrcW, SrcH;

    /// <summary>Frame of a packed sheet to show.</summary>
    public int Frame;

    /// <summary>Top left corner on screen.</summary>
    public int X, Y;

    /// <summary>Drawing scale.</summary>
    public double Scale = 1;

    /// <summary>Drawn mirrored left to right.</summary>
    public bool Mirror;
    /// <summary>Depth at which the background mask hides this sprite (actors), or null.</summary>
    public int? MaskDepth;

    /// <summary>A sprite owned by a character; its image changes with the character's animation.</summary>
    public Sprite() { }

    /// <summary>
    /// A sprite for an object or animation picture of <paramref name="width"/> by
    /// <paramref name="height"/>; packed sheets start far back (depth 8000).
    /// </summary>
    public Sprite(ResourceEntry resource, bool packed, int width, int height)
    {
        Resource = resource;
        Packed = packed;
        Depth = packed ? 8000 : 0;
        SrcW = width;
        SrcH = height;
    }
}

/// <summary>The sprites of the common resources and of the current location.</summary>
public sealed class SpriteBank
{
    readonly GameData _data;
    readonly Dictionary<ResourceEntry, Sprite> _byResource = new();
    readonly List<Sprite> _order = new();
    byte[] _frameBuffer = Array.Empty<byte>();

    /// <summary>Creates an empty bank that loads pictures from <paramref name="data"/>.</summary>
    public SpriteBank(GameData data) => _data = data;

    /// <summary>The sprites, in the order they were created.</summary>
    public IReadOnlyList<Sprite> All => _order;

    /// <summary>Gets (creating on first use) the sprite for a resource; null if it is not an image.</summary>
    public Sprite? For(ResourceEntry r)
    {
        if (_byResource.TryGetValue(r, out var s))
            return s;
        if (r.Kind != ResourceKind.Gfx || !_data.Exists(r.Path))
            return null;
        bool packed = r.Packed || _data.IsPacked(r.Path);
        int w, h;
        if (packed)
        {
            var sheet = _data.Packed(r.Path);
            (w, h) = (sheet.FrameWidth, sheet.FrameHeight);
        }
        else
        {
            var img = _data.Image(r.Path);
            (w, h) = (img.Width, img.Height);
        }
        s = new Sprite(r, packed, w, h);
        _byResource[r] = s;
        _order.Add(s);
        return s;
    }

    /// <summary>Finds the sprite of a loaded resource whose path contains <paramref name="pathPart"/>.</summary>
    public Sprite? FindByPath(string pathPart)
    {
        string needle = pathPart.Replace('/', '\\');
        foreach (var s in _order)
            if (s.Resource != null && s.Resource.Path.Replace('/', '\\').Contains(needle, StringComparison.OrdinalIgnoreCase))
                return s;
        return null;
    }

    /// <summary>Drops the sprites of resources (a location being unloaded).</summary>
    public void Remove(IEnumerable<ResourceEntry> resources)
    {
        foreach (var r in resources)
            if (_byResource.Remove(r, out var s))
                _order.Remove(s);
    }

    /// <summary>Draws visible sprites (plus extra ones), furthest first; equal depths keep load order.</summary>
    /// <param name="shadowed">Sprites that cast a shadow (drawn just before them), or null.</param>
    public void Draw(Framebuffer fb, IEnumerable<Sprite>? extra = null, Func<Sprite, Func<int, int, bool>?>? occlusion = null,
        Func<Sprite, bool>? shadowed = null)
    {
        var all = extra == null ? _order : _order.Concat(extra);
        foreach (var s in all.Where(s => s.Visible && s.Resource != null).OrderByDescending(s => s.Depth))
        {
            var hidden = occlusion?.Invoke(s);
            if (s.Packed)
            {
                var sheet = _data.Packed(s.Resource!.Path);
                int n = sheet.FrameWidth * sheet.FrameHeight;
                // Decode fills the whole buffer, so it must be exactly one frame long.
                if (_frameBuffer.Length != n)
                    _frameBuffer = new byte[n];
                var buf = _frameBuffer;
                int key = sheet.Decode(s.Frame, buf);
                if (shadowed != null && shadowed(s))
                    Shadow.Draw(fb, buf, sheet.FrameWidth, sheet.FrameHeight, key, s.X, s.Y, s.Scale, s.Mirror, hidden);
                fb.BlitIndexed(buf, sheet.FrameWidth, sheet.FrameHeight, sheet.Palette, key, s.X, s.Y, s.Scale, s.Mirror, hidden);
            }
            else
            {
                var img = _data.Image(s.Resource!.Path);
                int key = img.Pixels.Length > 0 ? img.Pixels[0] : -1;
                if (Math.Abs(s.Scale - 1) < 0.001)
                    fb.Blit(img, s.X, s.Y, key, s.SrcX, s.SrcY, s.SrcW, s.SrcH);
                else
                    fb.BlitScaled(img, s.X, s.Y, key, s.SrcX, s.SrcY, s.SrcW, s.SrcH, s.Scale);
            }
        }
    }
}
