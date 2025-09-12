using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerRemoveEntityMessage : INetworkSerializable
{
    public uint EntityId { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        EntityId = reader.ReadUInt32();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}