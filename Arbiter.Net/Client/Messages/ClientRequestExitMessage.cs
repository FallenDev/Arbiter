using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientRequestExitMessage : INetworkSerializable
{
    public ClientExitReason Reason { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Reason = (ClientExitReason)reader.ReadByte();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}