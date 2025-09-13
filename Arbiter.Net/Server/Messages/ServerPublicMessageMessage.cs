using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.PublicMessage)]
public class ServerPublicMessageMessage : ServerMessage
{
    public PublicMessageType MessageType { get; set; }
    public uint SenderId { get; set; }
    public string Message { get; set; } = string.Empty;

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        MessageType = (PublicMessageType)reader.ReadByte();
        SenderId = reader.ReadUInt32();
        Message = reader.ReadString8();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}