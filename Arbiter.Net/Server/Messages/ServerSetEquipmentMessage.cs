using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerSetEquipmentMessage : INetworkSerializable
{
    public EquipmentSlot Slot { get; set; }
    public ushort Sprite { get; set; }
    public DyeColor DyeColor { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint Durability { get; set; }
    public uint MaxDurability { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Slot = (EquipmentSlot)reader.ReadByte();
        Sprite = reader.ReadUInt16();
        DyeColor = (DyeColor)reader.ReadByte();
        Name = reader.ReadString8();

        reader.Skip(1); // always zero, not sure what it is

        MaxDurability = reader.ReadUInt32();
        Durability = reader.ReadUInt32();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}