namespace OpenFive.Engine.Story;

/// <summary>The background sound of each location ("" = silence; locations not listed keep the current one).</summary>
public static class Ambience
{
    /// <summary>The track of each location, by location name.</summary>
    public static readonly Dictionary<string, string> Tracks = new(StringComparer.OrdinalIgnoreCase)
    {
        ["loc01"] = "",
        ["loc03"] = @"wav\atmo\hausruhi.wav",
        ["loc04"] = @"wav\atmo\hausruhi.wav",
        ["loc06"] = @"wav\atmo\felsenhaus.wav",
        ["loc07"] = @"wav\atmo\felsenhaus.wav",
        ["loc08"] = @"wav\atmo\geheim.wav",
        ["loc09"] = @"wav\atmo\festlandstrand.wav",
        ["loc13"] = @"wav\atmo\meer.wav",
        ["loc14"] = @"wav\atmo\wind.wav",
        ["loc15"] = @"wav\atmo\meer.wav",
        ["loc16"] = @"wav\atmo\festlandstrand.wav",
        ["loc17"] = @"wav\atmo\wind.wav",
        ["loc17b"] = @"wav\atmo\wind.wav",
        ["loc18"] = @"wav\atmo\verlies1.wav",
        ["loc19"] = @"wav\atmo\verlies1.wav",
        ["loc20"] = @"wav\atmo\verlies1.wav",
        ["loc21"] = @"wav\atmo\verlies1.wav",
        ["loc22"] = @"wav\atmo\verlies1.wav",
        ["loc23"] = @"wav\atmo\verlies1.wav",
        ["loc24"] = @"wav\atmo\verlies1.wav",
        ["loc25"] = @"wav\atmo\verlies1.wav",
        ["loc26"] = @"wav\atmo\verlies1.wav",
        ["loc27"] = @"wav\atmo\schatz.wav",
        ["loc28"] = @"wav\atmo\verlies1.wav",
        ["loc29"] = @"wav\atmo\verlies1.wav",
        ["schiff"] = "",
        ["morning"] = "",
        ["morgen"] = "",
        ["hand"] = "",
        ["hand2"] = "",
        ["schiffc1"] = "",
        ["schiffc2"] = "",
        ["schiffc3"] = "",
        ["locKarte"] = "",
    };
}
