using System;

namespace Arbiter.App.Models.Server;

[Flags]
public enum ServerUpdateStatsFlags : byte
{
    None = 0x0,
    UnreadMail = 0x1,
    Unknown = 0x2,
    Modifiers = 0x4,
    ExperienceGold = 0x8,
    Vitals = 0x10,
    Stats = 0x20,
    GameMasterA = 0x40,
    GameMasterB = 0x80,
    Swimming = GameMasterA | GameMasterB,
    Full = Stats | Vitals | ExperienceGold | Modifiers
}