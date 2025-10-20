using System.Collections.ObjectModel;
using Arbiter.App.Models;
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
    }

    public void SetSlot(int slot, InventoryItem item) =>
        _inventory.SetSlot(slot, item);
    
    public void ClearSlot(int slot) =>
        _inventory.ClearSlot(slot);
}