using System;

namespace Arbiter.App.Models.Tracing;

[Flags]
public enum PacketDirection
{
    Auto = 0x00,
    Client = 0x01,
    Server = 0x02,
    Both = Client | Server
}