using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.EntityTurn)]
public class ServerEntityTurnMessage : IPacketMessage
{
    [InspectSection("Entity")]
    [InspectProperty(ShowHex = true)]
    public uint EntityId { get; set; }
    
    [InspectSection("Facing")]
    [InspectProperty]
    public WorldDirection Direction { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        EntityId = reader.ReadUInt32();
        Direction = (WorldDirection)reader.ReadByte();
    }
}