using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.AddSkill)]
public class ServerAddSkillMessage : ServerMessage
{
    public byte Slot { get; set; }
    public ushort Icon { get; set; }
    public string Name { get; set; } = string.Empty;

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Slot = reader.ReadByte();
        Icon = reader.ReadUInt16();
        Name = reader.ReadString8();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}