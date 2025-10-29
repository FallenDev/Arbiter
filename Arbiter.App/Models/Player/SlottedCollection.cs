using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Arbiter.App.Models.Player;

public abstract class SlottedCollection<T> : ISlottedCollection<T> where T : class, ISlotted
{
    protected ReaderWriterLockSlim Lock { get; } = new();
    
    private readonly T?[] _items;
    
    public int Capacity { get; }

    public int Count
    {
        get
        {
            Lock.EnterReadLock();
            try
            {
                return _items.Count(x => x is not null);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }
    }

    public event Action<int, T>? ItemAdded;
    public event Action<int, T>? ItemRemoved;
    public event Action<int, T?, int, T?>? ItemsSwapped;
    public event Action<int, T, T>? ItemUpdated;
    public event Action? ItemsChanged;

    public SlottedCollection(int capacity)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero");
        }

        _items = new T[capacity];
        Capacity = capacity;
    }
    
    public int? GetFirstEmptySlot() => GetEmptySlots().FirstOrDefault();
    public int? GetFirstNonEmptySlot() => GetNonEmptySlots().FirstOrDefault();

    public IEnumerable<int> GetEmptySlots()
    {
        Lock.EnterReadLock();

        try
        {
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] is null)
                {
                    yield return i + 1;
                }
            }
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public IEnumerable<int> GetNonEmptySlots()
    {
        Lock.EnterReadLock();

        try
        {
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] is not null)
                {
                    yield return i + 1;
                }
            }
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public bool IsSlotEmpty(int slot)
    {
        if (slot < 1 || slot > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(slot),
                "Slot must be between 1 and the capacity of the collection");
        }

        Lock.EnterReadLock();

        try
        {
            return _items[slot - 1] is null;
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public bool TryFind(string name, [NotNullWhen(true)] out T? item)
    {
        item = default;

        Lock.EnterReadLock();

        try
        {
            foreach (var eachItem in _items)
            {
                if (eachItem is null || !string.Equals(name, eachItem.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                item = eachItem;
                return true;
            }

            return false;
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public void SetSlot(int slot, T item)
    {
        if (slot < 1 || slot > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(slot),
                "Slot must be between 1 and the capacity of the collection");
        }
        
        T? existing;
        
        Lock.EnterWriteLock();
        try
        {
            existing = _items[slot - 1];
            _items[slot - 1] = item;
        }
        finally
        {
            Lock.ExitWriteLock();
        }

        if (existing is null)
        {
            ItemAdded?.Invoke(slot, item);
        }
        else
        {
            ItemUpdated?.Invoke(slot, existing, item);
        }

        ItemsChanged?.Invoke();
    }

    public void ClearSlot(int slot)
    {
        if (slot < 1 || slot > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(slot),
                "Slot must be between 1 and the capacity of the collection");
        }
        
        T? existing;
        
        Lock.EnterWriteLock();
        try
        {
            existing = _items[slot - 1];
            _items[slot - 1] = null;
        }
        finally
        {
            Lock.ExitWriteLock();
        }

        if (existing is not null)
        {
            ItemRemoved?.Invoke(slot, existing);
            ItemsChanged?.Invoke();
        }
    }

    public void SwapSlot(int slotA, int slotB)
    {
        if (slotA < 1 || slotA > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(slotA),
                "Slot A must be between 1 and the capacity of the collection");
        }

        if (slotB < 1 || slotB > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(slotB),
                "Slot B must be between 1 and the capacity of the collection");
        }

        T? itemA;
        T? itemB;

        Lock.EnterWriteLock();

        try
        {
            itemA = _items[slotA - 1];
            itemB = _items[slotB - 1];

            _items[slotA - 1] = itemB;
            _items[slotB - 1] = itemA;
        }
        finally
        {
            Lock.ExitWriteLock();
        }

        ItemsSwapped?.Invoke(slotA, itemA, slotB, itemB);
        ItemsChanged?.Invoke();
    }

    public IEnumerator<T> GetEnumerator()
    {
        Lock.EnterReadLock();

        try
        {
            // Snapshot for thread safety
            return _items.Where(x => x is not null).Cast<T>().ToList().GetEnumerator();
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected T? GetSlot(int slot)
    {
        Lock.EnterReadLock();

        try
        {
            return _items[slot - 1];
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

}