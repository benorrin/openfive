using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenFive.Engine.Audio;
using OpenFive.Engine.Formats;

namespace OpenFive.Platform;

/// <summary>Feeds the engine's <see cref="AudioMixer"/> to the default SDL3 playback device.</summary>
public sealed unsafe class AudioOutput : IDisposable
{
    static AudioMixer? s_mixer;
    static float[] s_buffer = new float[8192];
    IntPtr _stream;

    public bool Available => _stream != IntPtr.Zero;

    public AudioOutput(AudioMixer mixer)
    {
        s_mixer = mixer;
        var spec = new Sdl.AudioSpec { Format = Sdl.AUDIO_F32, Channels = 2, Freq = Wav.OutputRate };
        _stream = Sdl.OpenAudioDeviceStream(Sdl.AUDIO_DEVICE_DEFAULT_PLAYBACK, ref spec, &Callback, IntPtr.Zero);
        if (_stream == IntPtr.Zero)
        {
            Console.Error.WriteLine("audio: cannot open playback device: " + Sdl.GetError());
            return;
        }
        Sdl.ResumeAudioStreamDevice(_stream);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static void Callback(IntPtr userdata, IntPtr stream, int additional, int total)
    {
        var mixer = s_mixer;
        if (mixer == null || additional <= 0)
            return;
        int floats = additional / sizeof(float);
        if (s_buffer.Length < floats)
            s_buffer = new float[floats];
        var span = s_buffer.AsSpan(0, floats & ~1);
        mixer.Mix(span);
        fixed (float* p = s_buffer)
            Sdl.PutAudioStreamData(stream, p, span.Length * sizeof(float));
    }

    public void Dispose()
    {
        if (_stream != IntPtr.Zero)
            Sdl.DestroyAudioStream(_stream);
        _stream = IntPtr.Zero;
        s_mixer = null;
    }
}
