using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.LightLevel)]
public class ServerLightLevelMessage : IPacketMessage
{
    [InspectSection("Light Level")]
    [InspectProperty]
    public byte Brightness { get; set; }

    [InspectProperty(ShowHex = true)] public byte Unknown { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Brightness = reader.ReadByte();
        Unknown = reader.ReadByte();
    }
}