using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFive.Engine.Game;

/// <summary>Where OpenFive keeps its settings and saved games.</summary>
public static class UserPaths
{
    public static string DataDir
    {
        get
        {
            string? env = Environment.GetEnvironmentVariable("OPENFIVE_HOME");
            if (!string.IsNullOrEmpty(env))
                return env;
            if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenFive");
            string? xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            string baseDir = !string.IsNullOrEmpty(xdg) ? xdg
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
            return Path.Combine(baseDir, "openfive");
        }
    }

    public static string SaveDir => Path.Combine(DataDir, "saves");
    public static string SettingsFile => Path.Combine(DataDir, "settings.json");
}

/// <summary>Player options (the original's options screen, plus the disc location).</summary>
public sealed class Settings
{
    public int MusicVolume { get; set; } = 90;
    public int SpeechVolume { get; set; } = 100;
    public bool Shadows { get; set; } = true;
    public bool AnimatedBackgrounds { get; set; } = true;
    public bool SmoothScaling { get; set; } = true;
    /// <summary>Dialogue box with talking heads (off: text only).</summary>
    public bool DialogueBox { get; set; } = true;
    /// <summary>Names shown when the pointer rests on something.</summary>
    public bool HoverLabels { get; set; } = true;
    public string Language { get; set; } = "en";
    public bool Fullscreen { get; set; }
    public string? Disc { get; set; }

    static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

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
    public const int Slots = 10;
    public int Version { get; set; } = 1;
    public string Name { get; set; } = "";
    public DateTime Saved { get; set; }
    public string Location { get; set; } = "";
    public string PreviousLocation { get; set; } = "";
    public int Quest { get; set; }
    public int Episode { get; set; }
    public string Hero { get; set; } = "Dick";
    public List<int> Items { get; set; } = new();
    public Dictionary<string, int> Vars { get; set; } = new();
    public Dictionary<string, bool> Objects { get; set; } = new();
    public Dictionary<string, List<int>> AskedOptions { get; set; } = new();

    static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    static string SlotPath(int slot) => Path.Combine(UserPaths.SaveDir, $"slot{slot}.json");

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
