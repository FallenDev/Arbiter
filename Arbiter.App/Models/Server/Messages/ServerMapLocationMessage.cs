using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.MapLocation)]
public class ServerMapLocationMessage : IPacketMessage
{
    [InspectSection("Location")]
    [InspectProperty]
    public ushort X { get; set; }

    [InspectProperty] public ushort Y { get; set; }

    [InspectSection("Uncategorized")]
    [InspectProperty(ShowHex = true)]
    public ushort UnknownX { get; set; }

    [InspectProperty(ShowHex = true)] public ushort UnknownY { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
        UnknownX = reader.ReadUInt16();
        UnknownY = reader.ReadUInt16();
    }
}