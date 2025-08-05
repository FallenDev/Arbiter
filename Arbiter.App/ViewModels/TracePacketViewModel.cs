using System;
using System.Collections.Generic;
using System.Linq;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

public partial class TracePacketViewModel(
    ProxyConnection connection,
    NetworkPacket packet,
    IReadOnlyCollection<byte> payload)
    : ViewModelBase
{
    [NotifyPropertyChangedFor(nameof(DisplayValue))] [ObservableProperty]
    private PacketDisplayMode _displayMode = PacketDisplayMode.Decrypted;
    
    public int ClientId => Connection.Id;
    public string? ClientName => Connection.Name;

    public DateTime Timestamp { get; } = DateTime.Now;

    public ProxyConnection Connection { get; } = connection;
    public NetworkPacket Packet { get; } = packet;
    public IReadOnlyCollection<byte> Payload { get; } = payload;

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
            ClientName = Connection.Name,
            Command = packet.Command,
            RawPacket = Packet.ToList(),
            Payload = Payload,
            Checksum = Packet is ClientPacket clientPacket ? clientPacket.Checksum : null
        };
        return tracePacket;
    }
}