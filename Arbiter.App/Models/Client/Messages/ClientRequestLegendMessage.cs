using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.RequestLegend)]
public class ClientRequestLegendMessage : IPacketMessage
{
    // Nothing to show
    
    public void ReadFrom(NetworkPacketReader reader)
    {
        
    }
}