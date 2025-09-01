using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.AddSpell)]
public class ServerAddSpellMessage : IPacketMessage
{
    [InspectSection("Spell")]
    [InspectProperty]
    public byte Slot { get; set; }
    
    [InspectProperty(ShowHex = true)]
    public ushort Icon { get; set; }
    
    [InspectProperty(ShowMultiline = true)]
    public string Name { get; set; } = string.Empty;
    
    [InspectProperty]
    public byte CastLines { get; set; }

    [InspectSection("Target")]
        
    [InspectProperty]
    public SpellTargetType TargetType { get; set; }
    
    [InspectProperty(ShowMultiline = true)] public string Prompt { get; set; } = string.Empty;

    public void ReadFrom(NetworkPacketReader reader)
    {
        Slot = reader.ReadByte();
        Icon = reader.ReadUInt16();
        TargetType = (SpellTargetType)reader.ReadByte();
        Name = reader.ReadString8();
        Prompt = reader.ReadString8();
        CastLines = reader.ReadByte();
    }
}