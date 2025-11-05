using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia.Threading;

namespace Arbiter.App.Collections;

public sealed class ConcurrentSlottedCollection<T> : SlottedCollection<T> where T : class
{
    private readonly ReaderWriterLockSlim _lock = new();

    public Dispatcher Dispatcher { get; }

    public ConcurrentSlottedCollection(int capacity, Dispatcher? dispatcher = null)
        : base(capacity)
    {
        Dispatcher = dispatcher ?? Dispatcher.UIThread;
    }

    public override IEnumerable<int> GetEmptySlots()
    {
        _lock.EnterReadLock();
        try
        {
            return base.GetEmptySlots().ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public override IEnumerable<int> GetNonEmptySlots()
    {
        _lock.EnterReadLock();
        try
        {
            return base.GetNonEmptySlots().ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public override bool TryGetValue(Func<Slotted<T>, bool> predicate, out Slotted<T> item)
    {
        _lock.EnterReadLock();
        try
        {
            return base.TryGetValue(predicate, out item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public override T? GetSlot(int slot)
    {
        _lock.EnterReadLock();
        try
        {
            return base.GetSlot(slot);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public override T? SetSlot(int slot, T value)
    {
        _lock.EnterWriteLock();
        try
        {
            return base.SetSlot(slot, value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public override T? ClearSlot(int slot)
    {
        _lock.EnterWriteLock();
        try
        {
            return base.ClearSlot(slot);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    protected override void OnItemAdded(Slotted<T> item)
    {
        if (Dispatcher.CheckAccess())
        {
            base.OnItemAdded(item);
        }
        else
        {
            Dispatcher.Post(() => base.OnItemAdded(item));
        }
    }

    protected override void OnItemRemoved(Slotted<T> item)
    {
        if (Dispatcher.CheckAccess())
        {
            base.OnItemRemoved(item);
        }
        else
        {
            Dispatcher.Post(() => base.OnItemRemoved(item));
        }
    }

    protected override void OnItemChanged(Slotted<T> oldItem, Slotted<T> newItem)
    {
        if (Dispatcher.CheckAccess())
        {
            base.OnItemChanged(oldItem, newItem);
        }
        else
        {
            Dispatcher.Post(() => base.OnItemChanged(oldItem, newItem));
        }
    }
}