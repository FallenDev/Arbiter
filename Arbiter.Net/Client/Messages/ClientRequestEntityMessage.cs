using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.RequestEntity)]
public class ClientRequestEntityMessage : ClientMessage
{
    public uint EntityId { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        EntityId = reader.ReadUInt32();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendUInt32(EntityId);
    }
}