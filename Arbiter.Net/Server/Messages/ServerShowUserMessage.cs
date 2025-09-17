using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.ShowUser)]
public class ServerShowUserMessage : ServerMessage
{
    public ushort X { get; set; }
    public ushort Y { get; set; }
    public WorldDirection Direction { get; set; }
    public uint EntityId { get; set; }

    public ushort HeadSprite { get; set; }
    public byte FaceShape { get; set; }
    public DyeColor? HairColor { get; set; }
    public BodySprite? BodySprite { get; set; }
    public SkinColor? SkinColor { get; set; }
    public bool IsTranslucent { get; set; }
    public bool IsHidden { get; set; }

    public ushort? MonsterSprite { get; set; }
    public IReadOnlyList<byte>? MonsterUnknown { get; set; }

    public ushort? Armor1Sprite { get; set; }
    public ushort? Armor2Sprite { get; set; }
    public DyeColor? PantsColor { get; set; }
    public byte? BootsSprite { get; set; }
    public DyeColor? BootsColor { get; set; }
    public ushort? WeaponSprite { get; set; }
    public byte? ShieldSprite { get; set; }
    public ushort? Accessory1Sprite { get; set; }
    public DyeColor? Accessory1Color { get; set; }
    public ushort? Accessory2Sprite { get; set; }
    public DyeColor? Accessory2Color { get; set; }
    public ushort? Accessory3Sprite { get; set; }
    public DyeColor? Accessory3Color { get; set; }
    public DyeColor? OvercoatColor { get; set; }
    public ushort? OvercoatSprite { get; set; }

    public LanternSize Lantern { get; set; }
    public RestPosition? RestPosition { get; set; }

    public NameTagStyle NameStyle { get; set; }
    public string? Name { get; set; }
    public string? GroupBox { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
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
            IsTranslucent = reader.ReadBoolean();

            FaceShape = reader.ReadByte();
        }

        NameStyle = (NameTagStyle)reader.ReadByte();
        Name = reader.ReadString8();
        GroupBox = reader.ReadString8();

        if (BodySprite == Types.BodySprite.None && IsTranslucent)
        {
            IsHidden = true;
            IsTranslucent = false;
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}