using Arbiter.App.Models;

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