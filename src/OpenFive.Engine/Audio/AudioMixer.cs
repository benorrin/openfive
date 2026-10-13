using OpenFive.Engine.Formats;

namespace OpenFive.Engine.Audio;

public enum SoundCategory { Music, Effects, Speech }

/// <summary>
/// Software mixer for the game's sounds: a looping background track (music or ambience), sound
/// effects and speech. Produces interleaved stereo float samples at <see cref="Wav.OutputRate"/>;
/// the platform layer pulls them with <see cref="Mix"/> from its audio thread.
/// </summary>
public sealed class AudioMixer
{
    sealed class Voice
    {
        public required AudioClip Clip;
        public required SoundCategory Category;
        public int Position;
        public bool Loop, Playing = true;
        public float Gain = 1, Pan; // pan -1 (left) .. 1 (right)
    }

    readonly object _lock = new();
    readonly Dictionary<int, Voice> _voices = new();
    int _nextId = 1;
    int _musicId;
    readonly float[] _categoryVolume = { 1, 1, 1 };
    bool _paused;

    public float MasterVolume { get; set; } = 1;

    /// <summary>Volume of a category, 0..1 (the game's options sliders).</summary>
    public void SetCategoryVolume(SoundCategory c, float volume)
    {
        lock (_lock)
            _categoryVolume[(int)c] = Math.Clamp(volume, 0, 1);
    }

    public float GetCategoryVolume(SoundCategory c) => _categoryVolume[(int)c];

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

    public void Stop(int id)
    {
        lock (_lock)
            _voices.Remove(id);
    }

    public bool IsPlaying(int id)
    {
        lock (_lock)
            return _voices.TryGetValue(id, out var v) && v.Playing;
    }

    public void SetGain(int id, float gain)
    {
        lock (_lock)
            if (_voices.TryGetValue(id, out var v))
                v.Gain = gain;
    }

    public void SetPan(int id, float pan)
    {
        lock (_lock)
            if (_voices.TryGetValue(id, out var v))
                v.Pan = pan;
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

    public void StopAll(bool keepMusic = false)
    {
        lock (_lock)
        {
            foreach (int id in _voices.Keys.ToList())
                if (!keepMusic || id != _musicId)
                    _voices.Remove(id);
            if (!keepMusic)
                _musicId = 0;
        }
    }

    public bool Paused
    {
        get => _paused;
        set { lock (_lock) _paused = value; }
    }

    /// <summary>Fills <paramref name="output"/> (interleaved stereo) with the mix of all voices.</summary>
    public void Mix(Span<float> output)
    {
        output.Clear();
        lock (_lock)
        {
            if (_paused)
                return;
            List<int>? finished = null;
            foreach (var (id, v) in _voices)
            {
                if (!v.Playing)
                    continue;
                float g = v.Gain * _categoryVolume[(int)v.Category] * MasterVolume;
                float gl = g * (v.Pan > 0 ? 1 - v.Pan : 1), gr = g * (v.Pan < 0 ? 1 + v.Pan : 1);
                var s = v.Clip.Samples;
                int frames = output.Length / 2;
                for (int i = 0; i < frames; i++)
                {
                    if (v.Position >= v.Clip.Frames)
                    {
                        if (v.Loop && v.Clip.Frames > 0)
                            v.Position = 0;
                        else
                        {
                            v.Playing = false;
                            (finished ??= new List<int>()).Add(id);
                            break;
                        }
                    }
                    output[i * 2] += s[v.Position * 2] * gl;
                    output[i * 2 + 1] += s[v.Position * 2 + 1] * gr;
                    v.Position++;
                }
            }
            if (finished != null)
                foreach (int id in finished)
                    if (id != _musicId)
                        _voices.Remove(id);
        }
        for (int i = 0; i < output.Length; i++)
            output[i] = Math.Clamp(output[i], -1f, 1f);
    }
}
