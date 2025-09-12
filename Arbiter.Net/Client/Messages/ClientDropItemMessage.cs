using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientDropItemMessage : INetworkSerializable
{
    public byte Slot { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }

    public uint Quantity { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Slot = reader.ReadByte();
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
        Quantity = reader.ReadUInt32();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}