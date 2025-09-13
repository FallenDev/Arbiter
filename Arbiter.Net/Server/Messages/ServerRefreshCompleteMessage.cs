using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerRefreshCompleteMessage : ServerMessage
{
    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        // Nothing to read
    }
    
    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}