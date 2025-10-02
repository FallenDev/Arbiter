using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.EntityTurn)]
public class ServerEntityTurnMessage : ServerMessage
{
    public uint EntityId { get; set; }
    public WorldDirection Direction { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        EntityId = reader.ReadUInt32();
        Direction = (WorldDirection)reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);

        builder.AppendUInt32(EntityId);
        builder.AppendByte((byte)Direction);
    }
}