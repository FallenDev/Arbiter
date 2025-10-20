using Arbiter.App.Models;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerInventorySlotViewModel : ViewModelBase
{
    private readonly InventoryItem _item;

    public int Slot { get; }
    public bool IsEmpty => _item.IsEmpty;
    public string Name => _item.Name;
    public ushort Sprite => _item.Sprite;
    public byte Color => _item.Color;
    public bool IsStackable => _item.IsStackable;
    public long Quantity => _item.Quantity;
    public long Durability => _item.Durability ?? 0;
    public long MaxDurability => _item.MaxDurability ?? 0;
    public bool HasDurability => Durability > 0 && MaxDurability > 0;
    public int PercentDurability => HasDurability ? (int)(Durability * 100 / MaxDurability) : 100;

    public PlayerInventorySlotViewModel(int slot, InventoryItem item)
    {
        Slot = slot;
        _item = item;
    }
}