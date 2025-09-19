using System;
using System.Collections.Generic;
using System.Linq;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TracePacketViewModel(
    NetworkPacket encrypted,
    NetworkPacket decrypted,
    string? clientName = null)
    : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayValue))]
    private PacketDisplayMode _displayMode = PacketDisplayMode.Decrypted;
    
    [ObservableProperty]
    private double _opacity = 1;

    [ObservableProperty] private string? _clientName = clientName;

    [ObservableProperty]
    private PacketDirection _direction = encrypted is ClientPacket ? PacketDirection.Client : PacketDirection.Server;
    
    [ObservableProperty] private byte _command = encrypted.Command;
    
    [ObservableProperty] private string _commandName = GetCommandName(encrypted);

    [ObservableProperty] private byte? _sequence = encrypted switch
    {
        ClientPacket clientPacket => clientPacket.Sequence,
        ServerPacket serverPacket => serverPacket.Sequence,
        _ => null
    };
    
    [ObservableProperty] private uint? _checksum = encrypted is ClientPacket clientPacket ? clientPacket.Checksum : null;
    
    public DateTime Timestamp { get; private init; } = DateTime.Now;
    public NetworkPacket EncryptedPacket { get; } = encrypted;
    public NetworkPacket DecryptedPacket { get; } = decrypted;
    public IReadOnlyList<byte> Payload { get; } = decrypted.ToList();
    
    public string DisplayValue => DisplayMode switch
    {
        PacketDisplayMode.Decrypted => FormattedDecrypted,
        _ => FormattedEncrypted
    };
    
    public bool IsClient => DecryptedPacket is ClientPacket;
    public bool IsServer => DecryptedPacket is ServerPacket;

    public string FormattedEncrypted => string.Join(' ', EncryptedPacket.Select(x => x.ToString("X2")));
    public string FormattedDecrypted => string.Join(' ', DecryptedPacket.Select(x => x.ToString("X2")));

    public TracePacket ToTracePacket()
    {
        var tracePacket = new TracePacket
        {
            Timestamp = Timestamp,
            Direction = Direction,
            ClientName = ClientName,
            Command = Command,
            Sequence = Sequence,
            RawData = EncryptedPacket.ToList(),
            Payload = Payload,
            Checksum = Checksum
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
            PacketDirection.Client => new ClientPacket(tracePacket.Command, tracePacket.RawData, tracePacket.Checksum)
            {
                Sequence = tracePacket.Sequence
            },
            PacketDirection.Server => new ServerPacket(tracePacket.Command, tracePacket.RawData)
            {
                Sequence = tracePacket.Sequence
            },
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
}