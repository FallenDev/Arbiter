using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerMessageMessage : INetworkSerializable
{
    public WorldMessageType MessageType { get; set; }
    public string Message { get; set; } = string.Empty;

    public void Deserialize(INetworkPacketReader reader)
    {
        MessageType = (WorldMessageType)reader.ReadByte();
        Message = reader.ReadString16();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}