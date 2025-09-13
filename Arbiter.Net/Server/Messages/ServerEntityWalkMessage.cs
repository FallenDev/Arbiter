using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.EntityWalk)]
public class ServerEntityWalkMessage : ServerMessage
{
    public uint EntityId { get; set; }
    public ushort PreviousX { get; set; }
    public ushort PreviousY { get; set; }
    public WorldDirection Direction { get; set; }
    public byte Unknown { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        EntityId = reader.ReadUInt32();
        PreviousX = reader.ReadUInt16();
        PreviousY = reader.ReadUInt16();
        Direction = (WorldDirection)reader.ReadByte();
        Unknown = reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}