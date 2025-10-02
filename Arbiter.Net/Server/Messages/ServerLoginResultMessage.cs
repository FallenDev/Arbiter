using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.LoginResult)]
public class ServerLoginResultMessage : ServerMessage
{
    public LoginResult Result { get; set; }
    public string? Message { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Result = (LoginResult)reader.ReadByte();
        Message = reader.ReadString8();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);

        builder.AppendByte((byte)Result);
        builder.AppendString8(Message ?? string.Empty);
    }
}