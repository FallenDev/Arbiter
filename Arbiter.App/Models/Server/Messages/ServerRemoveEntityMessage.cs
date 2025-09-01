using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.RemoveEntity)]
public class ServerRemoveEntityMessage : IPacketMessage
{
    [InspectSection("Entity")]
    [InspectProperty(ShowHex = true)]
    public uint EntityId { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        EntityId = reader.ReadUInt32();
    }
}