using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerEntityTurnMessage : ServerMessage
{
    public uint EntityId { get; set; }
    public WorldDirection Direction { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        EntityId = reader.ReadUInt32();
        Direction = (WorldDirection)reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}