using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Arbiter.App.Models.Player;

public interface ISlottedCollection<T> : IReadOnlyCollection<T> where T : ISlotted
{
    int Capacity { get; }

    event Action<int, T>? ItemAdded;
    event Action<int, T>? ItemRemoved;
    event Action<int, T?, T?>? ItemUpdated;
    event Action<int, T?, int, T?>? ItemsSwapped;
    event Action? ItemsChanged;

    int? GetFirstEmptySlot();
    int? GetFirstNonEmptySlot();

    IEnumerable<int> GetEmptySlots();
    IEnumerable<int> GetNonEmptySlots();
    
    T? GetSlot(int slot);

    bool TryFind(string name, [NotNullWhen(true)] out T? item);
}