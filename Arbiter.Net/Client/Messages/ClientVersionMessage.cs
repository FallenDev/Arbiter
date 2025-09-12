using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientVersionMessage : INetworkSerializable
{
    public ushort Version { get; set; }
    public ushort Checksum { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Version = reader.ReadUInt16();

        // Seems to be 0x4C4B on 7.41
        Checksum = reader.ReadUInt16();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}