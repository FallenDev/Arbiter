using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.Heartbeat)]
public class ClientHeartbeatMessage : IPacketMessage
{
    [InspectSection("Heartbeat")]
    [InspectProperty(ShowHex = true)]
    public ushort Reply { get; set; }
    
    public void ReadFrom(NetworkPacketReader reader)
    {
        Reply = reader.ReadUInt16();
    }
}