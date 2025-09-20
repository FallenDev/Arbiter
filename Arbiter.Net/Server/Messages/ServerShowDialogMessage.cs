using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.ShowDialog)]
public class ServerShowDialogMessage : ServerMessage
{
    public DialogType DialogType { get; set; }
    public EntityTypeFlags EntityType { get; set; }
    public uint? EntityId { get; set; }
    public ushort? Sprite { get; set; }
    public byte? Color { get; set; }
    public ushort? PursuitId { get; set; }
    public ushort? StepId { get; set; }
    public bool HasPreviousButton { get; set; }
    public bool HasNextButton { get; set; }
    public bool ShowGraphic { get; set; }
    public string? Name { get; set; }
    public string? Content { get; set; }
    public List<string> MenuChoices { get; set; } = [];
    public string? InputPrompt { get; set; }
    public byte? InputMaxLength { get; set; }
    public string? InputDescription { get; set; }
    public byte? Unknown1 { get; set; }
    public byte? Unknown2 { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        DialogType = (DialogType)reader.ReadByte();

        if (DialogType == DialogType.CloseDialog)
        {
            return;
        }

        EntityType = (EntityTypeFlags)reader.ReadByte();
        EntityId = reader.ReadUInt32();
        Unknown1 = reader.ReadByte();

        Sprite = reader.ReadUInt16();
        Color = reader.ReadByte();
        Unknown2 = reader.ReadByte();

        var spriteSecondary = reader.ReadUInt16();
        var colorSecondary = reader.ReadByte();

        PursuitId = reader.ReadUInt16();
        StepId = reader.ReadUInt16();

        HasPreviousButton = reader.ReadBoolean();
        HasNextButton = reader.ReadBoolean();
        ShowGraphic = !reader.ReadBoolean();    // inverted for some reason

        Name = reader.ReadString8();
        Content = reader.ReadString16();

        if (Sprite == 0)
        {
            Sprite = spriteSecondary;
        }

        if (Color == 0)
        {
            Color = colorSecondary;
        }

        if (DialogType is DialogType.Menu or DialogType.CreatureMenu)
        {
            var choiceCount = reader.ReadByte();
            for (var i = 0; i < choiceCount; i++)
            {
                MenuChoices.Add(reader.ReadString8());
            }
        }
        else if (DialogType == DialogType.TextInput)
        {
            InputPrompt = reader.ReadString8();
            InputMaxLength = reader.ReadByte();
            InputDescription = reader.ReadString8();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}