using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public abstract class ServerMessage : IServerMessage
{
    public ServerCommand Command { get; protected set; }
    public byte? Sequence { get; protected set; }

    public virtual void Deserialize(INetworkPacketReader reader)
    {
        Command = (ServerCommand)reader.Command;
        Sequence = reader.Sequence;
    }

    public virtual void Serialize(INetworkPacketBuilder builder)
    {

    }
}