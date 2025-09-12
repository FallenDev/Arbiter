using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerRemoveItemMessage : ServerMessage
{
    public byte Slot { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Slot = reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}