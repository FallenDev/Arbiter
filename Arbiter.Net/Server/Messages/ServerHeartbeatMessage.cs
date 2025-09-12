using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerHeartbeatMessage : ServerMessage
{
    public ushort Request { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Request = reader.ReadUInt16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}