using Arbiter.App.Collections;
using Arbiter.App.Models.Player;

namespace Arbiter.App.ViewModels.Player;

public sealed class DesignPlayerInventoryViewModel : PlayerInventoryViewModel
{
    public DesignPlayerInventoryViewModel()
        : base(CreateTestInventory())
    {

    }

    private static ISlottedCollection<InventoryItem> CreateTestInventory()
    {
        var inventory = new SlottedCollection<InventoryItem>(PlayerState.MaxInventorySlots);

        var testItem = new InventoryItem
        {
            Name = "Apple",
            Sprite = 1,
            Quantity = 10,
            IsStackable = true,
        };

        inventory.SetSlot(1, testItem);
        return inventory;
    }
}