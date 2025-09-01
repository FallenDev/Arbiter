using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.PublicMessage)]
public class ServerPublicMessageMessage : IPacketMessage
{
    [InspectSection("Message")]
    [InspectProperty]
    public PublicMessageType MessageType { get; set; }
    
    [InspectProperty(ShowMultiline = true)]
    public string Message { get; set; } = string.Empty;
    
    [InspectSection("Sender")]
    [InspectProperty(ShowHex = true)]
    public uint SenderId { get; set; }
    
    public void ReadFrom(NetworkPacketReader reader)
    {
        MessageType = (PublicMessageType)reader.ReadByte();
        SenderId = reader.ReadUInt32();
        Message = reader.ReadString8();
    }
}