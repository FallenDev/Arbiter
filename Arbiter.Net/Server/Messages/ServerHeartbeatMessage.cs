using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerHeartbeatMessage : INetworkSerializable
{
    public ushort Request { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Request = reader.ReadUInt16();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}