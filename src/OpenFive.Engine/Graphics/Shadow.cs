namespace OpenFive.Engine.Graphics;

/// <summary>
/// Character shadows: the character's current frame, turned by the light's angle and squashed to
/// half its height, darkens the background below it. No location of the game defines a light,
/// so every shadow falls at the same angle and with the same darkness.
/// </summary>
public static class Shadow
{
    /// <summary>Angle of the shadow: the default light (a quarter turn) less a quarter turn.</summary>
    const float Angle = (float)(Math.PI / 4) + (float)(-Math.PI / 2);
    /// <summary>The shadow is half as high as it is wide.</summary>
    const float Squash = 2f;
    /// <summary>Darkness 0..31 (0: darken by 1/32, 31: black); the default light gives 7.</summary>
    const int Level = (0 + 50) * 31 / 200 & 31;

    /// <summary>darken[c * 32 + level]: a 5-bit colour channel c darkened by (level + 1) / 32.</summary>
    static readonly byte[] Darken = BuildDarken();

    static byte[] BuildDarken()
    {
        var t = new byte[32 * 32];
        for (int c = 0; c < 32; c++)
        {
            int v = c, acc = 16;
            for (int k = 0; k < 32; k++)
            {
                acc += c;
                if (acc > 31)
                {
                    acc -= 32;
                    v--;
                }
                t[c * 32 + k] = (byte)v;
            }
        }
        return t;
    }

    static ushort DarkenPixel(ushort p)
    {
        int r = Darken[(p >> 11) * 32 + Level];
        int g = Darken[((p & 0x7c0) >> 6) * 32 + Level];
        int b = Darken[(p & 0x1f) * 32 + Level];
        return (ushort)(r << 11 | g << 6 | b);
    }

    /// <summary>
    /// Draws the shadow of a palette-indexed frame (w x h) that is shown at (dx, dy) with the given
    /// scale. Pixels for which <paramref name="hidden"/> is true stay as they are (scenery in front).
    /// </summary>
    public static void Draw(Framebuffer fb, byte[] idx, int w, int h, int transparent, int dx, int dy,
        double scale, bool mirror, Func<int, int, bool>? hidden)
    {
        int dw = (int)Math.Round(w * scale), dh = (int)Math.Round(h * scale);
        if (dw <= 0 || dh <= 0)
            return;
        int largest = Math.Max(w, h);
        int diag = (int)Math.Sqrt((double)dh * dh + (double)dw * dw);
        int x0 = dx - ((diag - dw) >> 1);
        int y0 = dy - ((diag - dh) >> 1);
        float ratio = (float)w / dw;
        // The foot of the shadow is raised by 2% of the frame's height.
        int foot = (int)((h * 2 / 100) / ratio);
        int c = (int)((uint)(dh - foot) / Squash) + (diag - dh) / 2 - diag / 2;
        x0 += (int)(-c * Math.Sin(Angle));
        y0 -= (int)(c * Math.Cos(Angle) / Squash);
        int t = dh - foot + (diag - dh) / 2;
        y0 = (t - (int)(t / Squash)) / 2 + y0 + (t - diag / 2);

        // Source position in 16.16 fixed point, stepped per screen pixel and per screen row.
        int stepUy = (int)(ratio * Math.Sin(-Angle) * 65536.0);
        int stepUx = (int)(ratio * Math.Cos(-Angle) * 65536.0);
        int rowU = (int)(stepUy * Squash), rowV = (int)(stepUx * Squash);
        int u0 = (largest >> 1) * 65536 - (diag >> 1) * (stepUx + stepUy);
        int v0 = (largest >> 1) * 65536 - (diag >> 1) * (stepUx - stepUy);
        int xEnd = Math.Min(diag + x0, Framebuffer.Width);
        int yEnd = Math.Min((int)(diag / Squash) + y0, Framebuffer.Height);
        stepUy = (int)(stepUy * Squash);
        rowV = (int)(rowV * Squash);
        v0 = (int)(v0 * Squash);
        int centre = (largest - w) >> 1;

        // As in the original, row n of the shadow lands on screen row y0 + n - 1 (row 0 on y0).
        int screenY = y0;
        for (int y = y0; y <= yEnd && y >= 0; y++)
        {
            int u = u0, v = v0;
            for (int x = x0; x < xEnd; x++, u += stepUx, v -= stepUy)
            {
                if (x < 0 || screenY >= Framebuffer.Height)
                    continue;
                int sx = (u >> 16) - centre, sy = v >> 16;
                if (sx < 0 || sx >= w || sy < 0 || sy >= h)
                    continue;
                if (mirror)
                    sx = w - 1 - sx;
                if (idx[sy * w + sx] == transparent || (hidden != null && hidden(x, screenY)))
                    continue;
                int i = screenY * Framebuffer.Width + x;
                fb.Pixels[i] = DarkenPixel(fb.Pixels[i]);
               
            }
            screenY = y;
            u0 += rowU;
            v0 += rowV;
        }
    }
}
