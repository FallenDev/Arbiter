using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMapChangedMessage : INetworkSerializable
{
    public ushort Result { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Result = reader.ReadUInt16();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}