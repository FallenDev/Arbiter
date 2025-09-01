using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.UpdateStats)]
public class ServerUpdateStatsMessage : IPacketMessage
{
    [InspectSection("Flags")]
    [InspectProperty]
    public ServerUpdateStatsFlags UpdateType { get; set; }

    [InspectProperty] public bool IsAdmin { get; set; }

    [InspectProperty] public bool IsSwimming { get; set; }

    [InspectSection("Level", IsExpandedHandler = nameof(ShouldExpandLevel))]
    [InspectProperty]
    public byte? Level { get; set; }

    [InspectProperty] public byte? AbilityLevel { get; set; }

    [InspectSection("Vitals", IsExpandedHandler = nameof(ShouldExpandVitals))]
    [InspectProperty]
    public uint? Health { get; set; }

    [InspectProperty] public uint? MaxHealth { get; set; }

    [InspectProperty] public uint? Mana { get; set; }

    [InspectProperty] public uint? MaxMana { get; set; }

    [InspectSection("Stats", IsExpandedHandler = nameof(ShouldExpandStats))]
    [InspectProperty]
    public byte? Strength { get; set; }

    [InspectProperty] public byte? Intelligence { get; set; }

    [InspectProperty] public byte? Wisdom { get; set; }

    [InspectProperty] public byte? Constitution { get; set; }

    [InspectProperty] public byte? Dexterity { get; set; }

    [InspectProperty] public bool? HasStatPoints { get; set; }

    [InspectProperty] public byte? StatPoints { get; set; }

    [InspectSection("Modifiers", IsExpandedHandler = nameof(ShouldExpandModifiers))]
    [InspectProperty]
    public sbyte? ArmorClass { get; set; }

    [InspectProperty] public byte? MagicResist { get; set; }

    [InspectProperty] public byte? DamageModifier { get; set; }

    [InspectProperty] public byte? HitModifier { get; set; }

    [InspectProperty] public ElementModifier? AttackElement { get; set; }

    [InspectProperty] public ElementModifier? DefenseElement { get; set; }

    [InspectSection("Experience", IsExpandedHandler = nameof(ShouldExpandExperience))]
    [InspectProperty]
    public uint? TotalExperience { get; set; }

    [InspectProperty] public uint? ToNextLevel { get; set; }

    [InspectProperty] public uint? TotalAbility { get; set; }

    [InspectProperty] public uint? ToNextAbility { get; set; }

    [InspectSection("Currency", IsExpandedHandler = nameof(ShouldExpandCurrency))]
    [InspectProperty]
    public uint? Gold { get; set; }

    [InspectProperty] public uint? GamePoints { get; set; }

    [InspectSection("Weight", IsExpandedHandler = nameof(ShouldExpandWeight))]
    [InspectProperty]
    public short? Weight { get; set; }

    [InspectProperty] public short? MaxWeight { get; set; }

    [InspectSection("Status", IsExpandedHandler = nameof(ShouldExpandStatus))]
    [InspectProperty]
    public bool? IsBlinded { get; set; }

    [InspectProperty] public bool? CanMove { get; set; }

    [InspectSection("Mail", IsExpandedHandler = nameof(ShouldExpandMail))]
    [InspectProperty]
    public bool? HasUnreadMail { get; set; }

    [InspectProperty] public bool? HasUnreadParcels { get; set; }

    [InspectSection("Uncategorized")]
    [InspectProperty(ShowHex = true)]
    public uint? Unknown { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        UpdateType = (ServerUpdateStatsFlags)reader.ReadByte();

        IsAdmin = UpdateType.HasFlag(ServerUpdateStatsFlags.GameMasterA) &&
                  !UpdateType.HasFlag(ServerUpdateStatsFlags.GameMasterB);
        IsSwimming = UpdateType.HasFlag(ServerUpdateStatsFlags.Swimming);

        if (UpdateType.HasFlag(ServerUpdateStatsFlags.Stats))
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
            MaxWeight = reader.ReadInt16();
            Weight = reader.ReadInt16();

            // unknown bytes always 0x42 0x00 0x88 0x2E
            Unknown = reader.ReadUInt32();
        }

        if (UpdateType.HasFlag(ServerUpdateStatsFlags.Vitals))
        {
            Health = reader.ReadUInt32();
            Mana = reader.ReadUInt32();
        }

        if (UpdateType.HasFlag(ServerUpdateStatsFlags.ExperienceGold))
        {
            TotalExperience = reader.ReadUInt32();
            ToNextLevel = reader.ReadUInt32();
            TotalAbility = reader.ReadUInt32();
            ToNextAbility = reader.ReadUInt32();
            GamePoints = reader.ReadUInt32();
            Gold = reader.ReadUInt32();
        }

        if (UpdateType.HasFlag(ServerUpdateStatsFlags.Modifiers))
        {
            reader.Skip(1); // unknown byte 0x00
            IsBlinded = reader.ReadByte() == 0x08;
            reader.Skip(3); // unknown bytes 0x00 0x00 0x00

            var mailFlags = (ServerMailFlags)reader.ReadByte();
            HasUnreadMail = mailFlags.HasFlag(ServerMailFlags.Mail);
            HasUnreadParcels = mailFlags.HasFlag(ServerMailFlags.Parcel);

            AttackElement = (ElementModifier)reader.ReadByte();
            DefenseElement = (ElementModifier)reader.ReadByte();
            MagicResist = reader.ReadByte();
            CanMove = reader.ReadBoolean();
            ArmorClass = reader.ReadSByte();
            DamageModifier = reader.ReadByte();
            HitModifier = reader.ReadByte();
        }
    }

    // Determines whether each section is automatically expanded or not
    private bool ShouldExpandLevel() => UpdateType.HasFlag(ServerUpdateStatsFlags.Stats);

    private bool ShouldExpandVitals() => UpdateType.HasFlag(ServerUpdateStatsFlags.Stats) ||
                                         UpdateType.HasFlag(ServerUpdateStatsFlags.Vitals);

    private bool ShouldExpandStats() => UpdateType.HasFlag(ServerUpdateStatsFlags.Stats);
    private bool ShouldExpandModifiers() => UpdateType.HasFlag(ServerUpdateStatsFlags.Modifiers);
    private bool ShouldExpandExperience() => UpdateType.HasFlag(ServerUpdateStatsFlags.ExperienceGold);
    private bool ShouldExpandCurrency() => UpdateType.HasFlag(ServerUpdateStatsFlags.ExperienceGold);
    private bool ShouldExpandWeight() => UpdateType.HasFlag(ServerUpdateStatsFlags.Stats);
    private bool ShouldExpandStatus() => UpdateType.HasFlag(ServerUpdateStatsFlags.Modifiers);
    private bool ShouldExpandMail() => UpdateType.HasFlag(ServerUpdateStatsFlags.Modifiers);
}