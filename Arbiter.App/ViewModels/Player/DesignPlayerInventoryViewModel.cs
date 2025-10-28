using Arbiter.App.Models.Player;

namespace Arbiter.App.ViewModels.Player;

public class DesignPlayerInventoryViewModel : PlayerInventoryViewModel
{
    public DesignPlayerInventoryViewModel()
        : base(CreateTestInventory())
    {

    }

    private static PlayerInventory CreateTestInventory()
    {
        var inventory = new PlayerInventory();

        var testItem = new InventoryItem
        {
            Slot = 1,
            Name = "Apple",
            Sprite = 1,
            Quantity = 10,
            IsStackable = true,
        };

        inventory.SetSlot(1, testItem);
        return inventory;
        ;
    }
}