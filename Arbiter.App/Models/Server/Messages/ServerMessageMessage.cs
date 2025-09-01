using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.WorldMessage)]
public class ServerMessageMessage : IPacketMessage
{
    [InspectSection("Message")]
    [InspectProperty]
    public WorldMessageType MessageType { get; set; }
    
    [InspectProperty(ShowMultiline = true)]
    public string Message { get; set; } = string.Empty;

    public void ReadFrom(NetworkPacketReader reader)
    {
        MessageType = (WorldMessageType)reader.ReadByte();
        Message = reader.ReadString16();
    }
}