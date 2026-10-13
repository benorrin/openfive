using OpenFive.Engine.Formats;

namespace OpenFive.Engine.World;

/// <summary>A point inside a walkable shape.</summary>
public readonly record struct WalkPos(int Shape, int X, int Y);

/// <summary>A straight piece of a walk: go to (X, Y), then be on <see cref="Shape"/>.</summary>
public readonly record struct WalkLeg(int X, int Y, int Shape);

/// <summary>
/// The walkable parts of a location: polylines ("paths") and convex polygons ("areas"), joined
/// by link points. Shapes are numbered paths first, then areas.
/// </summary>
public sealed class WalkGraph
{
    public readonly List<WalkShape> Shapes = new();
    public readonly List<bool> Enabled = new();
    public int PathCount { get; }

    public WalkGraph(ResScript res)
    {
        Shapes.AddRange(res.Paths);
        Shapes.AddRange(res.Areas);
        PathCount = res.Paths.Count;
        foreach (var _ in Shapes)
            Enabled.Add(true);
    }

    public bool IsArea(int shape) => shape >= PathCount;
    public bool Valid(int shape) => shape >= 0 && shape < Shapes.Count;

    /// <summary>Is (x, y) on the polyline of path <paramref name="shape"/> (y matching the line at x)?</summary>
    public bool OnPath(int shape, int x, int y)
    {
        var p = Shapes[shape].Points;
        for (int i = 0; i < p.Length - 1; i++)
        {
            int x0 = p[i].X, y0 = p[i].Y, x1 = p[i + 1].X;
            if ((x0 < x && x <= x1) || (x < x0 && x1 <= x))
            {
                if (x1 == x0)
                    x1++;
                double ly = y0 + (x - x0) * ((double)(p[i + 1].Y - y0) / (x1 - x0));
                if ((int)Math.Round(ly) == y)
                    return true;
            }
        }
        return false;
    }

    /// <summary>Is (x, y) inside (or on the border of) the convex area <paramref name="shape"/>?</summary>
    public bool InArea(int shape, int x, int y)
    {
        var p = Shapes[shape].Points;
        if (p.Length < 3)
            return false;
        long orient = Cross(p[0], p[1], p[2].X, p[2].Y) < 1 ? -1 : 1;
        for (int i = 0; i < p.Length - 1; i++)
        {
            long c = Cross(p[i], p[i + 1], x, y);
            if (c == 0)
                return true;
            if (orient < 1 ? c > 0 : c < 0)
                return false;
        }
        return true;
    }

    static long Cross(WalkPoint a, WalkPoint b, int x, int y) =>
        (long)(b.X - a.X) * (y - a.Y) - (long)(b.Y - a.Y) * (x - a.X);

    /// <summary>Is (x, y) on its shape (path line or area polygon)?</summary>
    public bool Contains(int shape, int x, int y) => IsArea(shape) ? InArea(shape, x, y) : OnPath(shape, x, y);

    /// <summary>Closest point to (x, y) on segment a-b, as the original computes it (clamped by x).</summary>
    static (int X, int Y) ClosestOnSegment(WalkPoint a, WalkPoint b, int x, int y)
    {
        int ax = a.X, ay = a.Y, bx = b.X, by = b.Y;
        if (bx == ax)
            bx++;
        double m = (double)(by - ay) / (bx - ax);
        double inv = m == 0 ? double.NegativeInfinity : -(1.0 / m);
        int px;
        if (m == 0)
            px = x;
        else
            px = (int)Math.Round((y + (ax * m - ay) - x * inv) / (m - inv));
        int py = (int)Math.Round(ay + (px - ax) * m);
        if (ax < bx)
        {
            if (px < ax) (px, py) = (ax, ay);
            else if (px > bx) (px, py) = (bx, by);
        }
        else
        {
            if (px > ax) (px, py) = (ax, ay);
            else if (px < bx) (px, py) = (bx, by);
        }
        return (px, py);
    }

