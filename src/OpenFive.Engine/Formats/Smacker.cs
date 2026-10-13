namespace OpenFive.Engine.Formats;

/// <summary>
/// Smacker (SMK2/SMK4) video decoder, video track only. The game uses Smacker files for its
/// intro/outro movies and for the location backgrounds (1-10 frame loops).
/// </summary>
public sealed class Smacker
{
    static readonly byte[] PalMap =
    {
        0x00, 0x04, 0x08, 0x0C, 0x10, 0x14, 0x18, 0x1C, 0x20, 0x24, 0x28, 0x2C, 0x30, 0x34, 0x38, 0x3C,
        0x41, 0x45, 0x49, 0x4D, 0x51, 0x55, 0x59, 0x5D, 0x61, 0x65, 0x69, 0x6D, 0x71, 0x75, 0x79, 0x7D,
        0x82, 0x86, 0x8A, 0x8E, 0x92, 0x96, 0x9A, 0x9E, 0xA2, 0xA6, 0xAA, 0xAE, 0xB2, 0xB6, 0xBA, 0xBE,
        0xC3, 0xC7, 0xCB, 0xCF, 0xD3, 0xD7, 0xDB, 0xDF, 0xE3, 0xE7, 0xEB, 0xEF, 0xF3, 0xF7, 0xFB, 0xFF,
    };

