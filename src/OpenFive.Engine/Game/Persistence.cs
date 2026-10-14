using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFive.Engine.Game;

/// <summary>Where OpenFive keeps its settings and saved games.</summary>
public static class UserPaths
{
    /// <summary>
    /// The data folder: $OPENFIVE_HOME if set; otherwise %APPDATA%\OpenFive on Windows,
    /// ~/Library/Application Support/OpenFive on macOS, and $XDG_DATA_HOME/openfive
    /// (~/.local/share/openfive) elsewhere.
    /// </summary>
    public static string DataDir
    {
        get
        {
            string? env = Environment.GetEnvironmentVariable("OPENFIVE_HOME");
            if (!string.IsNullOrEmpty(env))
                return env;
            if (OperatingSystem.IsWindows())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenFive");
            if (OperatingSystem.IsMacOS())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenFive");
            string? xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            string baseDir = !string.IsNullOrEmpty(xdg) ? xdg
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
            return Path.Combine(baseDir, "openfive");
        }
    }

    /// <summary>The folder holding the saved games.</summary>
    public static string SaveDir => Path.Combine(DataDir, "saves");
    /// <summary>The settings file.</summary>
    public static string SettingsFile => Path.Combine(DataDir, "settings.json");
}

/// <summary>Player options (the original's options screen, plus the disc location).</summary>
public sealed class Settings
{
    /// <summary>Music volume, 0-100.</summary>
    public int MusicVolume { get; set; } = 90;

    /// <summary>Speech and sound-effect volume, 0-100.</summary>
    public int SpeechVolume { get; set; } = 100;

    /// <summary>Characters cast shadows.</summary>
    public bool Shadows { get; set; } = true;

    /// <summary>Background animations (birds, waves...) are played.</summary>
    public bool AnimatedBackgrounds { get; set; } = true;

    /// <summary>The picture is scaled up smoothly (off: sharp pixels).</summary>
    public bool SmoothScaling { get; set; } = true;

    /// <summary>Dialogue box with talking heads (off: text only).</summary>
    public bool DialogueBox { get; set; } = true;
    /// <summary>Names shown when the pointer rests on something.</summary>
    public bool HoverLabels { get; set; } = true;

    /// <summary>Game language: "en" or "de".</summary>
    public string Language { get; set; } = "en";

    /// <summary>Start in full-screen mode.</summary>
    public bool Fullscreen { get; set; }

    /// <summary>The disc image or folder used last time (used when no --disc is given).</summary>
    public string? Disc { get; set; }

    static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    /// <summary>Reads the settings file; defaults when there is none or it cannot be read.</summary>
    public static Settings Load()
    {
        try
        {
            if (File.Exists(UserPaths.SettingsFile))
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(UserPaths.SettingsFile)) ?? new Settings();
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine("settings: " + e.Message);
        }
        return new Settings();
    }

    /// <summary>Writes the settings file (errors are reported, not thrown).</summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(UserPaths.DataDir);
            File.WriteAllText(UserPaths.SettingsFile, JsonSerializer.Serialize(this, Json));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine("settings: " + e.Message);
        }
    }
}

/// <summary>A saved game (OpenFive's own format, JSON).</summary>
public sealed class SaveGame
{
    /// <summary>The number of save slots on the save and load screens.</summary>
    public const int Slots = 10;

    /// <summary>Format version.</summary>
    public int Version { get; set; } = 1;

    /// <summary>The name the player gave the save.</summary>
    public string Name { get; set; } = "";

    /// <summary>When the game was saved.</summary>
    public DateTime Saved { get; set; }

    /// <summary>The location the hero is in.</summary>
    public string Location { get; set; } = "";

    /// <summary>The location the hero came from (decides where the characters stand on loading).</summary>
    public string PreviousLocation { get; set; } = "";

    /// <summary>Story chapter.</summary>
    public int Quest { get; set; }

    /// <summary>Step within the chapter.</summary>
    public int Episode { get; set; }

    /// <summary>The character the player controls ("Dick" or "George").</summary>
    public string Hero { get; set; } = "Dick";

    /// <summary>The items in the rucksack.</summary>
    public List<int> Items { get; set; } = new();

    /// <summary>The story variables.</summary>
    public Dictionary<string, int> Vars { get; set; } = new();

    /// <summary>The on/off state of the objects that keep it between locations.</summary>
    public Dictionary<string, bool> Objects { get; set; } = new();

    /// <summary>The options already asked, per choice menu.</summary>
    public Dictionary<string, List<int>> AskedOptions { get; set; } = new();

    static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    static string SlotPath(int slot) => Path.Combine(UserPaths.SaveDir, $"slot{slot}.json");

    /// <summary>Reads a save slot; null when it is empty or cannot be read.</summary>
    public static SaveGame? Load(int slot)
    {
        try
        {
            string p = SlotPath(slot);
            return File.Exists(p) ? JsonSerializer.Deserialize<SaveGame>(File.ReadAllText(p)) : null;
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"save slot {slot}: {e.Message}");
            return null;
        }
    }

    /// <summary>Writes the game to a save slot (atomically); false if that failed.</summary>
    public bool Write(int slot)
    {
        try
        {
            Directory.CreateDirectory(UserPaths.SaveDir);
            string tmp = SlotPath(slot) + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(this, Json));
            File.Move(tmp, SlotPath(slot), overwrite: true);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"save slot {slot}: {e.Message}");
            return false;
        }
    }
}
