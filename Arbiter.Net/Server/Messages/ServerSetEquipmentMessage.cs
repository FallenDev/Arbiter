using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.SetEquipment)]
public class ServerSetEquipmentMessage : ServerMessage
{
    public EquipmentSlot Slot { get; set; }
    public ushort Sprite { get; set; }
    public DyeColor Color { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint Durability { get; set; }
    public uint MaxDurability { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Slot = (EquipmentSlot)reader.ReadByte();
        Sprite = reader.ReadUInt16();
        Color = (DyeColor)reader.ReadByte();
        Name = reader.ReadString8();

        reader.Skip(1); // always zero, not sure what it is

        MaxDurability = reader.ReadUInt32();
        Durability = reader.ReadUInt32();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}