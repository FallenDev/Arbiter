using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMapLocationMessage : ServerMessage
{
    public ushort X { get; set; }
    public ushort Y { get; set; }
    public ushort UnknownX { get; set; }
    public ushort UnknownY { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
        UnknownX = reader.ReadUInt16();
        UnknownY = reader.ReadUInt16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}