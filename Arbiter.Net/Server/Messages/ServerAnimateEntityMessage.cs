using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerAnimateEntityMessage : INetworkSerializable
{
    public uint EntityId { get; set; }
    public byte Animation { get; set; }
    public ushort Speed { get; set; }
    public byte Effect { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        EntityId = reader.ReadUInt32();
        Animation = reader.ReadByte();
        Speed = reader.ReadUInt16();
        Effect = reader.ReadByte();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}