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
    public ushort StepId { get; set; }
    
    public DialogArgsType ArgsType { get; set; }
    public byte? MenuChoice { get; set; }
    public List<string> TextInputs { get; set; } = [];

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        EntityType = (EntityTypeFlags)reader.ReadByte();
        EntityId = reader.ReadUInt32();
        PursuitId = reader.ReadUInt16();
        StepId = reader.ReadUInt16();
        
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
            TextInputs = reader.ReadStringArgs8().ToList();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        builder.AppendByte((byte)EntityType);
        builder.AppendUInt32(EntityId);
        builder.AppendUInt16(PursuitId);
        builder.AppendUInt16(StepId);
        if (ArgsType == DialogArgsType.None)
        {
            return;
        }
        builder.AppendByte((byte)ArgsType);
        if (ArgsType == DialogArgsType.MenuChoice)
        {
            builder.AppendByte(MenuChoice ?? 0);
        }
        else if (ArgsType == DialogArgsType.TextInput)
        {
            foreach (var text in TextInputs)
            {
                builder.AppendString8(text);
            }
        }
    }
}