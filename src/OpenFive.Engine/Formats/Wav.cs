namespace OpenFive.Engine.Formats;

/// <summary>Decoded PCM audio, interleaved stereo float at <see cref="Wav.OutputRate"/>.</summary>
public sealed class AudioClip
{
    public float[] Samples { get; }
    public int Frames => Samples.Length / 2;
    public AudioClip(float[] samples) => Samples = samples;
}

/// <summary>Reader for the uncompressed PCM ".wav" files used for speech, music and effects.</summary>
public static class Wav
{
    public const int OutputRate = 44100;

    public static AudioClip Load(byte[] d)
    {
        if (d.Length < 12 || d[0] != 'R' || d[1] != 'I' || d[2] != 'F' || d[3] != 'F' || d[8] != 'W' || d[9] != 'A')
            throw new InvalidDataException("not a RIFF/WAVE file");
        int channels = 1, rate = 22050, bits = 16, fmt = 1, dataPos = -1, dataLen = 0;
        for (int p = 12; p + 8 <= d.Length;)
        {
            int len = BitConverter.ToInt32(d, p + 4);
            int avail = Math.Min(len, d.Length - p - 8);
            string id = System.Text.Encoding.ASCII.GetString(d, p, 4);
            if (id == "fmt " && avail >= 16)
            {
                fmt = BitConverter.ToUInt16(d, p + 8);
                channels = BitConverter.ToUInt16(d, p + 10);
                rate = BitConverter.ToInt32(d, p + 12);
                bits = BitConverter.ToUInt16(d, p + 22);
            }
            else if (id == "data")
            {
                dataPos = p + 8;
                dataLen = Math.Max(0, avail);
            }
            if (len < 0)
                break;
            p += 8 + len + (len & 1);
        }
        if (dataPos < 0 || fmt != 1 || channels is < 1 or > 2 || (bits != 8 && bits != 16) || rate <= 0)
            throw new InvalidDataException("unsupported WAV format");
        int bps = bits / 8 * channels;
        int inFrames = dataLen / bps;
        float Sample(int f, int ch)
        {
            int s = dataPos + f * bps + (channels == 2 ? ch : 0) * (bits / 8);
            return bits == 8 ? (d[s] - 128) / 128f : BitConverter.ToInt16(d, s) / 32768f;
        }
        double step = (double)rate / OutputRate;
        int outFrames = inFrames > 0 ? (int)((inFrames - 1) / step) + 1 : 0;
        var pcm = new float[outFrames * 2];
        for (int i = 0; i < outFrames; i++)
        {
            double src = i * step;
            int f0 = (int)src, f1 = Math.Min(f0 + 1, inFrames - 1);
            float t = (float)(src - f0);
            for (int ch = 0; ch < 2; ch++)
                pcm[i * 2 + ch] = Sample(f0, ch) * (1 - t) + Sample(f1, ch) * t;
        }
        return new AudioClip(pcm);
    }
}
