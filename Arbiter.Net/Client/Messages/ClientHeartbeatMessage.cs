using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientHeartbeatMessage : INetworkSerializable
{
    public ushort Reply { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Reply = reader.ReadUInt16();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}