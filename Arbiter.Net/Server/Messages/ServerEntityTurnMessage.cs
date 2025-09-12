using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerEntityTurnMessage : INetworkSerializable
{
    public uint EntityId { get; set; }
    public WorldDirection Direction { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        EntityId = reader.ReadUInt32();
        Direction = (WorldDirection)reader.ReadByte();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}