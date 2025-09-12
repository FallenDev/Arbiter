using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientAssailMessage : INetworkSerializable
{
    // Nothing to show

    public void Deserialize(INetworkPacketReader reader)
    {
        // Nothing to read
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        // Nothing to write
    }
}