using System.Collections.ObjectModel;
using Arbiter.App.Models.Player;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerInventoryViewModel : ViewModelBase
{
    private readonly PlayerInventory _inventory;

    [ObservableProperty]
    private PlayerInventorySlotViewModel? _selectedItem;
    
    public ObservableCollection<PlayerInventorySlotViewModel> InventorySlots { get; } = [];

    public PlayerInventoryViewModel(PlayerInventory inventory)
    {
        _inventory = inventory;

        for (var i = 0; i < inventory.Capacity; i++)
        {
            InventorySlots.Add(new PlayerInventorySlotViewModel(i + 1, inventory[i]));
        }

        _inventory.ItemAdded += OnItemAdded;
        _inventory.ItemRemoved += OnItemRemoved;
    }

    public void SetSlot(int slot, InventoryItem item) =>
        _inventory.SetSlot(slot, item);
    
    public void ClearSlot(int slot) =>
        _inventory.ClearSlot(slot);

    private void OnItemAdded(int slot, InventoryItem item)
    {
        if (slot < 1 || slot > _inventory.Capacity)
        {
            return;
        }

        InventorySlots[slot - 1] = new PlayerInventorySlotViewModel(slot, item);
    }

    private void OnItemRemoved(int slot, InventoryItem item)
    {
        if (slot < 1 || slot > _inventory.Capacity)
        {
            return;
        }

        InventorySlots[slot - 1] = new PlayerInventorySlotViewModel(slot, InventoryItem.Empty(slot));
    }
}