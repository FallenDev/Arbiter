using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Arbiter.App.Collections;
using Arbiter.App.Models.Player;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerInventoryViewModel : ViewModelBase
{
    private readonly ISlottedCollection<InventoryItem> _inventory;

    [ObservableProperty] private PlayerInventorySlotViewModel? _selectedItem;

    public ObservableCollection<PlayerInventorySlotViewModel> InventorySlots { get; } = [];

    public PlayerInventoryViewModel(ISlottedCollection<InventoryItem> inventory)
    {
        _inventory = inventory;

        for (var i = 0; i < inventory.Capacity; i++)
        {
            InventorySlots.Add(new PlayerInventorySlotViewModel(i + 1));
        }

        _inventory.ItemAdded += OnItemAdded;
        _inventory.ItemRemoved += OnItemRemoved;
    }
    
    public int? GetFirstEmptySlot() => _inventory.GetFirstEmptySlot();

    public bool HasItem(string name) => GetItem(name, out _);

    public bool GetItem(string name, out Slotted<InventoryItem> item)
    {
        item = default;
        if (!_inventory.TryGetValue(x => string.Equals(x.Value.Name, name, StringComparison.OrdinalIgnoreCase),
                out var found))
        {
            return false;
        }

        item = default;
        return true;
    }

    public bool TryGetSlot(int slot, out Slotted<InventoryItem> item)
    {
        item = default;
        if (!_inventory.TryGetValue(x => x.Slot == slot, out var found))
        {
            return false;
        }

        item = default;
        return true;
    }

    public void SetSlot(int slot, InventoryItem item)
        => _inventory.SetSlot(slot, item);

    public void ClearSlot(int slot)
        => _inventory.ClearSlot(slot);

    private void OnItemAdded(Slotted<InventoryItem> item)
    {
        if (item.Slot < 1 || item.Slot > _inventory.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnItemAdded(item));
            return;
        }

        InventorySlots[item.Slot - 1] = new PlayerInventorySlotViewModel(item.Slot, item.Value);
    }

    private void OnItemRemoved(Slotted<InventoryItem> item)
    {
        if (item.Slot < 1 || item.Slot > _inventory.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnItemRemoved(item));
            return;
        }

        InventorySlots[item.Slot - 1] = new PlayerInventorySlotViewModel(item.Slot);
    }
}