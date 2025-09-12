using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerLoginResultMessage : INetworkSerializable
{
    public ServerLoginMessageType MessageType { get; set; }
    public string? Message { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        MessageType = (ServerLoginMessageType)reader.ReadByte();
        Message = reader.ReadString8();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}