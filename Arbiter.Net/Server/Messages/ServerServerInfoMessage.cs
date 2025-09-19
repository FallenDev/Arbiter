using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.ServerInfo)]
public class ServerServerInfoMessage : ServerMessage
{
    public ServerInfoType DataType { get; set; }
    public string? Value { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        DataType = (ServerInfoType)reader.ReadByte();
        Value = reader.ReadString8();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}