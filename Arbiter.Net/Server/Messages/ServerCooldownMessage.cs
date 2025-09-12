using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerCooldownMessage : ServerMessage
{
    public AbilityPane Pane { get; set; }
    public byte Slot { get; set; }
    public uint Seconds { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Pane = (AbilityPane)reader.ReadByte();
        Slot = reader.ReadByte();
        Seconds = reader.ReadUInt32();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}