using OpenFive.Engine.Formats;
using OpenFive.Engine.Graphics;

namespace OpenFive.Engine.Game;

/// <summary>Plays a Smacker movie full screen at its own frame rate, with a music track.</summary>
public sealed class MoviePlayer
{
    readonly Smacker _video;
    readonly ushort[] _pixels;
    double _clock;
    int _shown;

    /// <summary>True once the movie has played to its end or was skipped.</summary>
    public bool Finished { get; private set; }

    /// <summary>Opens a movie from the disc and decodes its first frame.</summary>
    public MoviePlayer(GameData data, string path)
    {
        _video = data.Video(path);
        _pixels = new ushort[_video.Width * _video.Height];
        _video.NextFrame();
        _video.ToRgb565(_pixels);
        _shown = 1;
    }

    /// <summary>Advances by <paramref name="ms"/> milliseconds.</summary>
    public void Update(double ms)
    {
        if (Finished)
            return;
        _clock += ms;
        while (_clock >= _video.FrameDelay)
        {
            _clock -= _video.FrameDelay;
            // The last frame is a ring frame (a copy of the first); stop before it.
            if (_shown >= _video.Frames - 1)
            {
                Finished = true;
                return;
            }
            _video.NextFrame();
            _video.ToRgb565(_pixels);
            _shown++;
        }
    }

    /// <summary>Stops the movie.</summary>
    public void Skip() => Finished = true;

    /// <summary>Draws the current frame centred on a black screen.</summary>
    public void Draw(Framebuffer fb)
    {
        fb.Clear();
        int ox = (Framebuffer.Width - _video.Width) / 2, oy = (Framebuffer.Height - _video.Height) / 2;
        for (int y = 0; y < _video.Height; y++)
        {
            int ty = oy + y;
            if (ty < 0 || ty >= Framebuffer.Height)
                continue;
            Array.Copy(_pixels, y * _video.Width, fb.Pixels, ty * Framebuffer.Width + Math.Max(0, ox),
                Math.Min(_video.Width, Framebuffer.Width));
        }
    }
}
