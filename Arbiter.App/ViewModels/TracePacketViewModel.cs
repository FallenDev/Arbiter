using System;
using System.Collections.Generic;
using System.Linq;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

public partial class TracePacketViewModel(
    NetworkPacket packet,
    IReadOnlyCollection<byte> payload,
    string? clientName = null)
    : ViewModelBase
{
    [NotifyPropertyChangedFor(nameof(DisplayValue))] [ObservableProperty]
    private PacketDisplayMode _displayMode = PacketDisplayMode.Decrypted;
    
    public string? ClientName => clientName;

    public DateTime Timestamp { get; private init; } = DateTime.Now;
    public NetworkPacket Packet { get; private init; } = packet;
    public IReadOnlyCollection<byte> Payload { get; private init; } = payload;

    public string DisplayValue => DisplayMode switch
    {
        PacketDisplayMode.Decrypted => FormattedPayload,
        _ => FormattedPacket
    };

    public string FormattedPacket => string.Join(' ', Packet.Select(x => x.ToString("X2")));
    public string FormattedPayload => string.Join(' ', Payload.Select(x => x.ToString("X2")));

    public TracePacket ToTracePacket()
    {
        var tracePacket = new TracePacket
        {
            Timestamp = Timestamp,
            Direction = Packet is ClientPacket ? "client" : "server",
            ClientName = ClientName,
            Command = Packet.Command,
            RawPacket = Packet.ToList(),
            Payload = Payload,
            Checksum = Packet is ClientPacket clientPacket ? clientPacket.Checksum : null
        };
        return tracePacket;
    }

    public static TracePacketViewModel FromTracePacket(TracePacket tracePacket, PacketDisplayMode displayMode)
    {
        var rawPayload = tracePacket.RawPacket.Skip(NetworkPacket.HeaderSize);

        NetworkPacket rawPacket = tracePacket.Direction.ToLowerInvariant() switch
        {
            "client" => new ClientPacket(tracePacket.Command, rawPayload, tracePacket.Checksum),
            "server" => new ServerPacket(tracePacket.Command, rawPayload),
            _ => throw new InvalidOperationException("Invalid packet direction")
        };

        return new TracePacketViewModel(rawPacket, tracePacket.Payload, tracePacket.ClientName)
        {
            DisplayMode = displayMode,
        };
    }
}