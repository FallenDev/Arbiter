using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public abstract class ClientMessage : IClientMessage
{
    public ClientCommand Command { get; private set; }
    public byte? Sequence { get; private set; }

    public virtual void Deserialize(INetworkPacketReader reader)
    {
        Command = (ClientCommand)reader.Command;
        Sequence = reader.Sequence;
    }

    public virtual void Serialize(INetworkPacketBuilder builder)
    {

    }
}