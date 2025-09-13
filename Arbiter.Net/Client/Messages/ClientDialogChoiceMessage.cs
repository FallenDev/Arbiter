using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.DialogChoice)]
public class ClientDialogChoiceMessage : ClientMessage
{
    public EntityTypeFlags EntityType { get; set; }
    public uint EntityId { get; set; }
    public ushort PursuitId { get; set; }
    public ushort DialogId { get; set; }
    
    public DialogArgsType ArgsType { get; set; }
    public byte? MenuChoice { get; set; }
    public List<string> TextInputs { get; set; } = [];

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        EntityType = (EntityTypeFlags)reader.ReadByte();
        EntityId = reader.ReadUInt32();
        PursuitId = reader.ReadUInt16();
        DialogId = reader.ReadUInt16();
        
        if (reader.IsEndOfPacket())
        {
            ArgsType = DialogArgsType.None;
            return;
        }

        ArgsType = (DialogArgsType)reader.ReadByte();

        if (ArgsType == DialogArgsType.MenuChoice)
        {
            MenuChoice = reader.ReadByte();
        }
        else if (ArgsType == DialogArgsType.TextInput)
        {
            while (!reader.IsEndOfPacket())
            {
                var text = reader.ReadString8();
                TextInputs.Add(text);
            }
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}