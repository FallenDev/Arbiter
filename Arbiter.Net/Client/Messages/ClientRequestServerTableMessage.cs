using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientRequestServerTableMessage : INetworkSerializable
{
    public bool NeedsServerTable { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        NeedsServerTable = reader.ReadBoolean();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}