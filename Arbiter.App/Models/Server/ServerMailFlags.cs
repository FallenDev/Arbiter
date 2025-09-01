using System;

namespace Arbiter.App.Models.Server;

[Flags]
public enum ServerMailFlags
{
    None = 0x0,
    Parcel = 0x01,
    Mail = 0x10
}