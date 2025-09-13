using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.Cooldown)]
public class ServerCooldownMessage : ServerMessage
{
    public AbilityType Type { get; set; }
    public byte Slot { get; set; }
    public uint Seconds { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Type = (AbilityType)reader.ReadByte();
        Slot = reader.ReadByte();
        Seconds = reader.ReadUInt32();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}