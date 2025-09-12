using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerServerInfoMessage : INetworkSerializable
{
    public ServerInfoType DataType { get; set; }
    public string? Value { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        DataType = (ServerInfoType)reader.ReadByte();
        Value = reader.ReadString8();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}