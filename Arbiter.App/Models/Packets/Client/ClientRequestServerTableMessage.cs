using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Packets.Client;

[InspectPacket(ClientCommand.RequestServerTable)]
public class ClientRequestServerTableMessage : IPacketMessage
{
    [InspectSection("Request")]
    [InspectProperty] public bool NeedsServerTable { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        NeedsServerTable = reader.ReadBoolean();
    }
}