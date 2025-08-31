using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.ServerInfo)]
public class ServerServerInfoMessage : IPacketMessage
{
    [InspectSection("Information")]
    [InspectProperty]
    public ServerInfoType DataType { get; set; }
    
    [InspectProperty]
    public string? Value { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        DataType = (ServerInfoType) reader.ReadByte();
        Value = reader.ReadString8();
    }
}