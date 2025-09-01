using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.EntityWalk)]
public class ServerEntityWalkMessage : IPacketMessage
{
    [InspectSection("Entity")]
    [InspectProperty(ShowHex = true)]
    public uint EntityId { get; set; }
    
    [InspectSection("Walk")]
    [InspectProperty]
    public WorldDirection Direction { get; set; }
    
    [InspectSection("Location")]
    [InspectProperty]
    public ushort PreviousX { get; set; }
    
    [InspectProperty]
    public ushort PreviousY { get; set; }
    
    [InspectSection("Uncategorized")]
    [InspectProperty(ShowHex = true)]
    public byte Unknown { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        EntityId = reader.ReadUInt32();
        PreviousX = reader.ReadUInt16();
        PreviousY = reader.ReadUInt16();
        Direction = (WorldDirection)reader.ReadByte();
        Unknown = reader.ReadByte();
    }
}