using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.MapChanged)]
public class ServerMapChangedMessage : ServerMessage
{
    public ushort Result { get; set; }

    public override void Deserialize(NetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Result = reader.ReadUInt16();
    }

    public override void Serialize(NetworkPacketBuilder builder)
    {
        base.Serialize(builder);

        builder.AppendUInt16(Result);
    }
}