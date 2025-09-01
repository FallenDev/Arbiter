using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.MapTransferComplete)]
public class ServerMapTransferCompleteMessage : IPacketMessage
{
    [InspectSection("Parameters")]
    [InspectProperty]
    public byte Result { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Result = reader.ReadByte();
    }
}