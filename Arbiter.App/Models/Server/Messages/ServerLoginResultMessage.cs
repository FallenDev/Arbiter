using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.LoginResult)]
public class ServerLoginResultMessage : IPacketMessage
{
    [InspectSection("Result")]
    [InspectProperty]
    public ServerLoginMessageType MessageType { get; set; }

    [InspectProperty(ShowMultiline = true)]
    public string? Message { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        MessageType = (ServerLoginMessageType)reader.ReadByte();
        Message = reader.ReadString8();
    }
}