using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientHeartbeatMessage : ClientMessage
{
    public ushort Reply { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Reply = reader.ReadUInt16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}