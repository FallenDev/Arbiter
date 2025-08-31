using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.AddSkill)]
public class ServerAddSkillMessage : IPacketMessage
{
    [InspectSection("Skill")]
    [InspectProperty]
    public byte Slot { get; set; }
    
    [InspectProperty(ShowHex = true)]
    public ushort Icon { get; set; }
    
    [InspectProperty(ShowMultiline = true)]
    public string Name { get; set; } = string.Empty;
    
    public void ReadFrom(NetworkPacketReader reader)
    {
        Slot = reader.ReadByte();
        Icon = reader.ReadUInt16();
        Name = reader.ReadString8();
    }
}