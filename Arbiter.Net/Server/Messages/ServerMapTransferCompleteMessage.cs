using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMapTransferCompleteMessage : ServerMessage
{
    public byte Result { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Result = reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}