using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Packets.Client;

[InspectPacket(ClientCommand.RequestLoginNotice)]
public class ClientRequestLoginNoticeMessage : IPacketMessage
{
    // Nothing to show
    
    public void ReadFrom(NetworkPacketReader reader)
    {
        
    }
}