using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.RequestMetadata)]
public class ClientRequestMetadataMessage : IPacketMessage
{
    [InspectSection("Request")]
    [InspectProperty] public bool NeedsMetadata { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        NeedsMetadata = reader.ReadBoolean();
    }
}