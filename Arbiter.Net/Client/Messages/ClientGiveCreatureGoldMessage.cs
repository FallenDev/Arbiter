using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.GiveCreatureGold)]
public class ClientGiveCreatureGoldMessage : ClientMessage
{
    public uint Amount { get; set; }
    public uint EntityId { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Amount = reader.ReadUInt32();
        EntityId = reader.ReadUInt32();
    }
    
    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}