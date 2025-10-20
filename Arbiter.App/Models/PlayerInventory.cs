using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Arbiter.App.Models;

public sealed class PlayerInventory : IReadOnlyCollection<InventoryItem>
{
    public const int MaxItems = 60;

    private readonly InventoryItem[] _items = new InventoryItem[MaxItems];

    public InventoryItem this[int index] => _items[index];

    public int Capacity => _items.Length;
    public int Count => _items.Count(i => !i.IsEmpty);

    public event Action<int, InventoryItem>? ItemAdded;
    public event Action<int, InventoryItem>? ItemRemoved;
    public event Action? ItemsChanged;

    public PlayerInventory()
    {
        for (var i = 0; i < _items.Length; i++)
        {
            _items[i] = InventoryItem.Empty(i + 1);
        }
    }

    public int? GetFirstEmptySlot() => GetEmptySlots().FirstOrDefault();
    public int? GetFirstNonEmptySlot() => GetNonEmptySlots().FirstOrDefault();

    public IEnumerable<int> GetEmptySlots()
    {
        for (var i = 0; i < _items.Length; i++)
        {
            if (_items[i].IsEmpty)
            {
                yield return i + 1;
            }
        }
    }

    public IEnumerable<int> GetNonEmptySlots()
    {
        for (var i = 0; i < _items.Length; i++)
        {
            if (!_items[i].IsEmpty)
            {
                yield return i + 1;
            }
        }
    }

    public bool TryFindItem(string name, out InventoryItem item)
    {
        item = InventoryItem.Empty();

        foreach (var t in _items)
        {
            if (!string.Equals(name, t.Name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            item = t;
            return true;
        }

        return false;
    }

    public bool IsSlotEmpty(int slot)
        => slot is < 1 or > MaxItems || _items[slot - 1].IsEmpty;

    public void SetSlot(int slot, InventoryItem item)
    {
        if (slot is < 1 or > MaxItems)
        {
            return;
        }

        _items[slot - 1] = item with { Slot = slot };
        ItemAdded?.Invoke(slot, item);
        ItemsChanged?.Invoke();
    }

    public void ClearSlot(int slot)
    {
        if (slot is < 1 or > MaxItems)
        {
            return;
        }

        var item = _items[slot - 1];
        if (item.IsEmpty)
        {
            return;
        }

        _items[slot - 1] = InventoryItem.Empty(slot);
        ItemRemoved?.Invoke(slot, item);
        ItemsChanged?.Invoke();
    }

    public void SwapSlot(int a, int b)
    {
        var itemA = a is < 1 or > MaxItems ? InventoryItem.Empty(a) : _items[a - 1];
        var itemB = b is < 1 or > MaxItems ? InventoryItem.Empty(b) : _items[b - 1];

        _items[a - 1] = itemB with { Slot = a };
        _items[b - 1] = itemA with { Slot = b };

        ItemsChanged?.Invoke();
    }

    public void Clear()
    {
        for (var i = 0; i < _items.Length; i++)
        {
            var wasEmpty = _items[i].IsEmpty;
            _items[i] = InventoryItem.Empty(i + 1);

            if (!wasEmpty)
            {
                ItemRemoved?.Invoke(i + 1, _items[i]);
            }
        }

        ItemsChanged?.Invoke();
    }

    public IEnumerator<InventoryItem> GetEnumerator()
    {
        return _items.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}