using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.Assail)]
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