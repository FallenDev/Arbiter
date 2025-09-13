using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.AnimateEntity)]
public class ServerAnimateEntityMessage : ServerMessage
{
    public uint EntityId { get; set; }
    public BodyAnimation Animation { get; set; }
    public ushort Speed { get; set; }
    public byte Sound { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        EntityId = reader.ReadUInt32();
        Animation = (BodyAnimation)reader.ReadByte();
        Speed = reader.ReadUInt16();
        Sound = reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}