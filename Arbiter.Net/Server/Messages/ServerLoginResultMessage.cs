using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerLoginResultMessage : ServerMessage
{
    public ServerLoginMessageType MessageType { get; set; }
    public string? Message { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        MessageType = (ServerLoginMessageType)reader.ReadByte();
        Message = reader.ReadString8();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}