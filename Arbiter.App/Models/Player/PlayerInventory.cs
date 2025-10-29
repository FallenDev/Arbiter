
namespace Arbiter.App.Models.Player;

public sealed class PlayerInventory : SlottedCollection<InventoryItem>
{
    public const int InventoryCount = 60;

    public PlayerInventory()
        : base(InventoryCount)
    {
    }

    public bool HasItem(string name) => TryFind(name, out _);
    public int FindItem(string name) => TryFind(name, out var item) ? item.Slot : -1;
    public int GetItemQuantity(string name) => TryFind(name, out var item) ? (int)item.Quantity : 0;
}