using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.ChangePassword)]
public class ClientChangePasswordMessage : ClientMessage
{
    public string Name { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Name = reader.ReadString8();
        CurrentPassword = reader.ReadString8();
        NewPassword = reader.ReadString8();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendString8(Name);
        builder.AppendString8(CurrentPassword);
        builder.AppendString8(NewPassword);
    }
}