    static double Dist(int x0, int y0, int x1, int y1) => Math.Sqrt((double)(x1 - x0) * (x1 - x0) + (double)(y1 - y0) * (y1 - y0));

    /// <summary>The nearest walkable point to (x, y) over all enabled-or-not shapes, preferring paths on ties.</summary>
    public WalkPos? Nearest(int x, int y)
    {
        WalkPos? bestPath = null, bestArea = null;
        double dPath = double.MaxValue, dArea = double.MaxValue;
        for (int s = 0; s < Shapes.Count; s++)
        {
            var pts = Shapes[s].Points;
            if (IsArea(s) && InArea(s, x, y))
            {
                if (bestArea == null || dArea > 0)
                {
                    bestArea = new WalkPos(s, x, y);
                    dArea = 0;
                }
                continue;
            }
            for (int i = 0; i < pts.Length - 1; i++)
            {
                var (px, py) = ClosestOnSegment(pts[i], pts[i + 1], x, y);
                double d = Dist(px, py, x, y);
                if (IsArea(s))
                {
                    if (d < dArea) { dArea = d; bestArea = new WalkPos(s, px, py); }
                }
                else if (d < dPath) { dPath = d; bestPath = new WalkPos(s, px, py); }
            }
        }
        if (bestPath == null) return bestArea;
        if (bestArea == null) return bestPath;
        return dPath <= dArea ? bestPath : bestArea;
    }

    /// <summary>Interpolates a per-point value (depth or scale) by y between the shape's extreme points.</summary>
    double Interpolate(int shape, int x, int y, Func<WalkPoint, int> value)
    {
        var p = Shapes[shape].Points;
        if (IsArea(shape))
        {
            int lo = 0, hi = 0;
            for (int i = 1; i < p.Length; i++)
            {
                if (p[i].Y < p[lo].Y) lo = i;
                if (p[i].Y > p[hi].Y) hi = i;
            }
            if (p[hi].Y == p[lo].Y)
                return value(p[hi]);
            return value(p[hi]) - (p[hi].Y - y) * ((double)(value(p[hi]) - value(p[lo])) / (p[hi].Y - p[lo].Y));
        }
        // Path: the segment whose bounding box holds the point, else the nearest segment.
        int seg = -1;
        for (int i = 0; i < p.Length - 1; i++)
            if (Between(x, p[i].X, p[i + 1].X) && Between(y, p[i].Y, p[i + 1].Y))
                seg = i;
        if (seg < 0)
        {
            double best = double.MaxValue;
            for (int i = 0; i < p.Length - 1; i++)
            {
                var (px, py) = ClosestOnSegment(p[i], p[i + 1], x, y);
                double d = Dist(px, py, x, y);
                if (d < best) { best = d; seg = i; }
            }
        }
        if (seg < 0)
            return value(p[0]);
        var a = p[seg];
        var b = p[seg + 1];
        if (a.Y == b.Y)
            return value(a);
        return value(a) - (y - a.Y) * ((double)(value(b) - value(a)) / (a.Y - b.Y));
    }

    static bool Between(int v, int a, int b) => (a <= v && v <= b) || (b <= v && v <= a);

    /// <summary>Character scale at a point (the shape's scale values are percent * 2).</summary>
    public double ScaleAt(int shape, int x, int y) => Valid(shape) ? Interpolate(shape, x, y, p => p.Scale) / 200.0 : 0.5;

    /// <summary>Drawing depth at a point.</summary>
    public int DepthAt(int shape, int x, int y) => Valid(shape) ? (int)Math.Round(Interpolate(shape, x, y, p => p.Depth)) : 1000;

    /// <summary>Distance between two points measured along path <paramref name="shape"/>.</summary>
    double PathDistance(int shape, int x0, int y0, int x1, int y1)
    {
        var legs = AlongPath(shape, x0, y0, x1, y1);
        double d = 0;
        int cx = x0, cy = y0;
        foreach (var (lx, ly) in legs)
        {
            d += Dist(cx, cy, lx, ly);
            (cx, cy) = (lx, ly);
        }
        return d;
    }

