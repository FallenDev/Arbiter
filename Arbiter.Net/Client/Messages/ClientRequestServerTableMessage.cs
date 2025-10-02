using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.RequestServerTable)]
public class ClientRequestServerTableMessage : ClientMessage
{
    public bool NeedsServerTable { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        NeedsServerTable = reader.ReadBoolean();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendBoolean(NeedsServerTable);
    }
}