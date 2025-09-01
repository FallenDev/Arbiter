using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.AnimateEntity)]
public class ServerAnimateEntityMessage : IPacketMessage
{
    [InspectSection("Entity")]
    [InspectProperty(ShowHex = true)]
    public uint EntityId { get; set; }
    
    [InspectSection("Animation")]
    [InspectProperty]
    public byte Animation { get; set; }
    
    [InspectProperty]
    public ushort Speed { get; set; }
    
    [InspectSection("Sound")]
    [InspectProperty(ShowHex = true)]
    public byte Effect { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        EntityId = reader.ReadUInt32();
        Animation = reader.ReadByte();
        Speed = reader.ReadUInt16();
        Effect = reader.ReadByte();
    }
}