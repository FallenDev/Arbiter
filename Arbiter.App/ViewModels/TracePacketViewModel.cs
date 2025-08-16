using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arbiter.App.Extensions;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public partial class TracePacketViewModel(
    NetworkPacket packet,
    NetworkPacket encryptedPacket,
    string? clientName = null)
    : ViewModelBase
{
    [NotifyPropertyChangedFor(nameof(DisplayValue))] [ObservableProperty]
    private PacketDisplayMode _displayMode = PacketDisplayMode.Decrypted;
    
    public string? ClientName => clientName;

    public DateTime Timestamp { get; private init; } = DateTime.Now;
    public NetworkPacket EncryptedPacket { get; } = encryptedPacket;
    public NetworkPacket Packet { get; } = packet;

    public byte? Sequence => Packet switch
    {
        ClientPacket clientPacket => clientPacket.Sequence,
        ServerPacket serverPacket => serverPacket.Sequence,
        _ => null
    };
    
    public IReadOnlyCollection<byte> Payload { get; } = packet.Data;

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
        NetworkPacket decryptedPacket = tracePacket.Direction switch
        {
            PacketDirection.Client => new ClientPacket(tracePacket.Command, tracePacket.Payload, tracePacket.Checksum)
                { Sequence = tracePacket.Sequence },
            PacketDirection.Server => new ServerPacket(tracePacket.Command, tracePacket.Payload)
                { Sequence = tracePacket.Sequence },
            _ => throw new InvalidOperationException("Invalid packet direction")
        };

        NetworkPacket encryptedPacket = tracePacket.Direction switch
        {
            PacketDirection.Client => new ClientPacket(tracePacket.Command,
                    tracePacket.RawPacket.Skip(NetworkPacket.HeaderSize), tracePacket.Checksum)
                { Sequence = tracePacket.Sequence },
            PacketDirection.Server => new ServerPacket(tracePacket.Command,
                    tracePacket.RawPacket.Skip(NetworkPacket.HeaderSize))
                { Sequence = tracePacket.Sequence },
            _ => throw new InvalidOperationException("Invalid packet direction")
        };

        return new TracePacketViewModel(decryptedPacket, encryptedPacket, tracePacket.ClientName)
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

    [RelayCommand]
    private async Task CopyToClipboard()
    {
        var clipboard = Application.Current?.TryGetClipboard();
        if (clipboard is not null)
        {
           await clipboard.SetTextAsync(DisplayValue);
        }
    }
}