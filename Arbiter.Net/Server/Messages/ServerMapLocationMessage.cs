using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMapLocationMessage : INetworkSerializable
{
    public ushort X { get; set; }
    public ushort Y { get; set; }
    public ushort UnknownX { get; set; }
    public ushort UnknownY { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
        UnknownX = reader.ReadUInt16();
        UnknownY = reader.ReadUInt16();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}