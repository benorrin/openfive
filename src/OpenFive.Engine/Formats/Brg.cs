namespace OpenFive.Engine.Formats;

/// <summary>An RGB565 image (the game's native pixel format).</summary>
public sealed class Image16
{
    public int Width { get; }
    public int Height { get; }
    public ushort[] Pixels { get; }

    public Image16(int width, int height, ushort[]? pixels = null)
    {
        Width = width;
        Height = height;
        Pixels = pixels ?? new ushort[width * height];
    }

    public ushort this[int x, int y] => Pixels[y * Width + x];
}

/// <summary>
/// An 8-bit "PACKED" sprite sheet: a 256-colour RGB565 palette and run-length coded frames.
/// Used for character animations and many object animations.
/// </summary>
public sealed class PackedSheet
{
    public int SheetWidth, SheetHeight, FrameWidth, FrameHeight;
    public ushort[] Palette = new ushort[256];
    public uint[] FrameEnd = Array.Empty<uint>();
    public byte[] Data = Array.Empty<byte>();
    public int Frames => FrameEnd.Length;

    /// <summary>
    /// Decodes frame <paramref name="n"/> into palette indices (FrameWidth*FrameHeight).
    /// Returns the transparent index, or -1 if the frame is opaque.
    /// </summary>
    public int Decode(int n, byte[] output)
    {
        Array.Fill(output, (byte)0xFF);
        if (n < 0 || n >= Frames)
            n = 0;
        if (Frames == 0)
            return 0xFF;
        uint start = n > 0 ? FrameEnd[n - 1] : 0;
        uint end = Math.Min(FrameEnd[n], (uint)Data.Length);
        if (start + 4 > end)
            return 0xFF;
        int key = Data[start + 1] == 0xFF ? Data[start] : -1;
        long remain = (long)(end - start) - 1;
        int o = 0, total = output.Length;
        uint p = start;
        while (remain >= 0 && p + 4 <= end && o < total)
        {
            byte b0 = Data[p], b1 = Data[p + 1], b2 = Data[p + 2], b3 = Data[p + 3];
            p += 4;
            remain -= 4;
            if (b1 != 0xFF)
            {
                output[o++] = b0;
                if (o < total) output[o++] = b1;
                if (o < total) output[o++] = b2;
                if (o < total) output[o++] = b3;
            }
            else
            {
                int run = (b2 | b3 << 8) * 4;
                for (int k = 0; k < run && o < total; k++)
                    output[o++] = b0;
            }
        }
        return key;
    }
}

/// <summary>Reader for the game's ".brg" bitmap files.</summary>
public static class Brg
{
    readonly record struct Header(int HeaderSize, int W, int H, int A, int B, int Type);

    static Header ReadHeader(byte[] d)
    {
        if (d.Length < 24 || d[0] != 'B' || d[1] != 'R')
            throw new InvalidDataException("not a BRG image");
        return new Header(BitConverter.ToInt32(d, 2), BitConverter.ToInt32(d, 6), BitConverter.ToInt32(d, 10),
            BitConverter.ToInt32(d, 14), BitConverter.ToInt32(d, 18), BitConverter.ToUInt16(d, 22));
    }

    /// <summary>True if the file holds a palettised, run-length coded sprite sheet.</summary>
    public static bool IsPacked(byte[] d) => ReadHeader(d).Type != 0;

    public static Image16 LoadRaw(byte[] d)
    {
        var h = ReadHeader(d);
        if (h.Type != 0)
            throw new InvalidDataException("BRG is packed, not raw");
        var img = new Image16(h.W, h.H);
        int avail = Math.Max(0, (d.Length - h.HeaderSize) / 2);
        Buffer.BlockCopy(d, h.HeaderSize, img.Pixels, 0, Math.Min(img.Pixels.Length, avail) * 2);
        return img;
    }

    public static PackedSheet LoadPacked(byte[] d)
    {
        var h = ReadHeader(d);
        if (h.Type == 0)
            throw new InvalidDataException("BRG is raw, not packed");
        var s = new PackedSheet { SheetWidth = h.W, SheetHeight = h.H, FrameWidth = h.A, FrameHeight = h.B };
        int pos = 24;
        for (int i = 0; i < 256; i++, pos += 2)
            s.Palette[i] = BitConverter.ToUInt16(d, pos);
        pos += 4; // declared data size (unreliable)
        int n = BitConverter.ToInt32(d, pos);
        pos += 4;
        if (n < 0 || n > 10000)
            throw new InvalidDataException("bad frame count");
        s.FrameEnd = new uint[n];
        for (int i = 0; i < n; i++, pos += 4)
            s.FrameEnd[i] = BitConverter.ToUInt32(d, pos);
        s.Data = d[Math.Min(pos, d.Length)..];
        return s;
    }

    /// <summary>Loads an 8-bit depth mask (one region id per pixel) used to hide actors behind scenery.</summary>
    public static (byte[] mask, int w, int h) LoadMask(byte[] d)
    {
        var h = ReadHeader(d);
        int pos = h.HeaderSize;
        int packed = BitConverter.ToInt32(d, pos);
        pos += 4;
        int end = Math.Min(d.Length, pos + packed);
        var mask = new byte[h.W * h.H];
        int o = 0;
        for (int i = pos; i < end && o < mask.Length;)
        {
            byte c = d[i];
            if (c == 0xFF && i + 2 < end)
            {
                byte v = d[i + 1];
                int cnt = d[i + 2];
                for (int k = 0; k < cnt && o < mask.Length; k++)
                    mask[o++] = v;
                i += 3;
            }
            else
            {
                mask[o++] = c;
                i++;
            }
        }
        return (mask, h.W, h.H);
    }
}
