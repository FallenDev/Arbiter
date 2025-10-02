using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.RequestSequence)]
public class ClientRequestSequenceMessage : ClientMessage
{
    public uint Unknown { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        // always the same value?
        Unknown = reader.ReadUInt32();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendUInt32(Unknown);
    }
}