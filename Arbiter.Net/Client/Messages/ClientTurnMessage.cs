using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

public class ClientTurnMessage : INetworkSerializable
{
    public WorldDirection Direction { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Direction = (WorldDirection)reader.ReadByte();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}