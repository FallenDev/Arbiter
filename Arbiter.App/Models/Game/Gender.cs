using System;

namespace Arbiter.App.Models.Game;

[Flags]
public enum Gender : byte
{
    None = 0,
    Male = 1,
    Female = 2,
    Unisex = Male | Female
}