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
    byte? sequence = null,
    string? clientName = null)
    : ViewModelBase
{
    [NotifyPropertyChangedFor(nameof(DisplayValue))] [ObservableProperty]
    private PacketDisplayMode _displayMode = PacketDisplayMode.Decrypted;
    
    public string? ClientName => clientName;

    public DateTime Timestamp { get; private init; } = DateTime.Now;
    public NetworkPacket Packet { get; } = packet;
    public byte? Sequence { get; } = sequence;
    public IReadOnlyCollection<byte> Payload { get; } = payload;

    public string CommandName => GetCommandName(Packet);
    public string DisplayValue => DisplayMode switch
    {
        PacketDisplayMode.Decrypted => FormattedPayload,
        _ => FormattedPacket
    };
    
    public bool IsClient => Packet is ClientPacket;
    public bool IsServer => Packet is ServerPacket;

    public string FormattedPacket => string.Join(' ', Packet.Select(x => x.ToString("X2")));
    public string FormattedPayload => string.Join(' ', Payload.Select(x => x.ToString("X2")));

    public TracePacket ToTracePacket()
    {
        var tracePacket = new TracePacket
        {
            Timestamp = Timestamp,
            Direction = Packet is ClientPacket ? PacketDirection.Client : PacketDirection.Server,
            ClientName = ClientName,
            Command = Packet.Command,
            Sequence = Sequence,
            RawPacket = Packet.ToList(),
            Payload = Payload,
            Checksum = Packet switch
            {
                ClientPacket clientPacket => clientPacket.Checksum,
                _ => null
            }
        };
        return tracePacket;
    }

    public static TracePacketViewModel FromTracePacket(TracePacket tracePacket, PacketDisplayMode displayMode)
    {
        var rawPayload = tracePacket.RawPacket.Skip(NetworkPacket.HeaderSize);

        NetworkPacket rawPacket = tracePacket.Direction switch
        {
            PacketDirection.Client => new ClientPacket(tracePacket.Command, rawPayload, tracePacket.Checksum),
            PacketDirection.Server => new ServerPacket(tracePacket.Command, rawPayload),
            _ => throw new InvalidOperationException("Invalid packet direction")
        };

        return new TracePacketViewModel(rawPacket, tracePacket.Payload, tracePacket.Sequence, tracePacket.ClientName)
        {
            Timestamp = tracePacket.Timestamp,
            DisplayMode = displayMode,
        };
    }

    private static string GetCommandName(NetworkPacket packet)
    {
        return packet switch
        {
            ClientPacket clientPacket => $"{clientPacket.Command}",
            ServerPacket serverPacket => $"{serverPacket.Command}",
            _ => $"Unknown {packet.Command:X2}"
        };
    }
}