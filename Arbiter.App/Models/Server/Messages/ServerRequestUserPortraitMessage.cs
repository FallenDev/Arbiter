using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.RequestUserPortrait)]
public class ServerRequestUserPortraitMessage : IPacketMessage
{
    [InspectSection("Parameters")]
    [InspectProperty(ShowHex = true)]
    public ushort Unknown { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Unknown = reader.ReadUInt16();
    }
}