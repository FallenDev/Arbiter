using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientRequestHomepageMessage : INetworkSerializable
{
    public bool NeedsHomepage { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        NeedsHomepage = reader.ReadBoolean();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}