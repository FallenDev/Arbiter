using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerRemoveEntityMessage : ServerMessage
{
    public uint EntityId { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        EntityId = reader.ReadUInt32();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}