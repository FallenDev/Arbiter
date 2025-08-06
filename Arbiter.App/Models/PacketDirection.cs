using System;

namespace Arbiter.App.Models;

[Flags]
public enum PacketDirection
{
    None = 0x00,
    Client = 0x01,
    Server = 0x02,
    Both = Client | Server
}