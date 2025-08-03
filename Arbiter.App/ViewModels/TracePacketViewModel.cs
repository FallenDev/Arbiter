using System;
using Arbiter.Net;

namespace Arbiter.App.ViewModels;

public class TracePacketViewModel(ProxyConnection connection, NetworkPacket packet)
    : ViewModelBase
{
    public DateTime Timestamp { get; } = DateTime.Now;
    
    public ProxyConnection Connection { get; } = connection;
    public NetworkPacket Packet { get; } = packet;
}