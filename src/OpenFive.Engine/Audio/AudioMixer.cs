using OpenFive.Engine.Formats;

namespace OpenFive.Engine.Audio;

/// <summary>The kinds of sound, each with its own volume.</summary>
public enum SoundCategory
{
    /// <summary>The background track (music or ambience).</summary>
    Music,
    /// <summary>Sound effects.</summary>
    Effects,
    /// <summary>Spoken lines.</summary>
    Speech,
}

/// <summary>
/// Software mixer for the game's sounds: a looping background track (music or ambience), sound
/// effects and speech. Produces interleaved stereo float samples at <see cref="Wav.OutputRate"/>;
/// the platform layer pulls them with <see cref="Mix"/> from its audio thread, so every public
/// member is thread-safe.
/// </summary>
public sealed class AudioMixer
{
    /// <summary>A sound being played.</summary>
    sealed class Voice
    {
        public required AudioClip Clip;
        public required SoundCategory Category;
        public int Position;
        public bool Loop, Playing = true;
        public float Gain = 1;
        /// <summary>-1 (left) .. 1 (right).</summary>
        public float Pan;
    }

    readonly object _lock = new();
    readonly Dictionary<int, Voice> _voices = new();
    readonly float[] _categoryVolume = { 1, 1, 1 };
    int _nextId = 1;
    int _musicId;

    /// <summary>Sets the volume of a category, 0..1 (the game's options sliders).</summary>
    public void SetCategoryVolume(SoundCategory c, float volume)
    {
        lock (_lock)
            _categoryVolume[(int)c] = Math.Clamp(volume, 0, 1);
    }

    /// <summary>
    /// Converts the game's 0..100 volume scale into a linear gain: the original used
    /// (volume - 100) * 100 millibels of attenuation.
    /// </summary>
    public static float GainFromPercent(double percent)
    {
        double mb = Math.Clamp((percent - 100.0) * 100.0, -10000, 0);
        return mb <= -10000 ? 0 : (float)Math.Pow(10, mb / 2000.0);
    }

    /// <summary>Converts a pan of -10000 (left) .. 10000 (right) into -1..1.</summary>
    public static float PanFromUnits(int units) => Math.Clamp(units / 10000f, -1, 1);

    /// <summary>Starts playing a clip; returns its voice id.</summary>
    public int Play(AudioClip clip, SoundCategory category, bool loop = false, float gain = 1, float pan = 0)
    {
        lock (_lock)
        {
            int id = _nextId++;
            _voices[id] = new Voice { Clip = clip, Category = category, Loop = loop, Gain = gain, Pan = pan };
            return id;
        }
    }

    /// <summary>Replaces the background track (looped). Passing null stops it.</summary>
    public void PlayMusic(AudioClip? clip, float gain = 1)
    {
        lock (_lock)
        {
            if (_musicId != 0)
                _voices.Remove(_musicId);
            _musicId = clip == null ? 0 : Play(clip, SoundCategory.Music, true, gain);
        }
    }

    /// <summary>Stops a voice.</summary>
    public void Stop(int id)
    {
        lock (_lock)
            _voices.Remove(id);
    }

    /// <summary>Is the voice still playing?</summary>
    public bool IsPlaying(int id)
    {
        lock (_lock)
            return _voices.TryGetValue(id, out var v) && v.Playing;
    }

    /// <summary>Stops all speech; returns true if anything was playing.</summary>
    public bool StopSpeech()
    {
        lock (_lock)
        {
            var ids = _voices.Where(kv => kv.Value.Category == SoundCategory.Speech).Select(kv => kv.Key).ToList();
            foreach (int id in ids)
                _voices.Remove(id);
            return ids.Count > 0;
        }
    }

    /// <summary>Fills <paramref name="output"/> (interleaved stereo) with the mix of all voices.</summary>
    public void Mix(Span<float> output)
    {
        output.Clear();
        lock (_lock)
        {
            List<int>? finished = null;
            foreach (var (id, v) in _voices)
                if (v.Playing && !MixVoice(v, output))
                    (finished ??= new List<int>()).Add(id);
            // Finished voices are dropped; the background track stays (it loops).
            if (finished != null)
                foreach (int id in finished.Where(id => id != _musicId))
                    _voices.Remove(id);
        }
        for (int i = 0; i < output.Length; i++)
            output[i] = Math.Clamp(output[i], -1f, 1f);
    }

    /// <summary>Adds the voice's next samples to <paramref name="output"/>; false when it has ended.</summary>
    bool MixVoice(Voice v, Span<float> output)
    {
        float gain = v.Gain * _categoryVolume[(int)v.Category];
        float left = gain * (v.Pan > 0 ? 1 - v.Pan : 1), right = gain * (v.Pan < 0 ? 1 + v.Pan : 1);
        var samples = v.Clip.Samples;
        int frames = output.Length / 2;
        for (int i = 0; i < frames; i++)
        {
            if (v.Position >= v.Clip.Frames)
            {
                if (!v.Loop || v.Clip.Frames == 0)
                {
                    v.Playing = false;
                    return false;
                }
                v.Position = 0;
            }
            output[i * 2] += samples[v.Position * 2] * left;
            output[i * 2 + 1] += samples[v.Position * 2 + 1] * right;
            v.Position++;
        }
        return true;
    }
}
