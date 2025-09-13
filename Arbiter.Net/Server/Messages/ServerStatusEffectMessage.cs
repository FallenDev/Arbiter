using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.StatusEffect)]
public class ServerStatusEffectMessage : ServerMessage
{
    public ushort Icon { get; set; }
    public StatusEffectDuration Duration { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Icon = reader.ReadUInt16();
        Duration = (StatusEffectDuration)reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}