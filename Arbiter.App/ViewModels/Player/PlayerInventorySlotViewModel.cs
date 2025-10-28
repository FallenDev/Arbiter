using Arbiter.App.Models.Player;

namespace Arbiter.App.ViewModels.Player;

public class PlayerInventorySlotViewModel : ViewModelBase
{
    private readonly InventoryItem? _item;

    public int Slot { get; }
    public bool IsEmpty => _item is null;
    public string Name => _item?.Name ?? string.Empty;
    public ushort Sprite => _item?.Sprite ?? 0;
    public byte Color => _item?.Color ?? 0;
    public bool IsStackable => _item?.IsStackable ?? false;
    public long Quantity => _item?.Quantity ?? 0;
    public long Durability => _item?.Durability ?? 0;
    public long MaxDurability => _item?.MaxDurability ?? 0;
    public bool HasDurability => Durability > 0 && MaxDurability > 0;
    public int PercentDurability => HasDurability ? (int)(Durability * 100 / MaxDurability) : 100;

    public PlayerInventorySlotViewModel(int slot, InventoryItem? item = null)
    {
        Slot = slot;
        _item = item;
    }

    public override string ToString()
    {
        if (IsEmpty)
        {
            return "<empty>";
        }

        return IsStackable ? $"{Name} [{Quantity}]" : Name;
    }
}