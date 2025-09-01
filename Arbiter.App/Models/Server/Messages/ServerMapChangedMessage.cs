using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.MapChanged)]
public class ServerMapChangedMessage : IPacketMessage
{
    [InspectSection("Parameters")]
    [InspectProperty]
    public ushort Result { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Result = reader.ReadUInt16();
    }
}