    static readonly int[] BlockRuns =
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
        33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 128, 256,
        512, 1024, 2048,
    };

    const int Node = unchecked((int)0x80000000);

    sealed class BigTree
    {
        public List<int> Values = new();
        public int[] Last = { 0, 0, 0 };
    }

    ref struct BitReader
    {
        readonly ReadOnlySpan<byte> _d;
        long _bit;
        public BitReader(ReadOnlySpan<byte> d) { _d = d; _bit = 0; }

        public int Bit()
        {
            if ((_bit >> 3) >= _d.Length)
                return 0;
            int v = (_d[(int)(_bit >> 3)] >> (int)(_bit & 7)) & 1;
            _bit++;
            return v;
        }

        public int Bits(int n)
        {
            int v = 0;
            for (int i = 0; i < n; i++)
                v |= Bit() << i;
            return v;
        }
    }

    sealed class Tree8
    {
        public readonly List<int> Nodes = new();
        public bool Present;

        public void Build(ref BitReader br, int depth = 0)
        {
            if (depth > 32)
                return;
            if (br.Bit() == 0)
            {
                Nodes.Add(br.Bits(8));
                return;
            }
            int self = Nodes.Count;
            Nodes.Add(Node);
            Build(ref br, depth + 1);
            Nodes[self] = Node | Nodes.Count;
            Build(ref br, depth + 1);
        }

        public int Decode(ref BitReader br)
        {
            if (!Present || Nodes.Count == 0)
                return 0;
            int i = 0;
            while ((Nodes[i] & Node) != 0)
            {
                i = br.Bit() != 0 ? Nodes[i] & ~Node : i + 1;
                if (i >= Nodes.Count)
                    return 0;
            }
            return Nodes[i];
        }
    }

    readonly byte[] _file;
    readonly bool _v4;
    readonly uint[] _frameSize;
    readonly byte[] _frameType;
    readonly long[] _frameOffset;
    readonly BigTree _mmap = new(), _mclr = new(), _full = new(), _type = new();

    public int Width { get; }
    public int Height { get; }
    public int Frames { get; }
    public int CurrentFrame { get; private set; } = -1;
    /// <summary>Milliseconds per frame.</summary>
    public double FrameDelay { get; }
    /// <summary>Palette indices of the current frame.</summary>
    public byte[] Pixels { get; }
    /// <summary>Current palette, 256 * RGB888.</summary>
    public byte[] Palette { get; } = new byte[768];

    public Smacker(byte[] file)
    {
        _file = file;
        if (file.Length < 104 || file[0] != 'S' || file[1] != 'M' || file[2] != 'K' || (file[3] != '2' && file[3] != '4'))
            throw new InvalidDataException("not a Smacker file");
        _v4 = file[3] == '4';
        Width = BitConverter.ToInt32(file, 4);
        Height = BitConverter.ToInt32(file, 8);
        int frames = BitConverter.ToInt32(file, 12);
        int rate = BitConverter.ToInt32(file, 16);
        uint flags = BitConverter.ToUInt32(file, 20);
        if ((flags & 1) != 0)
            frames++; // ring frame
        Frames = frames;
        FrameDelay = rate > 0 ? rate : rate < 0 ? -rate / 100.0 : 100.0;
        uint treeSize = BitConverter.ToUInt32(file, 52);
        long pos = 104;
        if (pos + (long)frames * 5 + treeSize > file.Length)
            throw new InvalidDataException("truncated Smacker file");
        _frameSize = new uint[frames];
        _frameType = new byte[frames];
        for (int i = 0; i < frames; i++)
            _frameSize[i] = BitConverter.ToUInt32(file, (int)(pos + i * 4)) & ~3u;
        pos += frames * 4L;
        for (int i = 0; i < frames; i++)
            _frameType[i] = file[pos + i];
        pos += frames;
        ReadTrees(file.AsSpan((int)pos, (int)treeSize));
        pos += treeSize;
        _frameOffset = new long[frames];
        for (int i = 0; i < frames; i++)
        {
            _frameOffset[i] = pos;
            pos += _frameSize[i];
        }
        Pixels = new byte[Width * Height];
    }

    void ReadTrees(ReadOnlySpan<byte> data)
    {
        var br = new BitReader(data);
        foreach (var bt in new[] { _mmap, _mclr, _full, _type })
        {
            bt.Values.Clear();
            if (br.Bit() == 0)
            {
                bt.Values.AddRange(new[] { 0, 0 });
                bt.Last = new[] { 1, 1, 1 };
                continue;
            }
            var lo = new Tree8();
            var hi = new Tree8();
            if (br.Bit() != 0)
            {
                lo.Present = true;
                lo.Build(ref br);
                br.Bit();
            }
            if (br.Bit() != 0)
            {
                hi.Present = true;
                hi.Build(ref br);
                br.Bit();
            }
            int[] esc = { br.Bits(16), br.Bits(16), br.Bits(16) };
            int[] last = { -1, -1, -1 };
            BuildBig(ref br, lo, hi, esc, last, bt.Values, 0);
            br.Bit();
            for (int k = 0; k < 3; k++)
            {
                if (last[k] == -1)
                {
                    last[k] = bt.Values.Count;
                    bt.Values.Add(0);
                }
            }
            bt.Last = last;
        }
    }

    static int BuildBig(ref BitReader br, Tree8 lo, Tree8 hi, int[] esc, int[] last, List<int> v, int depth)
    {
        if (depth > 64)
            return 0;
        if (br.Bit() == 0)
        {
            int val = lo.Decode(ref br) | hi.Decode(ref br) << 8;
            for (int k = 0; k < 3; k++)
            {
                if (val == esc[k])
                {
                    last[k] = v.Count;
                    val = 0;
                    break;
                }
            }
            v.Add(val);
            return 1;
        }
        int self = v.Count;
        v.Add(Node);
        int r = BuildBig(ref br, lo, hi, esc, last, v, depth + 1);
        v[self] = Node | r;
        r++;
        return r + BuildBig(ref br, lo, hi, esc, last, v, depth + 1);
    }

    public void Rewind()
    {
        CurrentFrame = -1;
        Array.Clear(Pixels);
    }

    /// <summary>Decodes the next frame (wrapping after the last). Returns false on corrupt data.</summary>
    public bool NextFrame()
    {
        if (Frames == 0)
            return false;
        int f = CurrentFrame + 1;
        if (f >= Frames)
            f = 0;
        CurrentFrame = f;
        long pos = _frameOffset[f], end = pos + _frameSize[f];
        if (end > _file.Length)
            return false;
        if ((_frameType[f] & 1) != 0)
        {
            var old = (byte[])Palette.Clone();
            long chunk = _file[pos] * 4L;
            long p = pos + 1, pend = pos + chunk;
            int sz = 0, o = 0;
            while (sz < 256 && p < pend)
            {
                int t = _file[p++];
                if ((t & 0x80) != 0)
                {
                    sz += (t & 0x7F) + 1;
                    o += ((t & 0x7F) + 1) * 3;
                }
                else if ((t & 0x40) != 0)
                {
                    int off = _file[p++];
                    int j = (t & 0x3F) + 1;
                    if (off + j > 256)
                        break;
                    off *= 3;
                    while (j-- > 0 && sz < 256)
                    {
                        Palette[o++] = old[off];
                        Palette[o++] = old[off + 1];
                        Palette[o++] = old[off + 2];
                        sz++;
                        off += 3;
                    }
                }
                else
                {
                    Palette[o++] = PalMap[t & 0x3F];
                    Palette[o++] = PalMap[_file[p++] & 0x3F];
                    Palette[o++] = PalMap[_file[p++] & 0x3F];
                    sz++;
                }
            }
            pos += chunk;
        }
        for (int track = 0; track < 7; track++)
        {
            if ((_frameType[f] & (2 << track)) != 0)
            {
                if (pos + 4 > end)
                    return false;
                pos += BitConverter.ToUInt32(_file, (int)pos);
            }
        }
        if (pos < end)
            DecodeVideo(_file.AsSpan((int)pos, (int)(end - pos)));
        return true;
    }

    int Get(ref BitReader br, BigTree t)
    {
        var v = t.Values;
        int i = 0;
        while (i < v.Count && (v[i] & Node) != 0)
        {
            if (br.Bit() != 0)
                i += v[i] & ~Node;
            i++;
        }
        if (i >= v.Count)
            return 0;
        int val = v[i];
        if (val != v[t.Last[0]])
        {
            v[t.Last[2]] = v[t.Last[1]];
            v[t.Last[1]] = v[t.Last[0]];
            v[t.Last[0]] = val;
        }
        return val;
    }

    void DecodeVideo(ReadOnlySpan<byte> data)
    {
        var br = new BitReader(data);
        foreach (var t in new[] { _mmap, _mclr, _full, _type })
            for (int k = 0; k < 3; k++)
                if (t.Last[k] >= 0 && t.Last[k] < t.Values.Count)
                    t.Values[t.Last[k]] = 0;

        int bw = Width / 4, bh = Height / 4, blocks = bw * bh, blk = 0, stride = Width;
        var px = Pixels;
        while (blk < blocks)
        {
            int type = Get(ref br, _type);
            int run = BlockRuns[(type >> 2) & 0x3F];
            switch (type & 3)
            {
                case 0: // two-colour block
                    while (run-- > 0 && blk < blocks)
                    {
                        int clr = Get(ref br, _mclr), map = Get(ref br, _mmap);
                        int o = (blk / bw) * stride * 4 + (blk % bw) * 4;
                        byte hi = (byte)(clr >> 8), lo = (byte)clr;
                        for (int i = 0; i < 4; i++, o += stride, map >>= 4)
                        {
                            px[o] = (map & 1) != 0 ? hi : lo;
                            px[o + 1] = (map & 2) != 0 ? hi : lo;
                            px[o + 2] = (map & 4) != 0 ? hi : lo;
                            px[o + 3] = (map & 8) != 0 ? hi : lo;
                        }
                        blk++;
                    }
                    break;
                case 1: // full block
                {
                    int mode = 0;
                    if (_v4)
                    {
                        if (br.Bit() != 0) mode = 1;
                        else if (br.Bit() != 0) mode = 2;
                    }
                    while (run-- > 0 && blk < blocks)
                    {
                        int o = (blk / bw) * stride * 4 + (blk % bw) * 4;
                        if (mode == 0)
                        {
                            for (int i = 0; i < 4; i++, o += stride)
                            {
                                int p = Get(ref br, _full);
                                px[o + 2] = (byte)p;
                                px[o + 3] = (byte)(p >> 8);
                                p = Get(ref br, _full);
                                px[o] = (byte)p;
                                px[o + 1] = (byte)(p >> 8);
                            }
                        }
                        else if (mode == 1)
                        {
                            for (int i = 0; i < 2; i++, o += stride * 2)
                            {
                                int p = Get(ref br, _full);
                                px[o] = px[o + 1] = (byte)p;
                                px[o + 2] = px[o + 3] = (byte)(p >> 8);
                                Array.Copy(px, o, px, o + stride, 4);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 2; i++, o += stride * 2)
                            {
                                int p = Get(ref br, _full);
                                px[o + 2] = (byte)p;
                                px[o + 3] = (byte)(p >> 8);
                                p = Get(ref br, _full);
                                px[o] = (byte)p;
                                px[o + 1] = (byte)(p >> 8);
                                Array.Copy(px, o, px, o + stride, 4);
                            }
                        }
                        blk++;
                    }
                    break;
                }
                case 2: // unchanged
                    blk = Math.Min(blocks, blk + run);
                    break;
                case 3: // solid fill
                {
                    byte col = (byte)(type >> 8);
                    while (run-- > 0 && blk < blocks)
                    {
                        int o = (blk / bw) * stride * 4 + (blk % bw) * 4;
                        for (int i = 0; i < 4; i++, o += stride)
                            px.AsSpan(o, 4).Fill(col);
                        blk++;
                    }
                    break;
                }
            }
        }
    }

    /// <summary>Converts the current frame to RGB565.</summary>
    public void ToRgb565(ushort[] output)
    {
        Span<ushort> lut = stackalloc ushort[256];
        for (int i = 0; i < 256; i++)
            lut[i] = (ushort)((Palette[i * 3] >> 3) << 11 | (Palette[i * 3 + 1] >> 2) << 5 | (Palette[i * 3 + 2] >> 3));
        for (int i = 0; i < Pixels.Length && i < output.Length; i++)
            output[i] = lut[Pixels[i]];
    }
}
