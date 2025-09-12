using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientRequestLoginNoticeMessage : ClientMessage
{
    // Nothing to show

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        // Nothing to read
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        // Nothing to write
    }
}