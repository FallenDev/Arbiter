using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.Assail)]
public class ClientAssailMessage : IPacketMessage
{
    // Nothing to show

    public void ReadFrom(NetworkPacketReader reader)
    {
        // Nothing to read
    }
}