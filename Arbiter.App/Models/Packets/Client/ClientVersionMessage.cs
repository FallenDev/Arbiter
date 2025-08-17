using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Packets.Client;

[InspectPacket(ClientCommand.ClientVersion)]
public class ClientVersionMessage : IPacketMessage
{
    [InspectProperty] public uint Version { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Version = reader.ReadUInt32();
    }
}