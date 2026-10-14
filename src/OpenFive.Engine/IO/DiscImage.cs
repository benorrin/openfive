using System.Text;

namespace OpenFive.Engine.IO;

/// <summary>
/// Read-only access to the files on the game CD, either from a disc image (.iso, raw .img/.bin,
/// or a .cue/.ccd sheet pointing at one) or from a mounted CD directory.
/// Paths are case-insensitive and may use '\' or '/'.
/// </summary>
public interface IGameFiles : IDisposable
{
    /// <summary>A short description of where the files come from, for logs.</summary>
    string Source { get; }

    /// <summary>Does the file exist?</summary>
    bool Exists(string path);

    /// <summary>Reads a whole file.</summary>
    /// <exception cref="FileNotFoundException">The file is not on the disc.</exception>
    byte[] ReadAll(string path);

    /// <summary>The names of the files and folders in a folder (none if it does not exist).</summary>
    IEnumerable<string> List(string directory);
}

/// <summary>Opens the game's files from wherever they are.</summary>
public static class GameFiles
{
    /// <summary>Opens a disc image file or a mounted disc directory.</summary>
    public static IGameFiles Open(string path)
    {
        if (Directory.Exists(path))
            return new DirectoryFiles(path);
        if (!File.Exists(path))
            throw new FileNotFoundException($"No disc image or directory at '{path}'");
        string ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext is ".cue" or ".ccd")
            path = ResolveSheet(path);
        return new IsoFiles(path);
    }

    /// <summary>The image file a .cue or .ccd sheet belongs to.</summary>
    static string ResolveSheet(string sheet)
    {
        string dir = Path.GetDirectoryName(Path.GetFullPath(sheet)) ?? ".";
        if (Path.GetExtension(sheet).Equals(".cue", StringComparison.OrdinalIgnoreCase))
        {
            foreach (string line in File.ReadLines(sheet))
            {
                string t = line.Trim();
                if (!t.StartsWith("FILE", StringComparison.OrdinalIgnoreCase))
                    continue;
                int a = t.IndexOf('"'), b = t.LastIndexOf('"');
                string name = a >= 0 && b > a ? t[(a + 1)..b] : t.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
                string full = Path.Combine(dir, name);
                if (File.Exists(full))
                    return full;
            }
        }
        // .ccd (CloneCD) sits next to a .img with the same name.
        foreach (string e in new[] { ".img", ".bin", ".iso", ".IMG", ".BIN", ".ISO" })
        {
            string candidate = Path.ChangeExtension(sheet, e);
            if (File.Exists(candidate))
                return candidate;
        }
        throw new FileNotFoundException($"Cannot find the image data referenced by '{sheet}'");
    }

    /// <summary>The lookup key of a path: lower case, '/' separators, no doubled or outer slashes.</summary>
    internal static string Normalize(string path)
    {
        string s = path.Replace('\\', '/').ToLowerInvariant();
        while (s.Contains("//"))
            s = s.Replace("//", "/");
        return s.Trim('/');
    }
}

/// <summary>ISO 9660 image (2048-byte sectors, or raw 2352-byte MODE1/MODE2 sectors).</summary>
sealed class IsoFiles : IGameFiles
{
    readonly FileStream _file;
    readonly int _sectorSize, _dataOffset;
    readonly Dictionary<string, (long lba, long size)> _files = new();
    readonly Dictionary<string, List<string>> _dirs = new();

    /// <inheritdoc/>
    public string Source { get; }

    /// <summary>Opens an image and reads its directory tree (Joliet names when available).</summary>
    public IsoFiles(string path)
    {
        Source = path;
        _file = File.OpenRead(path);
        (_sectorSize, _dataOffset) = DetectLayout();
        byte[] pvd = ReadSector(16);
        if (Encoding.ASCII.GetString(pvd, 1, 5) != "CD001")
            throw new InvalidDataException($"'{path}' is not an ISO 9660 disc image");
        // Prefer the Joliet supplementary descriptor (long, mixed-case names) when present.
        byte[] root = pvd[156..190];
        bool joliet = false;
        for (int n = 17; n < 64; n++)
        {
            byte[] d = ReadSector(n);
            if (d[0] == 255)
                break;
            if (d[0] == 2 && d[88] == '%' && d[89] == '/' && (d[90] == '@' || d[90] == 'C' || d[90] == 'E'))
            {
                root = d[156..190];
                joliet = true;
            }
        }
        _dirs[""] = new List<string>();
        Walk(root, "", joliet, 0);
    }

