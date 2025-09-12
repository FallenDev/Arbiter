using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerAddItemMessage : INetworkSerializable
{
    public byte Slot { get; set; }
    public ushort Sprite { get; set; }
    public DyeColor DyeColor { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint Quantity { get; set; }
    public bool IsStackable { get; set; }
    public uint Durability { get; set; }
    public uint MaxDurability { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Slot = reader.ReadByte();
        Sprite = reader.ReadUInt16();
        DyeColor = (DyeColor)reader.ReadByte();
        Name = reader.ReadString8();
        Quantity = reader.ReadUInt32();
        IsStackable = reader.ReadBoolean();
        MaxDurability = reader.ReadUInt32();
        Durability = reader.ReadUInt32();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}