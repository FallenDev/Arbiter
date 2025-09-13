using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.WorldMessage)]
public class ServerWorldMessageMessage : ServerMessage
{
    public WorldMessageType MessageType { get; set; }
    public string Message { get; set; } = string.Empty;

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        MessageType = (WorldMessageType)reader.ReadByte();
        Message = reader.ReadString16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}