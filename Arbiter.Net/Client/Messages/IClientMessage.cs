using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public interface IClientMessage : INetworkSerializable
{
    public ClientCommand Command { get; }
    public byte? Sequence { get; }
}