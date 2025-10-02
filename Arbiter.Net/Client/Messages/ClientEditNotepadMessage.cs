using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.EditNotepad)]
public class ClientEditNotepadMessage : ClientMessage
{
    public byte Slot { get; set; }
    public string Content { get; set; } = string.Empty;

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Slot = reader.ReadByte();
        Content = reader.ReadString16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendByte(Slot);
        builder.AppendString16(Content);
    }
}