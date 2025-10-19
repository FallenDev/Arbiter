using System.Text;

namespace Arbiter.App.Models;

public readonly struct InventoryItem
{
    public static InventoryItem Empty => new() { IsEmpty = true };

    public bool IsEmpty { get; init; }

    public ushort Sprite { get; }
    public string Name { get; }
    public int Count { get; }
    public long? Durability { get; }
    public long? MaxDurability { get; }

    public InventoryItem(ushort sprite, string name, int count = 1, long? durability = null, long? maxDurability = null)
    {
        Sprite = sprite;
        Name = name;
        Count = count;
        Durability = durability;
        MaxDurability = maxDurability;
    }

    public override string ToString()
    {
        var sb = new StringBuilder(Name);

        if (Count > 1)
        {
            sb.Append($" (Count = {Count})");
        }

        if (Durability.HasValue && MaxDurability.HasValue)
        {
            sb.Append($" (Durability = {Durability} / {MaxDurability})");
        }

        return sb.ToString();
    }
}