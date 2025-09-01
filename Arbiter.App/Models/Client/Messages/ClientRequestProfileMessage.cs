using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.RequestProfile)]
public class ClientRequestProfileMessage : IPacketMessage
{
    // Nothing to show
    
    public void ReadFrom(NetworkPacketReader reader)
    {
        // Nothing to read
    }
}