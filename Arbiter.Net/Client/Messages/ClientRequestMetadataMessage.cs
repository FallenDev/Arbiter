using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.RequestMetadata)]
public class ClientRequestMetadataMessage : ClientMessage
{
    public MetadataRequestType RequestType { get; set; }
    public string? Name { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        RequestType = (MetadataRequestType)reader.ReadByte();
        Name = reader.ReadString8();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}