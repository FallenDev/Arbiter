using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientRequestSequenceMessage : INetworkSerializable
{
    public byte Sequence { get; set; }
    public uint Unknown { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        // The sequence of the packet is the sequence being requested
        Sequence = reader.Sequence ?? 0x00;
        
        // Unknown
        Unknown = reader.ReadUInt32();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        // Nothing to write
    }
}