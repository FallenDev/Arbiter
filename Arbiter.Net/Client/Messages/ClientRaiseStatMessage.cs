using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.RaiseStat)]
public class ClientRaiseStatMessage : ClientMessage
{
    public CharacterStatFlags Stat { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Stat = (CharacterStatFlags)reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendByte((byte)Stat);
    }
}