using System;

namespace Arbiter.App.Models.Game;

[Flags]
public enum EntityTypeFlags : byte
{
    None = 0,
    Creature = 0x1,
    Item = 0x2,
    Aisling = 0x4
}