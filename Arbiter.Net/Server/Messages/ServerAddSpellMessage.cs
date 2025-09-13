using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.AddSpell)]
public class ServerAddSpellMessage : ServerMessage
{
    public byte Slot { get; set; }
    public ushort Icon { get; set; }
    public SpellTargetType TargetType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public byte CastLines { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Slot = reader.ReadByte();
        Icon = reader.ReadUInt16();
        TargetType = (SpellTargetType)reader.ReadByte();
        Name = reader.ReadString8();
        Prompt = reader.ReadString8();
        CastLines = reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}