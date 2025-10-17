using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.RemoveEntity)]
public class ServerRemoveEntityMessage : ServerMessage
{
    public uint EntityId { get; set; }

    public override void Deserialize(NetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        EntityId = reader.ReadUInt32();
    }

    public override void Serialize(NetworkPacketBuilder builder)
    {
        base.Serialize(builder);

        builder.AppendUInt32(EntityId);
    }
}