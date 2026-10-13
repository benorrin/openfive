namespace OpenFive.Engine.Graphics;

/// <summary>Writes the framebuffer as a 24-bit BMP (used for screenshots and automated tests).</summary>
public static class Screenshot
{
    public static void SaveBmp(Framebuffer fb, string path)
    {
        const int w = Framebuffer.Width, h = Framebuffer.Height;
        int rowSize = w * 3;
        int dataSize = rowSize * h;
        using var f = new BinaryWriter(File.Create(path));
        f.Write((byte)'B');
        f.Write((byte)'M');
        f.Write(54 + dataSize);
        f.Write(0);
        f.Write(54);
        f.Write(40);
        f.Write(w);
        f.Write(h);
        f.Write((short)1);
        f.Write((short)24);
        f.Write(0);
        f.Write(dataSize);
        f.Write(2835);
        f.Write(2835);
        f.Write(0);
        f.Write(0);
        var row = new byte[rowSize];
        for (int y = h - 1; y >= 0; y--)
        {
            for (int x = 0; x < w; x++)
            {
                ushort p = fb.Pixels[y * w + x];
                row[x * 3] = (byte)((p & 31) << 3);
                row[x * 3 + 1] = (byte)(((p >> 5) & 63) << 2);
                row[x * 3 + 2] = (byte)((p >> 11) << 3);
            }
            f.Write(row);
        }
    }
}
