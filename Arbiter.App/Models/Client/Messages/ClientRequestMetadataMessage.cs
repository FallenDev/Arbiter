using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.RequestMetadata)]
public class ClientRequestMetadataMessage : IPacketMessage
{
    [InspectSection("Request")]
    [InspectProperty]
    public ClientMetadataRequestType RequestType { get; set; }

    [InspectProperty] public string? Name { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        RequestType = (ClientMetadataRequestType)reader.ReadByte();
        Name = reader.ReadString8();
    }
}