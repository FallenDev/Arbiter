using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientRequestMetadataMessage : ClientMessage
{
    public ClientMetadataRequestType RequestType { get; set; }
    public string? Name { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        RequestType = (ClientMetadataRequestType)reader.ReadByte();
        Name = reader.ReadString8();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}