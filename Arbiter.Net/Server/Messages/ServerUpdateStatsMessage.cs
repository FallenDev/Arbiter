using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.UpdateStats)]
public class ServerUpdateStatsMessage : ServerMessage
{
    public StatsFieldFlags Fields { get; set; }

    public bool IsAdmin { get; set; }
    public bool IsSwimming { get; set; }

    public byte? Level { get; set; }
    public byte? AbilityLevel { get; set; }

    public uint? Health { get; set; }
    public uint? MaxHealth { get; set; }
    public uint? Mana { get; set; }
    public uint? MaxMana { get; set; }

    public byte? Strength { get; set; }
    public byte? Intelligence { get; set; }
    public byte? Wisdom { get; set; }
    public byte? Constitution { get; set; }
    public byte? Dexterity { get; set; }
    public bool? HasStatPoints { get; set; }
    public byte? StatPoints { get; set; }
    
    public sbyte? ArmorClass { get; set; }
    public byte? MagicResist { get; set; }
    public byte? DamageModifier { get; set; }
    public byte? HitModifier { get; set; }
    public ElementModifier? AttackElement { get; set; }
    public ElementModifier? DefenseElement { get; set; }

    public uint? TotalExperience { get; set; }
    public uint? ToNextLevel { get; set; }
    public uint? TotalAbility { get; set; }
    public uint? ToNextAbility { get; set; }

    public uint? Gold { get; set; }
    public uint? GamePoints { get; set; }

    public ushort? Weight { get; set; }
    public ushort? MaxWeight { get; set; }

    public bool? IsBlinded { get; set; }
    public bool? CanMove { get; set; }

    public bool? HasUnreadMail { get; set; }
    public bool? HasParcelsAvailable { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Fields = (StatsFieldFlags)reader.ReadByte();

        IsAdmin = Fields.HasFlag(StatsFieldFlags.GameMasterA) &&
                  !Fields.HasFlag(StatsFieldFlags.GameMasterB);
        IsSwimming = Fields.HasFlag(StatsFieldFlags.Swimming);

        if (Fields.HasFlag(StatsFieldFlags.Stats))
        {
            reader.Skip(3); // unknown bytes, always 0x01, 0x00, 0x00
            Level = reader.ReadByte();
            AbilityLevel = reader.ReadByte();
            MaxHealth = reader.ReadUInt32();
            MaxMana = reader.ReadUInt32();
            Strength = reader.ReadByte();
            Intelligence = reader.ReadByte();
            Wisdom = reader.ReadByte();
            Constitution = reader.ReadByte();
            Dexterity = reader.ReadByte();
            HasStatPoints = reader.ReadBoolean();
            StatPoints = reader.ReadByte();
            MaxWeight = reader.ReadUInt16();
            Weight = reader.ReadUInt16();

            // unknown bytes always 0x42 0x00 0x88 0x2E
            reader.Skip(4);
        }

        if (Fields.HasFlag(StatsFieldFlags.Vitals))
        {
            Health = reader.ReadUInt32();
            Mana = reader.ReadUInt32();
        }

        if (Fields.HasFlag(StatsFieldFlags.ExperienceGold))
        {
            TotalExperience = reader.ReadUInt32();
            ToNextLevel = reader.ReadUInt32();
            TotalAbility = reader.ReadUInt32();
            ToNextAbility = reader.ReadUInt32();
            GamePoints = reader.ReadUInt32();
            Gold = reader.ReadUInt32();
        }

        if (Fields.HasFlag(StatsFieldFlags.Modifiers))
        {
            reader.Skip(1); // unknown byte 0x00
            IsBlinded = reader.ReadByte() == 0x08;
            reader.Skip(3); // unknown bytes 0x00 0x00 0x00

            var mailFlags = (MailFlags)reader.ReadByte();
            HasUnreadMail = mailFlags.HasFlag(MailFlags.Mail);
            HasParcelsAvailable = mailFlags.HasFlag(MailFlags.Parcel);

            AttackElement = (ElementModifier)reader.ReadByte();
            DefenseElement = (ElementModifier)reader.ReadByte();
            MagicResist = reader.ReadByte();
            CanMove = reader.ReadBoolean();
            ArmorClass = reader.ReadSByte();
            DamageModifier = reader.ReadByte();
            HitModifier = reader.ReadByte();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}