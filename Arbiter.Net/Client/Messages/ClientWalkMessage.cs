using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

public class ClientWalkMessage : INetworkSerializable
{
    public WorldDirection Direction { get; set; }
    public byte StepCount { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Direction = (WorldDirection)reader.ReadByte();
        StepCount = reader.ReadByte();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}