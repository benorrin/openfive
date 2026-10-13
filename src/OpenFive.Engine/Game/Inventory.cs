namespace OpenFive.Engine.Game;

/// <summary>
/// The rucksack: which of the game's items (1-32, the "COMMON_INVnn" objects) the Five carry.
/// </summary>
public sealed class Inventory
{
    public const int ItemCount = 32;
    readonly bool[] _carried = new bool[ItemCount + 1];

    public static string ObjectName(int item) => $"COMMON_INV{item:00}";

    /// <summary>Item number of a "COMMON_INVnn" object name, or 0.</summary>
    public static int ItemOf(string objectName) =>
        objectName.StartsWith("COMMON_INV", StringComparison.OrdinalIgnoreCase) &&
        int.TryParse(objectName.AsSpan(10), out int n) && n is >= 1 and <= ItemCount ? n : 0;

    public bool Has(int item) => item is >= 1 and <= ItemCount && _carried[item];

    public void Give(int item)
    {
        if (item is >= 1 and <= ItemCount)
            _carried[item] = true;
    }

    public void Take(int item)
    {
        if (item is >= 1 and <= ItemCount)
            _carried[item] = false;
    }

    public IEnumerable<int> Items => Enumerable.Range(1, ItemCount).Where(i => _carried[i]);

    public void Clear() => Array.Clear(_carried);
}
