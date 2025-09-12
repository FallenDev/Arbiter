using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMapTransferCompleteMessage : INetworkSerializable
{
    public byte Result { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Result = reader.ReadByte();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}