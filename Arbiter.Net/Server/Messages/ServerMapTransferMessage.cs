using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMapTransferMessage : INetworkSerializable
{
    public ushort RowY { get; set; }
    public IReadOnlyList<byte> Data { get; set; } = [];

    public void Deserialize(INetworkPacketReader reader)
    {
        RowY = reader.ReadUInt16();
        Data = reader.ReadToEnd();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}