using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientAssailMessage : ClientMessage
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