using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.WalkResponse)]
public class ServerWalkResponseMessage : IPacketMessage
{
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
    public ushort UnknownX { get; set; }
    
    [InspectProperty(ShowHex = true)]
    public ushort UnknownY { get; set; }
    
    [InspectProperty]
    public byte Unknown { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Direction = (WorldDirection)reader.ReadByte();
        PreviousX = reader.ReadUInt16();
        PreviousY = reader.ReadUInt16();
        UnknownX = reader.ReadUInt16();
        UnknownY = reader.ReadUInt16();
        Unknown = reader.ReadByte();
    }
}