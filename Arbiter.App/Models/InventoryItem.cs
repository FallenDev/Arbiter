using System;
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

    public static string GetBaseName(string name)
    {
        if (string.IsNullOrEmpty(name) || name[^1] != ']')
        {
            return name;
        }

        var bracketIndex = name.LastIndexOf(" [", StringComparison.Ordinal);
        if (bracketIndex < 0)
        {
            return name;
        }

        // Ensure it ends with ']' and content between '[' and ']' are digits only
        var start = bracketIndex + 2; // skip space and '['
        var end = name.Length - 1; // position of ']'
        if (start >= end) return name;

        for (var i = start; i < end; i++)
        {
            if (!char.IsDigit(name[i]))
            {
                return name;
            }
        }

        // Looks like a quantity suffix; strip it
        return name[..bracketIndex];
    }
}