using Arbiter.App.Models;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerInventorySlotViewModel : ViewModelBase
{
    private readonly InventoryItem _item;
    
    public int Slot { get; }

    public PlayerInventorySlotViewModel(int slot, InventoryItem item)
    {
        Slot = slot;
        _item = item;
    }
}