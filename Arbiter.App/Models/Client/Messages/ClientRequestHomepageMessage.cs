using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.RequestHomepage)]
public class ClientRequestHomepageMessage : IPacketMessage
{
    [InspectSection("Request")]
    [InspectProperty]
    public bool NeedsHomepage { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        NeedsHomepage = reader.ReadBoolean();
    }
}