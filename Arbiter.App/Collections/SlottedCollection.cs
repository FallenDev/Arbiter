using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Arbiter.App.Collections;

public class SlottedCollection<T> : ISlottedCollection<T>
{
    private readonly T?[] _items;

    public int Capacity { get; }

    public virtual int Count => _items.Count(x => x is not null);

    public event Action<Slotted<T>>? ItemAdded;
    public event Action<Slotted<T>>? ItemRemoved;
    public event Action<Slotted<T>, Slotted<T>>? ItemChanged;

    public SlottedCollection(int capacity)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero");
        }

        _items = new T[capacity];
        Capacity = capacity;
    }

    public int? GetFirstEmptySlot(int startSlot = 1) => GetEmptySlots().FirstOrDefault(x => x >= startSlot);
    public int? GetFirstNonEmptySlot(int startSlot = 1) => GetNonEmptySlots().FirstOrDefault(x => x >= startSlot);

    public virtual IEnumerable<int> GetEmptySlots()
    {
        for (var i = 0; i < _items.Length; i++)
        {
            if (_items[i] is null)
            {
                yield return i + 1;
            }
        }
    }

    public virtual IEnumerable<int> GetNonEmptySlots()
    {
        for (var i = 0; i < _items.Length; i++)
        {
            if (_items[i] is not null)
            {
                yield return i + 1;
            }
        }
    }

    public bool IsSlotEmpty(int slot) => GetSlot(slot) is null;

    public virtual bool TryGetValue(Func<Slotted<T>, bool> predicate, out Slotted<T> item)
    {
        item = default;
        
        foreach (var slottedItem in this)
        {
            if (!slottedItem.HasValue)
            {
                continue;
            }

            if (!predicate(slottedItem))
            {
                continue;
            }
            
            item = slottedItem;
            return true;
        }

        return false;
    }

    public virtual T? GetSlot(int slot)
    {
        if (slot < 1 || slot > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(slot),
                "Slot must be between 1 and the capacity of the collection");
        }

        return _items[slot - 1];
    }

    public virtual T? SetSlot(int slot, T item)
    {
        if (slot < 1 || slot > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(slot),
                "Slot must be between 1 and the capacity of the collection");
        }

        var existingItem = _items[slot - 1];
        _items[slot - 1] = item;

        if (existingItem is not null)
        {
            OnItemRemoved(new Slotted<T>(slot, existingItem));
        }
        else
        {
            OnItemAdded(new Slotted<T>(slot, item));
        }

        if (existingItem is not null)
        {
            OnItemChanged(new Slotted<T>(slot, existingItem), new Slotted<T>(slot, item));
        }

        return existingItem;
    }

    public virtual T? ClearSlot(int slot)
    {
        if (slot < 1 || slot > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(slot),
                "Slot must be between 1 and the capacity of the collection");
        }

        var existingItem = _items[slot - 1];
        _items[slot - 1] = default;

        if (existingItem is not null)
        {
            OnItemRemoved(new Slotted<T>(slot, existingItem));
        }

        return existingItem;
    }

    public virtual IEnumerator<Slotted<T>> GetEnumerator()
    {
        return _items
            .Select((item, index) => new Slotted<T>(index + 1, item))
            .Where(item => item.HasValue)
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    protected virtual void OnItemAdded(Slotted<T> item) => ItemAdded?.Invoke(item);
    protected virtual void OnItemRemoved(Slotted<T> item) => ItemRemoved?.Invoke(item);
    protected virtual void OnItemChanged(Slotted<T> oldItem, Slotted<T> newItem) => ItemChanged?.Invoke(oldItem, newItem);
}