using System.Collections.Generic;
using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.ShowPlayer)]
public class ServerShowPlayerMessage : IPacketMessage
{
    [InspectSection("Player")]
    [InspectProperty(ShowHex = true)]
    public uint EntityId { get; set; }
    
    [InspectProperty]
    public NameTagStyle NameStyle { get; set; }

    [InspectProperty]
    public string? Name { get; set; }

    [InspectProperty(ShowMultiline = true)]
    public string? GroupBox { get; set; }

    [InspectSection("Location")]
    [InspectProperty]
    public ushort X { get; set; }

    [InspectProperty] public ushort Y { get; set; }

    [InspectSection("Facing")]
    [InspectProperty]
    public WorldDirection Direction { get; set; }

    [InspectSection("Body")]
    [InspectProperty(ShowHex = true)]
    public ushort HeadSprite { get; set; }

    [InspectProperty] public byte FaceShape { get; set; }

    [InspectProperty] public DyeColor? HairColor { get; set; }

    [InspectProperty(ShowHex = true)] public BodySprite? BodySprite { get; set; }

    [InspectProperty] public SkinColor? SkinColor { get; set; }
    
    [InspectSection("Monster Form", IsExpandedHandler = nameof(IsMonsterForm))]
    [InspectProperty(ShowHex = true)] public ushort? MonsterSprite { get; set; }
    
    [InspectProperty(ShowMultiline = true)]
    public IReadOnlyList<byte>? MonsterUnknown { get; set; }

    [InspectSection("Equipment", IsExpandedHandler = nameof(IsHumanForm))]
    [InspectProperty(ShowHex = true)]
    public ushort? Armor1Sprite { get; set; }

    [InspectProperty(ShowHex = true)] public ushort? Armor2Sprite { get; set; }

    [InspectProperty] public DyeColor? PantsColor { get; set; }

    [InspectProperty(ShowHex = true)] public byte? BootsSprite { get; set; }
    [InspectProperty] public DyeColor? BootsColor { get; set; }

    [InspectProperty(ShowHex = true)] public ushort? WeaponSprite { get; set; }

    [InspectProperty(ShowHex = true)] public byte? ShieldSprite { get; set; }

    [InspectProperty(ShowHex = true)] public ushort? Accessory1Sprite { get; set; }

    [InspectProperty] public DyeColor? Accessory1Color { get; set; }

    [InspectProperty(ShowHex = true)] public ushort? Accessory2Sprite { get; set; }

    [InspectProperty] public DyeColor? Accessory2Color { get; set; }

    [InspectProperty(ShowHex = true)] public ushort? Accessory3Sprite { get; set; }

    [InspectProperty] public DyeColor? Accessory3Color { get; set; }

    [InspectProperty] public DyeColor? OvercoatColor { get; set; }

    [InspectProperty(ShowHex = true)] public ushort? OvercoatSprite { get; set; }

    [InspectSection("Visibility", IsExpandedHandler = nameof(IsHumanForm))]
    [InspectProperty]
    public bool IsTransparent { get; set; }

    [InspectProperty] public bool IsHidden { get; set; }

    [InspectSection("Lantern", IsExpandedHandler = nameof(IsHumanForm))]
    [InspectProperty]
    public LanternSize Lantern { get; set; }

    [InspectSection("Resting", IsExpandedHandler = nameof(IsHumanForm))]
    [InspectProperty]
    public RestPosition? RestPosition { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
        Direction = (WorldDirection)reader.ReadByte();
        EntityId = reader.ReadUInt32();

        var headSprite = reader.ReadUInt16();

        // Displaying as a monster instead
        if (headSprite == 0xFFFF)
        {
            MonsterSprite = reader.ReadUInt16();
            HairColor = (DyeColor)reader.ReadByte();
            BootsColor = (DyeColor)reader.ReadByte();

            // Not sure what these are for
            MonsterUnknown = reader.ReadBytes(6);
            return;
        }
        else
        {
            var bodySprite = reader.ReadByte();
            HeadSprite = headSprite;

            var pantsColor = (byte)(bodySprite % 16);

            if (pantsColor != 0)
            {
                bodySprite -= pantsColor;
                PantsColor = (DyeColor)pantsColor;
            }

            BodySprite = (BodySprite)bodySprite;
            Armor1Sprite = reader.ReadUInt16();

            BootsSprite = reader.ReadByte();
            Armor2Sprite = reader.ReadUInt16();

            ShieldSprite = reader.ReadByte();
            WeaponSprite = reader.ReadUInt16();

            HairColor = (DyeColor)reader.ReadByte();
            BootsColor = (DyeColor)reader.ReadByte();

            Accessory1Color = (DyeColor)reader.ReadByte();
            Accessory1Sprite = reader.ReadUInt16();
            Accessory2Color = (DyeColor)reader.ReadByte();
            Accessory2Sprite = reader.ReadUInt16();
            Accessory3Color = (DyeColor)reader.ReadByte();
            Accessory3Sprite = reader.ReadUInt16();

            Lantern = (LanternSize)reader.ReadByte();
            RestPosition = (RestPosition)reader.ReadByte();

            OvercoatSprite = reader.ReadUInt16();
            OvercoatColor = (DyeColor)reader.ReadByte();

            SkinColor = (SkinColor)reader.ReadByte();
            IsTransparent = reader.ReadBoolean();

            FaceShape = reader.ReadByte();
        }

        NameStyle = (NameTagStyle)reader.ReadByte();
        Name = reader.ReadString8();
        GroupBox = reader.ReadString8();

        if (BodySprite == Game.BodySprite.None && IsTransparent)
        {
            IsHidden = true;
            IsTransparent = false;
        }
    }
    
    private bool IsHumanForm()
    {
        return HeadSprite != 0xFFFF;
    }
    
    private bool IsMonsterForm()
    {
        return HeadSprite == 0xFFFF;
    }
}