using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.UserProfile)]
public class ServerUserProfileMessage : ServerMessage
{
    // This is the order the equipment is sent in, which differs from the enum order
    private static readonly List<EquipmentSlot> EquipmentSlotOrder =
    [
        EquipmentSlot.Weapon, EquipmentSlot.Armor, EquipmentSlot.Shield, EquipmentSlot.Helmet,
        EquipmentSlot.Earrings, EquipmentSlot.Necklace, EquipmentSlot.LeftRing, EquipmentSlot.RightRing,
        EquipmentSlot.LeftGauntlet, EquipmentSlot.RightGauntlet,
        EquipmentSlot.Belt, EquipmentSlot.Greaves, EquipmentSlot.Accessory1, EquipmentSlot.Boots,
        EquipmentSlot.Overcoat, EquipmentSlot.OverHelm, EquipmentSlot.Accessory2, EquipmentSlot.Accessory3,
    ];
    
    public uint EntityId { get; set; }
    public List<ServerEquipmentInfo> Equipment { get; set; } = [];
    public SocialStatus Status { get; set; }
    public string Name { get; set; } = string.Empty;
    public NationFlag Nation { get; set; }
    public string GuildRank { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsGroupOpen { get; set; }
    public string DisplayClass { get; set; } = string.Empty;
    public string Guild { get; set; } = string.Empty;
    public List<ServerLegendMark> LegendMarks { get; set; } = [];
    public IReadOnlyList<byte>? Portrait { get; set; }
    public string? Bio { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        EntityId = reader.ReadUInt32();
        
        foreach (var slot in EquipmentSlotOrder)
        {
            Equipment.Add(new ServerEquipmentInfo
            {
                Slot = slot,
                Sprite = reader.ReadUInt16(),
                Color = (DyeColor)reader.ReadByte(),
            });
        }
        
        Status = (SocialStatus)reader.ReadByte();
        Name = reader.ReadString8();
        Nation = (NationFlag)reader.ReadByte();
        Title = reader.ReadString8();
        IsGroupOpen = reader.ReadBoolean();
        GuildRank = reader.ReadString8();
        DisplayClass = reader.ReadString8();
        Guild = reader.ReadString8();

        var legendMarkCount = reader.ReadByte();

        for (var i = 0; i < legendMarkCount; i++)
        {
            LegendMarks.Add(new ServerLegendMark
            {
                Icon = (LegendMarkIcon)reader.ReadByte(),
                Color = (LegendMarkColor)reader.ReadByte(),
                Key = reader.ReadString8(),
                Text = reader.ReadString8()
            });
        }

        var remaining = reader.ReadUInt16();
        if (remaining >= 4)
        {
            return;
        }

        var portraitLength = reader.ReadUInt16();
        Portrait = reader.ReadBytes(portraitLength);

        Bio = reader.ReadString16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}