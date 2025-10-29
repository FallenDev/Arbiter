using System;

namespace Arbiter.App.Models.Entities;

[Flags]
public enum EntityFlags
{
    None = 0x00,
    Player = 0x01,
    Monster = 0x02,
    Mundane = 0x04,
    Reactor = 0x08,
    Item = 0x10,
}