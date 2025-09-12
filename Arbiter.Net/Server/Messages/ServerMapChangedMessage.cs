using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMapChangedMessage : ServerMessage
{
    public ushort Result { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Result = reader.ReadUInt16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}