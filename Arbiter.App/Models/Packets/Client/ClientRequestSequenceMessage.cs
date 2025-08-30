using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Packets.Client;

[InspectPacket(ClientCommand.RequestSequence)]
public class ClientRequestSequenceMessage : IPacketMessage
{
    [InspectSection("Request")]
    [InspectProperty]
    public byte Sequence { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        // The sequence of the packet is the sequence being requested
        Sequence = reader.Sequence ?? 0x00;
    }
}