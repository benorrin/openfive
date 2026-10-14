namespace OpenFive.Engine.Audio;

/// <summary>
/// The game's sound rules on top of the mixer: one background track that only restarts when the
/// track actually changes, effects and speech at the volumes chosen in the options (0..100).
/// </summary>
public sealed class GameAudio
{
    readonly GameData _data;
    string _musicPath = "";

    /// <summary>Loads sounds from <paramref name="data"/> and plays them through <paramref name="mixer"/>.</summary>
    public GameAudio(GameData data, AudioMixer mixer)
    {
        _data = data;
        Mixer = mixer;
        ApplyVolumes();
    }

    /// <summary>The mixer the sounds play through.</summary>
    public AudioMixer Mixer { get; }

    /// <summary>Music volume, 0..100 like the original's sliders.</summary>
    public int MusicVolume { get; private set; } = 90;

    /// <summary>Effects volume, 0..100.</summary>
    public int EffectsVolume { get; private set; } = 100;

    /// <summary>Speech volume, 0..100.</summary>
    public int SpeechVolume { get; private set; } = 100;

    /// <summary>Sets the three volumes (0..100 each).</summary>
    public void SetVolumes(int music, int effects, int speech)
    {
        MusicVolume = Math.Clamp(music, 0, 100);
        EffectsVolume = Math.Clamp(effects, 0, 100);
        SpeechVolume = Math.Clamp(speech, 0, 100);
        ApplyVolumes();
    }

    void ApplyVolumes()
    {
        Mixer.SetCategoryVolume(SoundCategory.Music, AudioMixer.GainFromPercent(MusicVolume));
        Mixer.SetCategoryVolume(SoundCategory.Effects, AudioMixer.GainFromPercent(EffectsVolume));
        Mixer.SetCategoryVolume(SoundCategory.Speech, AudioMixer.GainFromPercent(SpeechVolume));
    }

    /// <summary>Switches the background track (path relative to the Res folder); same track keeps playing.</summary>
    public void SetBackgroundTrack(string path)
    {
        if (string.Equals(path, _musicPath, StringComparison.OrdinalIgnoreCase))
            return;
        _musicPath = path;
        Mixer.PlayMusic(path.Length == 0 ? null : _data.Sound(path));
    }

    /// <summary>Stops the background track.</summary>
    public void StopBackgroundTrack()
    {
        _musicPath = "";
        Mixer.PlayMusic(null);
    }

    /// <summary>Plays an effect; <paramref name="volume"/> 0..100, <paramref name="pan"/> -10000..10000. Returns the voice id (0 if missing).</summary>
    public int PlayEffect(string path, int volume = 100, int pan = 0, bool loop = false)
    {
        var clip = _data.Sound(path);
        return clip == null ? 0
            : Mixer.Play(clip, SoundCategory.Effects, loop, AudioMixer.GainFromPercent(volume), AudioMixer.PanFromUnits(pan));
    }

    /// <summary>Plays a speech recording by name; returns 0 if it does not exist.</summary>
    public int PlaySpeech(string name)
    {
        var clip = _data.Speech(name);
        return clip == null ? 0 : Mixer.Play(clip, SoundCategory.Speech);
    }

    /// <summary>Is the voice (from <see cref="PlayEffect"/> or <see cref="PlaySpeech"/>) still playing?</summary>
    public bool IsPlaying(int id) => id != 0 && Mixer.IsPlaying(id);

    /// <summary>Stops a voice.</summary>
    public void Stop(int id) => Mixer.Stop(id);
}
