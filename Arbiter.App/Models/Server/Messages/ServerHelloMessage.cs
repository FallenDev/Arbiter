using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.Hello)]
public class ServerHelloMessage : IPacketMessage
{
    [InspectSection("Greeting")]
    [InspectProperty]
    public string Message { get; set; } = string.Empty;

    public void ReadFrom(NetworkPacketReader reader)
    {
        reader.Skip(1);
        Message = reader.ReadLine();
    }
}