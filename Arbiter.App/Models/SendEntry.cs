using System;
using Arbiter.Net;

namespace Arbiter.App.Models;

internal readonly struct SendEntry
{
    public static SendEntry Disconnect => new() { IsDisconnect = true };
        
    public SendEntry(NetworkPacket packet)
    {
        Packet = packet;
        Wait = null;
    }

    public SendEntry(TimeSpan wait)
    {
        Wait = wait;
        Packet = null;
    }

    public bool IsDisconnect { get; init; }
    public NetworkPacket? Packet { get; }
    public TimeSpan? Wait { get; }
    public bool IsWait => Wait.HasValue;
}