using OpenFive.Engine.Formats;

namespace OpenFive.Engine.Graphics;

/// <summary>
/// The game's bitmap font, read from the disc ("Res/BMP/COMMON/MISC/char.brg"). Glyphs are laid out
/// in rows 20 pixels high; each glyph is enclosed by two marker pixels (pure green) on the row's
/// marker line, in the order of <see cref="Order"/>. White is transparent.
/// </summary>
public sealed class BitmapFont
{
    /// <summary>Characters in the order the font image stores them.</summary>
    public const string Order = "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÜabcdefghijklmnopqrstuvwxyzäöü1234567890!\"/()=?-,.;:_+*´% ß";
    /// <summary>The height of a line of glyphs in the font image.</summary>
    public const int GlyphHeight = 20;

    const ushort Marker = 0x07E0, Transparent = 0xFFFF;

    readonly Image16 _image;
    readonly Dictionary<char, (int X, int Y, int W)> _glyphs = new();

    /// <summary>
    /// Finds the glyphs in the font image: each run between two marker pixels on a row is the
    /// next character of <see cref="Order"/>. (The markers are made transparent in a copy.)
    /// </summary>
    public BitmapFont(Image16 image)
    {
        _image = new Image16(image.Width, image.Height, (ushort[])image.Pixels.Clone());
        var px = _image.Pixels;
        int n = 0;
        for (int y = 0; y < _image.Height && n < Order.Length; y++)
        {
            for (int x = 0; x < _image.Width && n < Order.Length; x++)
            {
                if (px[y * _image.Width + x] != Marker)
                    continue;
                px[y * _image.Width + x] = Transparent;
                int start = x;
                x++;
                while (x < _image.Width && px[y * _image.Width + x] != Marker)
                    x++;
                if (x < _image.Width)
                    px[y * _image.Width + x] = Transparent;
                _glyphs[Order[n++]] = (start, y, x + 1 - start);
            }
        }
    }

    /// <summary>Maps typographic quotes and dashes onto the characters the font has.</summary>
    static char Normalize(char c) => c switch
    {
        '’' or '‘' or '\'' or '`' or '\u0092' => '´',
        '“' or '”' or '\u0093' or '\u0094' => '"',
        '–' or '—' or '\u0096' => '-',
        _ => c,
    };

    /// <summary>The width of a character (unknown ones are as wide as a space).</summary>
    public int CharWidth(char c) =>
        _glyphs.TryGetValue(Normalize(c), out var g) ? g.W : _glyphs.TryGetValue(' ', out var space) ? space.W : 8;

    /// <summary>The width of a text on one line.</summary>
    public int MeasureWidth(string s) => s.Sum(CharWidth);

    /// <summary>Draws text with its top-left at (x, y). Returns the x after the last glyph.</summary>
    public int Draw(Framebuffer fb, string s, int x, int y)
    {
        foreach (char ch in s)
        {
            char c = Normalize(ch);
            if (!_glyphs.TryGetValue(c, out var g))
            {
                x += CharWidth(' ');
                continue;
            }
            if (c != ' ')
                fb.Blit(_image, x, y, Transparent, g.X, g.Y, g.W, GlyphHeight);
            x += g.W;
        }
        return x;
    }

    /// <summary>Splits text into lines no wider than <paramref name="maxWidth"/>.</summary>
    public List<string> Wrap(string text, int maxWidth)
    {
        var lines = new List<string>();
        string cur = "";
        foreach (string word in text.Split(' '))
        {
            string trial = cur.Length == 0 ? word : cur + " " + word;
            if (MeasureWidth(trial) > maxWidth && cur.Length > 0)
            {
                lines.Add(cur);
                cur = word;
            }
            else
                cur = trial;
        }
        if (cur.Length > 0)
            lines.Add(cur);
        return lines;
    }
}
