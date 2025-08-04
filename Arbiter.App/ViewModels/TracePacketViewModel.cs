using System;
using System.Collections.Generic;
using System.Linq;
using Arbiter.Net;

namespace Arbiter.App.ViewModels;

public partial class TracePacketViewModel(ProxyConnection connection, NetworkPacket packet, IReadOnlyCollection<byte> payload)
    : ViewModelBase
{
    public DateTime Timestamp { get; } = DateTime.Now;

    public ProxyConnection Connection { get; } = connection;
    public NetworkPacket Packet { get; } = packet;
    public IReadOnlyCollection<byte> Payload { get; } = payload;

    public string FormattedPacket => string.Join(' ', Packet.Select(x => x.ToString("X2")));
    public string FormattedPayload => string.Join(' ', Payload.Select(x => x.ToString("X2")));
}