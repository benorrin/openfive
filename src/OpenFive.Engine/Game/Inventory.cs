namespace OpenFive.Engine.Game;

/// <summary>
/// The rucksack: which of the game's items (1-32, the "COMMON_INVnn" objects) the Five carry.
/// Item numbers are listed in <see cref="Item"/>.
/// </summary>
public sealed class Inventory
{
    /// <summary>The number of items in the game (numbered from 1).</summary>
    public const int ItemCount = 32;

    const string ObjectPrefix = "COMMON_INV";

    readonly bool[] _carried = new bool[ItemCount + 1];

    /// <summary>The name of the common object that shows an item in the rucksack ("COMMON_INV06").</summary>
    public static string ObjectName(int item) => $"{ObjectPrefix}{item:00}";

    /// <summary>Item number of a "COMMON_INVnn" object name, or 0.</summary>
    public static int ItemOf(string objectName) =>
        objectName.StartsWith(ObjectPrefix, StringComparison.OrdinalIgnoreCase) &&
        int.TryParse(objectName.AsSpan(ObjectPrefix.Length), out int n) && IsItem(n) ? n : 0;

    /// <summary>Is the item in the rucksack?</summary>
    public bool Has(int item) => IsItem(item) && _carried[item];

    /// <summary>Puts an item into the rucksack (unknown numbers are ignored).</summary>
    public void Give(int item)
    {
        if (IsItem(item))
            _carried[item] = true;
    }

    /// <summary>Takes an item out of the rucksack (unknown numbers are ignored).</summary>
    public void Take(int item)
    {
        if (IsItem(item))
            _carried[item] = false;
    }

    /// <summary>The items carried, in item-number order (the order the rucksack shows them in).</summary>
    public IEnumerable<int> Items => Enumerable.Range(1, ItemCount).Where(i => _carried[i]);

    /// <summary>Empties the rucksack.</summary>
    public void Clear() => Array.Clear(_carried);

    static bool IsItem(int item) => item is >= 1 and <= ItemCount;
}
