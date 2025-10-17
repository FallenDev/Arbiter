using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.MapChanging)]
public class ServerMapChangingMessage : ServerMessage
{
    public byte ChangeType { get; set; }
    public uint Unknown { get; set; }

    public override void Deserialize(NetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        ChangeType = reader.ReadByte();
        Unknown = reader.ReadUInt32();
    }

    public override void Serialize(NetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendByte(ChangeType);
        builder.AppendUInt32(Unknown);
    }
}