using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.ClientVersion)]
public class ClientVersionMessage : IPacketMessage
{
    [InspectSection("Version Information")]
    [InspectProperty] public ushort Version { get; set; }
    [InspectProperty(ShowHex = true)] public ushort Checksum { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Version = reader.ReadUInt16();

        // Seems to be 0x4C4B on 7.41
        Checksum = reader.ReadUInt16();
    }
}