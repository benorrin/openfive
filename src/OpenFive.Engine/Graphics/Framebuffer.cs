using OpenFive.Engine.Formats;

namespace OpenFive.Engine.Graphics;

/// <summary>The 800x600 RGB565 screen the game draws into.</summary>
public sealed class Framebuffer
{
    public const int Width = 800, Height = 600;
    public readonly ushort[] Pixels = new ushort[Width * Height];

    public void Clear(ushort colour = 0) => Array.Fill(Pixels, colour);

    public void CopyFrom(ushort[] src)
    {
        Array.Copy(src, Pixels, Math.Min(src.Length, Pixels.Length));
    }

    /// <summary>Draws an RGB565 image; pixels equal to <paramref name="key"/> are transparent.</summary>
    public void Blit(Image16 img, int dx, int dy, int key = -1, int sx = 0, int sy = 0, int w = -1, int h = -1)
    {
        if (w < 0) w = img.Width - sx;
        if (h < 0) h = img.Height - sy;
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

    public void FillRect(int x, int y, int w, int h, ushort colour)
    {
        for (int yy = Math.Max(0, y); yy < Math.Min(Height, y + h); yy++)
            for (int xx = Math.Max(0, x); xx < Math.Min(Width, x + w); xx++)
                Pixels[yy * Width + xx] = colour;
    }

    /// <summary>Darkens/blends a rectangle towards a colour (used for translucent text boxes).</summary>
    public void BlendRect(int x, int y, int w, int h, ushort colour, int alpha256)
    {
        int cr = colour >> 11, cg = (colour >> 5) & 63, cb = colour & 31;
        for (int yy = Math.Max(0, y); yy < Math.Min(Height, y + h); yy++)
            for (int xx = Math.Max(0, x); xx < Math.Min(Width, x + w); xx++)
            {
                ushort p = Pixels[yy * Width + xx];
                int r = p >> 11, g = (p >> 5) & 63, b = p & 31;
                r += (cr - r) * alpha256 >> 8;
                g += (cg - g) * alpha256 >> 8;
                b += (cb - b) * alpha256 >> 8;
                Pixels[yy * Width + xx] = (ushort)(r << 11 | g << 5 | b);
            }
    }

    public static ushort Rgb(int r, int g, int b) => (ushort)((r >> 3) << 11 | (g >> 2) << 5 | (b >> 3));
}
