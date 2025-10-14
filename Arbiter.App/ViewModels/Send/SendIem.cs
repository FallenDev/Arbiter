using System;
using Arbiter.Net;

namespace Arbiter.App.ViewModels.Send;

internal readonly struct SendItem
{
    public static SendItem Disconnect => new() { IsDisconnect = true };
        
    public SendItem(NetworkPacket packet)
    {
        Packet = packet;
        Wait = null;
    }

    public SendItem(TimeSpan wait)
    {
        Wait = wait;
        Packet = null;
    }

    public bool IsDisconnect { get; init; }
    public NetworkPacket? Packet { get; }
    public TimeSpan? Wait { get; }
    public bool IsWait => Wait.HasValue;
}