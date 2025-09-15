using Arbiter.Net.Types;

namespace Arbiter.Net.Server;

public class ServerEquipmentInfo
{
    public EquipmentSlot Slot { get; set; }
    public ushort Sprite { get; set; }
    public DyeColor Color { get; set; }
}