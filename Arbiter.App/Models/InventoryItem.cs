using System.Text;

namespace Arbiter.App.Models;

public readonly struct InventoryItem
{
    public static InventoryItem Empty(int slot = 0) => new() { Slot = slot, IsEmpty = true, Name = string.Empty };

    public bool IsEmpty { get; private init; }
    public int Slot { get; init; }
    public ushort Sprite { get; init; }
    public byte Color { get; init; }
    public required string Name { get; init; }
    public long Quantity { get; init; }
    public bool IsStackable { get; init; }
    public long? Durability { get; init; }
    public long? MaxDurability { get; init; }

    public InventoryItem()
    {
        Quantity = 1;
    }

    public override string ToString()
    {
        var sb = new StringBuilder(Name);

        if (Quantity > 1 || IsStackable)
        {
            sb.Append($" [{Quantity}]");
        }

        if (Durability.HasValue && MaxDurability.HasValue)
        {
            sb.Append($" (Durability = {Durability} / {MaxDurability})");
        }

        return sb.ToString();
    }
}