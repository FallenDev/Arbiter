using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.ShowMenu)]
public class ServerShowMenuMessage : ServerMessage
{
    public DialogMenuType MenuType { get; set; }
    public EntityTypeFlags EntityType { get; set; }
    public uint? EntityId { get; set; }
    public ushort? Sprite { get; set; }
    public byte? Color { get; set; }
    public ushort? PursuitId { get; set; }
    public bool ShowGraphic { get; set; }
    public string? Name { get; set; }
    public string? Content { get; set; }
    public string? Prompt { get; set; }
    public List<ServerDialogMenuChoice> MenuChoices { get; set; } = [];
    public List<ServerItemMenuChoice> ItemChoices { get; set; } = [];
    public List<byte> InventorySlots { get; set; } = [];
    public List<ServerSpellMenuChoice> SpellChoices { get; set; } = [];
    public List<ServerSkillMenuChoice> SkillChoices { get; set; } = [];
    public byte? Unknown1 { get; set; }
    public byte? Unknown2 { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        MenuType = (DialogMenuType)reader.ReadByte();
        
        EntityType = (EntityTypeFlags)reader.ReadByte();
        EntityId = reader.ReadUInt32();
        Unknown1 = reader.ReadByte();

        Sprite = reader.ReadUInt16();
        Color = reader.ReadByte();
        Unknown2 = reader.ReadByte();

        var spriteSecondary = reader.ReadUInt16();
        var colorSecondary = reader.ReadByte();
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

        if (MenuType is DialogMenuType.Menu or DialogMenuType.MenuWithArgs)
        {
            Prompt = MenuType == DialogMenuType.MenuWithArgs ? reader.ReadString8() : null;
            
            var choiceCount = reader.ReadByte();
            for (var i = 0; i < choiceCount; i++)
            {
                MenuChoices.Add(new ServerDialogMenuChoice
                {
                    Text = reader.ReadString8(),
                    PursuitId = reader.ReadUInt16()
                });
            }
        }
        else if (MenuType is DialogMenuType.TextInput or DialogMenuType.TextInputWithArgs)
        {
            Prompt = MenuType == DialogMenuType.TextInputWithArgs ? reader.ReadString8() : null;
            PursuitId = reader.ReadUInt16();
        }
        else if (MenuType is DialogMenuType.ItemChoices)
        {
            PursuitId = reader.ReadUInt16();
            var itemCount = reader.ReadUInt16();

            for (var i = 0; i < itemCount; i++)
            {
                ItemChoices.Add(new ServerItemMenuChoice
                {
                    Sprite = reader.ReadUInt16(),
                    Color = (DyeColor)reader.ReadByte(),
                    Price = reader.ReadUInt32(),
                    Name = reader.ReadString8(),
                    Description = reader.ReadString8()
                });
            }
        }
        else if (MenuType is DialogMenuType.UserInventory)
        {
            PursuitId = reader.ReadUInt16();
            var slotCount = reader.ReadByte();

            for (var i = 0; i < slotCount; i++)
            {
                InventorySlots.Add(reader.ReadByte());
            }
        }
        else if (MenuType is DialogMenuType.SpellChoices)
        {
            PursuitId = reader.ReadUInt16();
            var spellCount = reader.ReadUInt16();
            
            for (var i = 0; i < spellCount; i++)
            {
                reader.Skip(1); // sprite type
                
                SpellChoices.Add(new ServerSpellMenuChoice
                {
                    Sprite = reader.ReadUInt16(),
                    Color = (DyeColor)reader.ReadByte(),
                    Name = reader.ReadString8()
                });
            }
        }
        else if (MenuType is DialogMenuType.SkillChoices)
        {
            PursuitId = reader.ReadUInt16();
            var skillCount = reader.ReadUInt16();

            for (var i = 0; i < skillCount; i++)
            {
                reader.Skip(1); // sprite type

                SkillChoices.Add(new ServerSkillMenuChoice
                {
                    Sprite = reader.ReadUInt16(),
                    Color = (DyeColor)reader.ReadByte(),
                    Name = reader.ReadString8()
                });
            }
        }
        else if (MenuType is DialogMenuType.UserSkills or DialogMenuType.UserSpells)
        {
            PursuitId = reader.ReadUInt16();
        }
    }
    
    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}