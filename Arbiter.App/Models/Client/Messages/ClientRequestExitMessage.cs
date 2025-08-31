using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.RequestExit)]
public class ClientRequestExitMessage : IPacketMessage
{
    [InspectSection("Request")]
    [InspectProperty]
    public ClientExitReason Reason { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Reason = (ClientExitReason)reader.ReadByte();
    }
}