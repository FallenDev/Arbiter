using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerCooldownMessage : INetworkSerializable
{
    public AbilityPane Pane { get; set; }
    public byte Slot { get; set; }
    public uint Seconds { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Pane = (AbilityPane)reader.ReadByte();
        Slot = reader.ReadByte();
        Seconds = reader.ReadUInt32();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}