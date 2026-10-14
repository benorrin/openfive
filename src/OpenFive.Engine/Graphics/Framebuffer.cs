using OpenFive.Engine.Formats;

namespace OpenFive.Engine.Graphics;

/// <summary>The 800x600 RGB565 screen the game draws into.</summary>
public sealed class Framebuffer
{
    /// <summary>The screen size the game was made for.</summary>
    public const int Width = 800, Height = 600;

    /// <summary>The pixels, row by row.</summary>
    public readonly ushort[] Pixels = new ushort[Width * Height];

    /// <summary>Fills the screen with one colour (black by default).</summary>
    public void Clear(ushort colour = 0) => Array.Fill(Pixels, colour);

    /// <summary>Draws an RGB565 image; pixels equal to <paramref name="key"/> are transparent.</summary>
    public void Blit(Image16 img, int dx, int dy, int key = -1, int sx = 0, int sy = 0, int w = -1, int h = -1)
    {
        if (w < 0)
            w = img.Width - sx;
        if (h < 0)
            h = img.Height - sy;
        for (int y = 0; y < h; y++)
        {
            int ty = dy + y, srcY = sy + y;
            if (ty < 0 || ty >= Height || srcY < 0 || srcY >= img.Height)
                continue;
            int row = ty * Width, srow = srcY * img.Width;
            for (int x = 0; x < w; x++)
            {
                int tx = dx + x, srcX = sx + x;
                if (tx < 0 || tx >= Width || srcX < 0 || srcX >= img.Width)
                    continue;
                ushort p = img.Pixels[srow + srcX];
                if (p != key)
                    Pixels[row + tx] = p;
            }
        }
    }

    /// <summary>Draws part of an RGB565 image scaled.</summary>
    public void BlitScaled(Image16 img, int dx, int dy, int key, int sx, int sy, int w, int h, double scale)
    {
        int ow = (int)Math.Round(w * scale), oh = (int)Math.Round(h * scale);
        for (int y = 0; y < oh; y++)
        {
            int ty = dy + y, srcY = sy + Math.Min(h - 1, (int)(y / scale));
            if (ty < 0 || ty >= Height || srcY < 0 || srcY >= img.Height)
                continue;
            for (int x = 0; x < ow; x++)
            {
                int tx = dx + x, srcX = sx + Math.Min(w - 1, (int)(x / scale));
                if (tx < 0 || tx >= Width || srcX < 0 || srcX >= img.Width)
                    continue;
                ushort p = img.Pixels[srcY * img.Width + srcX];
                if (p != key)
                    Pixels[ty * Width + tx] = p;
            }
        }
    }

    /// <summary>
    /// Draws palette-indexed pixels, optionally scaled, skipping the transparent index and any pixel
    /// hidden by the occlusion test (screen x, y) => true.
    /// </summary>
    public void BlitIndexed(byte[] idx, int w, int h, ushort[] palette, int transparent, int dx, int dy,
        double scale = 1.0, bool mirror = false, Func<int, int, bool>? hidden = null)
    {
        int ow = (int)Math.Round(w * scale), oh = (int)Math.Round(h * scale);
        if (ow <= 0 || oh <= 0)
            return;
        for (int y = 0; y < oh; y++)
        {
            int ty = dy + y;
            if (ty < 0 || ty >= Height)
                continue;
            int sy = Math.Min(h - 1, (int)(y / scale));
            for (int x = 0; x < ow; x++)
            {
                int tx = dx + x;
                if (tx < 0 || tx >= Width)
                    continue;
                int sx = Math.Min(w - 1, (int)(x / scale));
                if (mirror)
                    sx = w - 1 - sx;
                byte v = idx[sy * w + sx];
                if (v == transparent)
                    continue;
                if (hidden != null && hidden(tx, ty))
                    continue;
                Pixels[ty * Width + tx] = palette[v];
            }
        }
    }
}
