using System;
using System.Linq;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Filters;
using Arbiter.Net.Server;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TracePacketViewModel(
    NetworkPacket encrypted,
    NetworkPacket decrypted,
    NetworkFilterResult? filterResult,
    string? clientName = null)
    : ViewModelBase
{

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DisplayValue))]
    private PacketDisplayMode _displayMode = PacketDisplayMode.Decrypted;

    [ObservableProperty] private double _opacity = 1;

    [ObservableProperty] private string? _clientName = clientName;

    [ObservableProperty]
    private PacketDirection _direction = decrypted is ClientPacket ? PacketDirection.Client : PacketDirection.Server;

    [ObservableProperty] private byte _command = encrypted.Command;

    [ObservableProperty] private string _commandName = GetCommandName(encrypted);

    [ObservableProperty] private byte? _sequence = encrypted switch
    {
        ClientPacket clientPacket => clientPacket.Sequence,
        ServerPacket serverPacket => serverPacket.Sequence,
        _ => null
    };

    [ObservableProperty]
    private uint? _checksum = encrypted is ClientPacket clientPacket ? clientPacket.Checksum : null;

    [ObservableProperty] private string _formattedEncrypted = string.Join(' ', encrypted.Select(x => x.ToString("X2")));

    [ObservableProperty]
    private string _formattedDecrypted = string.Join(' ', decrypted.Data.Select(x => x.ToString("X2")));

    [ObservableProperty] private string? _formattedFiltered = filterResult?.Output is not null
        ? string.Join(' ', filterResult.Output.Select(x => x.ToString("X2")))
        : null;

    public DateTime Timestamp { get; private init; } = DateTime.Now;
    public NetworkPacket EncryptedPacket { get; } = encrypted;
    public NetworkPacket DecryptedPacket { get; } = decrypted;

    public NetworkPacket? FilteredPacket { get; } = filterResult?.Output;
    public NetworkFilterAction FilterAction { get; } = filterResult?.Action ?? NetworkFilterAction.Allow;
    public bool WasBlocked => FilterAction == NetworkFilterAction.Block;
    public bool WasReplaced => FilterAction == NetworkFilterAction.Replace;

    public string DisplayValue => DisplayMode switch
    {
        PacketDisplayMode.Decrypted => FormattedDecrypted,
        _ => FormattedEncrypted
    };

    public bool IsClient => DecryptedPacket is ClientPacket;
    public bool IsServer => DecryptedPacket is ServerPacket;

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
            Payload = DecryptedPacket.Data,
            FilterAction = FilterAction,
            FilteredPayload = FilterAction switch
            {
                // Only need to store the filtered payload if we're replacing it
                NetworkFilterAction.Replace => FilteredPacket?.Data,
                _ => null
            },
            Checksum = Checksum
        };
        return tracePacket;
    }

    public static TracePacketViewModel FromTracePacket(TracePacket tracePacket, PacketDisplayMode displayMode)
    {
        var command = tracePacket.Command;
        var encryptedPayload = tracePacket.RawData.Skip(4);
        var decryptedPayload = tracePacket.Payload;
        var filteredPayload = tracePacket.FilteredPayload;

        NetworkPacket decryptedPacket = tracePacket.Direction switch
        {
            PacketDirection.Client => new ClientPacket(command, decryptedPayload, tracePacket.Checksum)
                { Sequence = tracePacket.Sequence },
            PacketDirection.Server => new ServerPacket(command, decryptedPayload)
                { Sequence = tracePacket.Sequence },
            _ => throw new InvalidOperationException("Invalid packet direction")
        };

        NetworkPacket? filteredPacket = null;
        if (filteredPayload is not null)
        {
            filteredPacket = tracePacket.Direction switch
            {
                PacketDirection.Client => new ClientPacket(command, filteredPayload)
                    { Sequence = tracePacket.Sequence },
                PacketDirection.Server => new ServerPacket(command, filteredPayload)
                    { Sequence = tracePacket.Sequence },
                _ => throw new InvalidOperationException("Invalid packet direction")
            };
        }

        NetworkPacket encryptedPacket = tracePacket.Direction switch
        {
            PacketDirection.Client => new ClientPacket(command, encryptedPayload, tracePacket.Checksum)
            {
                Sequence = tracePacket.Sequence
            },
            PacketDirection.Server => new ServerPacket(command, encryptedPayload)
            {
                Sequence = tracePacket.Sequence
            },
            _ => throw new InvalidOperationException("Invalid packet direction")
        };

        var filterResult = new NetworkFilterResult
        {
            Timestamp = tracePacket.Timestamp,
            Action = tracePacket.FilterAction,
            Input = encryptedPacket,
            Output = filteredPacket
        };

        return new TracePacketViewModel(encryptedPacket, decryptedPacket, filterResult, tracePacket.ClientName)
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