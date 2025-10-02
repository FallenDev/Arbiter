using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.Whisper)]
public class ClientWhisperMessage : ClientMessage
{
    public string Target { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Target = reader.ReadString8();
        Content = reader.ReadString8();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendString8(Target);
        builder.AppendString8(Content);
    }
}