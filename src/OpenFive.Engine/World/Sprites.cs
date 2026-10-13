using OpenFive.Engine.Formats;
using OpenFive.Engine.Graphics;

namespace OpenFive.Engine.World;

/// <summary>
/// A drawable layer bound to one image resource. Objects and animations that use the same image
/// share its sprite: objects place it, animations change which frame (source rectangle) it shows.
/// </summary>
public sealed class Sprite
{
    public ResourceEntry? Resource;
    public bool Packed;
    public bool Visible;
    /// <summary>Larger depth = further away (drawn earlier).</summary>
    public int Depth;
    public int SrcX, SrcY, SrcW, SrcH;
    /// <summary>Frame of a packed sheet to show.</summary>
    public int Frame;
    public int X, Y;
    public double Scale = 1;
    public bool Mirror;
    /// <summary>Depth at which the background mask hides this sprite (actors), or null.</summary>
    public int? MaskDepth;

    /// <summary>A sprite owned by a character; its image changes with the character's animation.</summary>
    public Sprite() { }

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

    public SpriteBank(GameData data) => _data = data;

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

    public void Remove(IEnumerable<ResourceEntry> resources)
    {
        foreach (var r in resources)
            if (_byResource.Remove(r, out var s))
                _order.Remove(s);
    }

    /// <summary>Draws visible sprites (plus extra ones), furthest first; equal depths keep load order.</summary>
    public void Draw(Framebuffer fb, IEnumerable<Sprite>? extra = null, Func<Sprite, Func<int, int, bool>?>? occlusion = null)
    {
        var all = extra == null ? _order : _order.Concat(extra);
        foreach (var s in all.Where(s => s.Visible && s.Resource != null).OrderByDescending(s => s.Depth))
        {
            var hidden = occlusion?.Invoke(s);
            if (s.Packed)
            {
                var sheet = _data.Packed(s.Resource!.Path);
                int n = sheet.FrameWidth * sheet.FrameHeight;
                if (_frameBuffer.Length < n)
                    _frameBuffer = new byte[n];
                var buf = _frameBuffer.AsSpan(0, n).ToArray();
                int key = sheet.Decode(s.Frame, buf);
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
