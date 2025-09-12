using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerPublicMessageMessage : INetworkSerializable
{
    public PublicMessageType MessageType { get; set; }
    public uint SenderId { get; set; }
    public string Message { get; set; } = string.Empty;

    public void Deserialize(INetworkPacketReader reader)
    {
        MessageType = (PublicMessageType)reader.ReadByte();
        SenderId = reader.ReadUInt32();
        Message = reader.ReadString8();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}