using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.MenuChoice)]
public class ClientMenuChoiceMessage : ClientMessage
{
    public EntityTypeFlags EntityType { get; set; }
    public uint EntityId { get; set; }
    public ushort PursuitId { get; set; }
    public byte? MenuChoice { get; set; }
    public List<string> TextInputs { get; set; } = [];

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        EntityType = (EntityTypeFlags)reader.ReadByte();
        EntityId = reader.ReadUInt32();
        PursuitId = reader.ReadUInt16();

        var menuChoiceOrLength = reader.ReadByte();
        if (reader.IsEndOfPacket())
        {
            MenuChoice = menuChoiceOrLength;
        }
        else
        {
            reader.Position -= 1;
            TextInputs = reader.ReadStringArgs8().ToList();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}