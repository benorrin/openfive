using OpenFive.Engine;
using OpenFive.Engine.Game;
using OpenFive.Engine.IO;
using OpenFive.Platform;

namespace OpenFive;

/// <summary>Entry point: finds the game disc, then runs the game (or the developer viewer).</summary>
static class Program
{
    static int Main(string[] args)
    {
        var opt = Options.Parse(args);
        if (opt.ShowHelp)
        {
            Console.WriteLine(Options.Help);
            return 0;
        }
        var settings = Settings.Load();
        // The disc given now, the one used last time (if it is still there), or a mounted CD.
        if (opt.Disc == null && settings.Disc != null && (File.Exists(settings.Disc) || Directory.Exists(settings.Disc)))
            opt.Disc = settings.Disc;
        opt.Disc ??= FindMountedDisc();
        opt.Language ??= settings.Language.StartsWith("de", StringComparison.OrdinalIgnoreCase) ? Language.German : Language.English;
        if (opt.Disc == null)
        {
            Fail("OpenFive needs your Famous Five: Treasure Island CD or a disc image of it.\n\n" +
                 "Insert the CD, or start OpenFive with the image: openfive --disc PATH " +
                 "(a .cue, .ccd, .img, .bin or .iso file, or the mounted CD). " +
                 "You can also open the image file with OpenFive from your file manager.\n\n" +
                 "The disc is remembered, so this is needed only once.", Options.Help);
            return 2;
        }
        GameData data;
        try
        {
            data = new GameData(GameFiles.Open(opt.Disc), opt.Language.Value);
        }
        catch (Exception e) when (e is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            Fail("Cannot read the game disc: " + e.Message);
            return 2;
        }
        using (data)
        {
            // Remember where the disc is, so it need not be given again.
            if (settings.Disc != opt.Disc)
            {
                settings.Disc = Path.GetFullPath(opt.Disc);
                settings.Save();
            }
            return opt.Viewer != null ? Viewer.Run(data, opt) : new GameApp(data, opt, settings).Run();
        }
    }

    /// <summary>A mounted Famous Five CD (the usual places desktops mount removable media).</summary>
    static string? FindMountedDisc()
    {
        if (!OperatingSystem.IsLinux())
            return null;
        string user = Environment.UserName;
        string[] roots = { $"/run/media/{user}", $"/media/{user}", "/media", "/mnt" };
        return roots.SelectMany(SubDirectories).FirstOrDefault(IsGameDisc);
    }

    /// <summary>The folders in <paramref name="root"/> (none if it is missing or cannot be read).</summary>
    static IEnumerable<string> SubDirectories(string root)
    {
        try
        {
            return Directory.Exists(root) ? Directory.GetDirectories(root) : Array.Empty<string>();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
    }

    /// <summary>Does the folder hold the game's files?</summary>
    static bool IsGameDisc(string dir)
    {
        try
        {
            using var files = GameFiles.Open(dir);
            return files.Exists(GameData.ResPath("common.res"));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>Reports a problem on the terminal and, when there is a desktop, in a message box.</summary>
    static void Fail(string message, string? details = null)
    {
        Console.Error.WriteLine(message + (details != null ? "\n\n" + details : ""));
        // Started from a terminal, the message there is enough.
        if (!Console.IsInputRedirected && !Console.IsErrorRedirected)
            return;
        try
        {
            Sdl.ShowSimpleMessageBox(Sdl.MESSAGEBOX_ERROR, "OpenFive", message, IntPtr.Zero);
        }
        catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException)
        {
            // No SDL: the terminal message has to do.
        }
    }
}