    /// <summary>The sector size and the offset of the data within a sector.</summary>
    (int SectorSize, int DataOffset) DetectLayout()
    {
        // Raw images carry a 12-byte sync pattern at the start of every sector.
        var head = new byte[16];
        _file.Position = 0;
        _file.ReadExactly(head);
        bool sync = head[0] == 0 && head[11] == 0;
        for (int i = 1; i < 11 && sync; i++)
            sync = head[i] == 0xFF;
        if (!sync)
            return (2048, 0);
        int mode = head[15];
        return (2352, mode == 2 ? 24 : 16);
    }

    /// <summary>Reads the 2048 data bytes of a sector (zeros past the end of the image).</summary>
    byte[] ReadSector(long lba)
    {
        var buf = new byte[2048];
        _file.Position = lba * _sectorSize + _dataOffset;
        int n = _file.Read(buf, 0, 2048);
        if (n < 2048)
            Array.Clear(buf, n, 2048 - n);
        return buf;
    }

    /// <summary>Reads <paramref name="size"/> bytes starting at a sector.</summary>
    byte[] ReadExtent(long lba, long size)
    {
        var data = new byte[size];
        long done = 0;
        while (done < size)
        {
            byte[] s = ReadSector(lba + done / 2048);
            int n = (int)Math.Min(2048, size - done);
            Buffer.BlockCopy(s, 0, data, (int)done, n);
            done += n;
        }
        return data;
    }

    /// <summary>Records the files and folders of a directory, and recurses into its folders.</summary>
    void Walk(byte[] rec, string dir, bool joliet, int depth)
    {
        if (depth > 32)
            return;
        long lba = BitConverter.ToUInt32(rec, 2), size = BitConverter.ToUInt32(rec, 10);
        byte[] data = ReadExtent(lba, size);
        int i = 0;
        while (i < data.Length)
        {
            int len = data[i];
            if (len == 0)
            {
                i = (i / 2048 + 1) * 2048;
                continue;
            }
            byte[] r = data[i..Math.Min(data.Length, i + len)];
            i += len;
            int nlen = r[32];
            if (nlen == 1 && (r[33] == 0 || r[33] == 1))
                continue;
            string name = joliet ? Encoding.BigEndianUnicode.GetString(r, 33, nlen) : Encoding.Latin1.GetString(r, 33, nlen);
            int semi = name.IndexOf(';');
            if (semi >= 0)
                name = name[..semi];
            if (!joliet && name.EndsWith('.'))
                name = name.TrimEnd('.');
            string full = GameFiles.Normalize(dir.Length == 0 ? name : dir + "/" + name);
            _dirs[GameFiles.Normalize(dir)].Add(name);
            if ((r[25] & 2) != 0)
            {
                _dirs.TryAdd(full, new List<string>());
                Walk(r, full, joliet, depth + 1);
            }
            else
            {
                _files[full] = (BitConverter.ToUInt32(r, 2), BitConverter.ToUInt32(r, 10));
            }
        }
    }

    /// <inheritdoc/>
    public bool Exists(string path) => _files.ContainsKey(GameFiles.Normalize(path));

    /// <inheritdoc/>
    public byte[] ReadAll(string path)
    {
        if (!_files.TryGetValue(GameFiles.Normalize(path), out var f))
            throw new FileNotFoundException($"'{path}' is not on the disc");
        lock (_file)
            return ReadExtent(f.lba, f.size);
    }

    /// <inheritdoc/>
    public IEnumerable<string> List(string directory) =>
        _dirs.TryGetValue(GameFiles.Normalize(directory), out var l) ? l : Enumerable.Empty<string>();

    /// <summary>Closes the image file.</summary>
    public void Dispose() => _file.Dispose();
}

/// <summary>A mounted CD (or any directory with the disc's contents).</summary>
sealed class DirectoryFiles : IGameFiles
{
    readonly string _root;
    readonly Dictionary<string, string> _map = new();

    /// <inheritdoc/>
    public string Source => _root;

    /// <summary>Indexes every file below <paramref name="root"/>.</summary>
    public DirectoryFiles(string root)
    {
        _root = Path.GetFullPath(root);
        foreach (string f in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
            _map[GameFiles.Normalize(Path.GetRelativePath(_root, f))] = f;
    }

    /// <inheritdoc/>
    public bool Exists(string path) => _map.ContainsKey(GameFiles.Normalize(path));

    /// <inheritdoc/>
    public byte[] ReadAll(string path) =>
        _map.TryGetValue(GameFiles.Normalize(path), out string? f)
            ? File.ReadAllBytes(f)
            : throw new FileNotFoundException($"'{path}' is not on the disc");

    /// <inheritdoc/>
    public IEnumerable<string> List(string directory)
    {
        string d = Path.Combine(_root, directory.Replace('\\', '/'));
        return Directory.Exists(d) ? Directory.EnumerateFileSystemEntries(d).Select(Path.GetFileName)!.OfType<string>() : Enumerable.Empty<string>();
    }

    /// <summary>Nothing to close.</summary>
    public void Dispose() { }
}
