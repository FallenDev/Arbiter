using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.UserId)]
public class ServerUserIdMessage : ServerMessage
{
    public uint UserId { get; set; }
    public WorldDirection Direction { get; set; }
    public bool HasGuild { get; set; }
    public CharacterClass Class { get; set; }
    public bool CanMove { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        UserId = reader.ReadUInt32();
        Direction = (WorldDirection)reader.ReadByte();

        HasGuild = reader.ReadBoolean();

        Class = (CharacterClass)reader.ReadByte();
        CanMove = (reader.ReadByte() & 1) == 0; // seems to be a bit flag but not sure what else it affects
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}