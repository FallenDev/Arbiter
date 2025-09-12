using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerAnimateEntityMessage : ServerMessage
{
    public uint EntityId { get; set; }
    public byte Animation { get; set; }
    public ushort Speed { get; set; }
    public byte Effect { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        EntityId = reader.ReadUInt32();
        Animation = reader.ReadByte();
        Speed = reader.ReadUInt16();
        Effect = reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}