using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.Heartbeat)]
public class ServerHeartbeatMessage : IPacketMessage
{
    [InspectSection("Heartbeat")]
    [InspectProperty(ShowHex = true)]
    public ushort Request { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Request = reader.ReadUInt16();
    }
}