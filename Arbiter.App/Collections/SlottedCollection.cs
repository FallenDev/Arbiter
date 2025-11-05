using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Arbiter.App.Collections;

public class SlottedCollection<T> : ISlottedCollection<T> where T: class
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

    public int? GetFirstEmptySlot(int startSlot = 1)
    {
        var availableSlot = GetEmptySlots().FirstOrDefault(x => x >= startSlot);
        return availableSlot > 0 ? availableSlot : null;
    }

    public int? GetFirstNonEmptySlot(int startSlot = 1)
    {
        var takenSlot = GetNonEmptySlots().FirstOrDefault(x => x >= startSlot);
        return takenSlot > 0 ? takenSlot : null;
    }

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
        _items[slot - 1] = null;

        if (existingItem is not null)
        {
            OnItemRemoved(new Slotted<T>(slot, existingItem));
        }

        return existingItem;
    }

    public virtual IEnumerator<Slotted<T>> GetEnumerator()
    {
        var items = new List<Slotted<T>>(_items.Length);
        for (var i = 0; i < _items.Length; i++)
        {
            var item = _items[i];
            if (item is not null)
            {
                items.Add(new Slotted<T>(i + 1, item));
            }
        }

        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    protected virtual void OnItemAdded(Slotted<T> item) => ItemAdded?.Invoke(item);
    protected virtual void OnItemRemoved(Slotted<T> item) => ItemRemoved?.Invoke(item);
    protected virtual void OnItemChanged(Slotted<T> oldItem, Slotted<T> newItem) => ItemChanged?.Invoke(oldItem, newItem);
}