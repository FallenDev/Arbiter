using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.UserId)]
public class ServerUserIdMessage : IPacketMessage
{
    [InspectSection("User")]
    [InspectProperty(ShowHex = true)]
    public uint UserId { get; set; }
    
    [InspectProperty]
    public CharacterClass Class { get; set; }
    
    [InspectProperty]
    public bool HasGuild { get; set; }
    
    [InspectSection("Facing")]
    [InspectProperty]
    public WorldDirection Direction { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        UserId = reader.ReadUInt32();
        Direction = (WorldDirection)reader.ReadByte();

        HasGuild = reader.ReadBoolean();

        Class = (CharacterClass)reader.ReadByte();
        // three more unknown zero values
    }
}