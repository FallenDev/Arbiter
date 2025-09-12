using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerUserIdMessage : INetworkSerializable
{
    public uint UserId { get; set; }
    public WorldDirection Direction { get; set; }
    public bool HasGuild { get; set; }
    public CharacterClass Class { get; set; }
    public IReadOnlyCollection<byte> Unknown { get; set; } = [];

    public void Deserialize(INetworkPacketReader reader)
    {
        UserId = reader.ReadUInt32();
        Direction = (WorldDirection)reader.ReadByte();

        HasGuild = reader.ReadBoolean();

        Class = (CharacterClass)reader.ReadByte();

        // these three bytes are always zero
        Unknown = reader.ReadBytes(3);
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}