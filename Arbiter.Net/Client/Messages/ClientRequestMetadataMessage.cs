using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientRequestMetadataMessage : INetworkSerializable
{
    public ClientMetadataRequestType RequestType { get; set; }
    public string? Name { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        RequestType = (ClientMetadataRequestType)reader.ReadByte();
        Name = reader.ReadString8();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}