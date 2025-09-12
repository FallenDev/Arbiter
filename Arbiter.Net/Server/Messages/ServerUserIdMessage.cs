using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerUserIdMessage : ServerMessage
{
    public uint UserId { get; set; }
    public WorldDirection Direction { get; set; }
    public bool HasGuild { get; set; }
    public CharacterClass Class { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        UserId = reader.ReadUInt32();
        Direction = (WorldDirection)reader.ReadByte();

        HasGuild = reader.ReadBoolean();

        Class = (CharacterClass)reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}