    int SegmentOf(int shape, int x, int y)
    {
        var p = Shapes[shape].Points;
        int best = 0;
        double bd = double.MaxValue;
        for (int i = 0; i < p.Length - 1; i++)
        {
            var (px, py) = ClosestOnSegment(p[i], p[i + 1], x, y);
            double d = Dist(px, py, x, y);
            if (d < bd) { bd = d; best = i; }
        }
        return best;
    }

    /// <summary>The corner points to pass when walking along a path from one point to another (ending at the target).</summary>
    public List<(int X, int Y)> AlongPath(int shape, int x0, int y0, int x1, int y1)
    {
        var p = Shapes[shape].Points;
        var legs = new List<(int, int)>();
        int s0 = SegmentOf(shape, x0, y0), s1 = SegmentOf(shape, x1, y1);
        if (s1 > s0)
            for (int i = s0 + 1; i <= s1; i++)
                legs.Add((p[i].X, p[i].Y));
        else if (s1 < s0)
            for (int i = s0; i > s1; i--)
                legs.Add((p[i].X, p[i].Y));
        legs.Add((x1, y1));
        return legs;
    }

    /// <summary>
    /// The legs to walk from (x0, y0) on <paramref name="from"/> to (x1, y1) on <paramref name="to"/>:
    /// the cheapest chain of shapes through their link points (along paths, straight across areas).
    /// Stops before a disabled shape. Returns an empty list if there is no connection.
    /// </summary>
    public List<WalkLeg> Route(int from, int x0, int y0, int to, int x1, int y1)
    {
        var legs = new List<WalkLeg>();
        if (!Valid(from) || !Valid(to))
            return legs;
        if (from == to)
        {
            AddLegs(legs, from, x0, y0, x1, y1);
            return legs;
        }
        int n = Shapes.Count;
        var cost = new double[n];
        var prev = new int[n];
        var entry = new (int X, int Y)[n];
        var done = new bool[n];
        Array.Fill(cost, double.MaxValue);
        Array.Fill(prev, -1);
        cost[from] = 0;
        entry[from] = (x0, y0);
        while (true)
        {
            int u = -1;
            for (int i = 0; i < n; i++)
                if (!done[i] && cost[i] < double.MaxValue && (u < 0 || cost[i] < cost[u]))
                    u = i;
            if (u < 0 || u == to)
                break;
            done[u] = true;
            foreach (var link in Shapes[u].Links)
            {
                int t = link.Target - 1;
                if (!Valid(t) || done[t])
                    continue;
                double c = cost[u] + (IsArea(u)
                    ? Dist(entry[u].X, entry[u].Y, link.X, link.Y)
                    : PathDistance(u, entry[u].X, entry[u].Y, link.X, link.Y));
                if (c < cost[t])
                {
                    cost[t] = c;
                    prev[t] = u;
                    entry[t] = (link.X, link.Y);
                }
            }
        }
        if (prev[to] < 0)
            return legs;
        var chain = new List<int>();
        for (int s = to; s != -1 && chain.Count < 32; s = prev[s])
            chain.Add(s);
        chain.Reverse();
        int cx = x0, cy = y0;
        for (int i = 0; i < chain.Count - 1; i++)
        {
            int s = chain[i];
            if (!Enabled[s])
                return legs;
            var e = entry[chain[i + 1]];
            AddLegs(legs, s, cx, cy, e.X, e.Y);
            // The link point belongs to the next shape.
            legs[^1] = legs[^1] with { Shape = chain[i + 1] };
            (cx, cy) = e;
        }
        if (Enabled[to])
            AddLegs(legs, to, cx, cy, x1, y1);
        return legs;
    }

    void AddLegs(List<WalkLeg> legs, int shape, int x0, int y0, int x1, int y1)
    {
        if (IsArea(shape))
            legs.Add(new WalkLeg(x1, y1, shape));
        else
            foreach (var (x, y) in AlongPath(shape, x0, y0, x1, y1))
                legs.Add(new WalkLeg(x, y, shape));
    }
}
