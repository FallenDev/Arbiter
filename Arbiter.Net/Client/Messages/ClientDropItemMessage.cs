using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientDropItemMessage : ClientMessage
{
    public byte Slot { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }

    public uint Quantity { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Slot = reader.ReadByte();
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
        Quantity = reader.ReadUInt32();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}