using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Arbiter.App.Models.Player;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerInventoryViewModel : ViewModelBase
{
    private readonly PlayerInventory _inventory;

    [ObservableProperty] private PlayerInventorySlotViewModel? _selectedItem;

    public ObservableCollection<PlayerInventorySlotViewModel> InventorySlots { get; } = [];

    public PlayerInventoryViewModel(PlayerInventory inventory)
    {
        _inventory = inventory;

        for (var i = 0; i < inventory.Capacity; i++)
        {
            InventorySlots.Add(new PlayerInventorySlotViewModel(i + 1));
        }

        _inventory.ItemAdded += OnItemAdded;
        _inventory.ItemUpdated += OnItemUpdated;
        _inventory.ItemRemoved += OnItemRemoved;
    }

    public bool HasItem(string name) => _inventory.TryFind(name, out _);
    
    public int? FindItem(string name) => _inventory.FindItem(name);

    public bool TryGetSlot(int slot, [NotNullWhen(true)] out InventoryItem? item)
    {
        item = null;

        if (slot < 1 || slot > _inventory.Capacity)
        {
            return false;
        }

        item = _inventory.GetSlot(slot);
        return item is not null;
    }

    public int? GetFirstEmptySlot(int startSlot = 1)
    {
        for (var i = startSlot; i <= _inventory.Capacity; i++)
        {
            if (_inventory.GetSlot(i) is null)
            {
                return i;
            }
        }

        return null;
    }

    public bool TryRemoveItem(string name, [NotNullWhen(true)] out int? slot)
    {
        slot = null;

        for (var i = 1; i <= _inventory.Capacity; i++)
        {
            var skill = _inventory.GetSlot(i);
            if (!string.Equals(name, skill?.Name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            slot = i;
            _inventory.ClearSlot(i);
            return true;
        }

        return false;
    }

    public void SetSlot(int slot, InventoryItem item)
    {
        var existing = _inventory.GetSlot(slot);
        if (existing?.IsVirtual is true)
        {
            return;
        }

        _inventory.SetSlot(slot, item);
    }

    public void ClearSlot(int slot) =>
        _inventory.ClearSlot(slot);

    private void OnItemAdded(int slot, InventoryItem item)
    {
        if (slot < 1 || slot > _inventory.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnItemAdded(slot, item));
            return;
        }

        InventorySlots[slot - 1] = new PlayerInventorySlotViewModel(slot, item);
    }

    private void OnItemUpdated(int slot, InventoryItem existing, InventoryItem updated)
    {
        if (slot < 1 || slot > _inventory.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnItemUpdated(slot, existing, updated));
            return;
        }

        InventorySlots[slot - 1] = new PlayerInventorySlotViewModel(slot, updated);
    }

    private void OnItemRemoved(int slot, InventoryItem item)
    {
        if (slot < 1 || slot > _inventory.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnItemRemoved(slot, item));
            return;
        }

        InventorySlots[slot - 1] = new PlayerInventorySlotViewModel(slot);
    }
}