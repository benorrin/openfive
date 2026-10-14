namespace OpenFive.Engine.World;

/// <summary>Geometry helpers for click areas and walk areas.</summary>
public static class Geometry
{
    /// <summary>Even-odd point-in-polygon test.</summary>
    public static bool Contains((int X, int Y)[] poly, int x, int y)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            var a = poly[i];
            var b = poly[j];
            if ((a.Y > y) != (b.Y > y) && x < (double)(b.X - a.X) * (y - a.Y) / (b.Y - a.Y) + a.X)
                inside = !inside;
        }
        return inside;
    }
}
