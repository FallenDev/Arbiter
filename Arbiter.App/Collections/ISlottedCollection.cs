using System;
using System.Collections.Generic;

namespace Arbiter.App.Collections;

public interface ISlottedCollection<T> : IReadOnlyCollection<Slotted<T>> where T : class
{
    int Capacity { get; }

    event Action<Slotted<T>>? ItemAdded;
    event Action<Slotted<T>>? ItemRemoved;
    event Action<Slotted<T>, Slotted<T>>? ItemChanged;

    int? GetFirstEmptySlot(int startSlot = 1);
    int? GetFirstNonEmptySlot(int startSlot = 1);

    IEnumerable<int> GetEmptySlots();
    IEnumerable<int> GetNonEmptySlots();

    bool TryGetValue(Func<Slotted<T>, bool> predicate, out Slotted<T> item);

    T? GetSlot(int slot);

    T? SetSlot(int slot, T value);

    T? ClearSlot(int slot);
}