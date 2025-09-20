using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Arbiter.App.ViewModels.Client;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class SendPacketViewModel : ViewModelBase
{
    private static readonly char[] NewLineCharacters = ['\r', '\n'];

    private static readonly Regex PacketLineRegex =
        new(@"^(<|>)?([0-9a-f]{2})(?:\s+([0-9a-f]{2}))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly ILogger<SendPacketViewModel> _logger;
    private readonly ClientManagerViewModel _clientManager;

    private readonly List<NetworkPacket> _parsedPackets = [];
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private string _inputText = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private bool _hasClients;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private bool _hasPackets;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private bool _hasError;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private ClientViewModel? _selectedClient;

    [ObservableProperty] private TimeSpan _selectedDelay = TimeSpan.Zero;
    [ObservableProperty] private TimeSpan _selectedRate = TimeSpan.FromMilliseconds(100);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSendCommand))]
    private bool _isSending;

    public ObservableCollection<ClientViewModel> Clients => _clientManager.Clients;

    public ObservableCollection<TimeSpan> AvailableDelays =>
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30)
    ];

    public ObservableCollection<TimeSpan> AvailableRates =>
    [
        TimeSpan.Zero,
        TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(300),
        TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(5)
    ];

    public SendPacketViewModel(ILogger<SendPacketViewModel> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _clientManager = serviceProvider.GetRequiredService<ClientManagerViewModel>();

        _clientManager.Clients.CollectionChanged += OnClientsCollectionChanged;
    }

    private void OnClientsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not ObservableCollection<ClientViewModel> collection)
        {
            return;
        }

        HasClients = collection.Count > 0;

        if (e.Action != NotifyCollectionChangedAction.Remove)
        {
            return;
        }

        // If the currently selected client was removed from the collection, clear the selection
        if (SelectedClient is not null && !collection.Contains(SelectedClient))
        {
            CancelSend();
            SelectedClient = null;
        }
    }

    private bool CanSend() => !IsSending && HasPackets && SelectedClient is not null;

    [RelayCommand(CanExecute = nameof(CanSend))]
    public void StartSend()
    {
        if (IsSending || SelectedClient is null)
        {
            return;
        }

        IsSending = true;
    }

    private bool CanCancelSend() => IsSending;

    [RelayCommand(CanExecute = nameof(CanCancelSend))]
    public void CancelSend()
    {
        if (!IsSending)
        {
            return;
        }

        _cancellationTokenSource.Cancel();
        IsSending = false;
    }

    partial void OnInputTextChanged(string value)
    {
        var lines = value.Split(NewLineCharacters, StringSplitOptions.RemoveEmptyEntries);
        ParsePackets(lines);
    }

    private void ParsePackets(IReadOnlyList<string> lines)
    {
        _parsedPackets.Clear();
        HasPackets = false;
        
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var match = PacketLineRegex.Match(line.Trim());
            if (!match.Success)
            {
                HasError = true;
                throw new DataValidationException($"Invalid packet on line {i + 1}");
            }

            try
            {
                var caret = match.Groups[1].Value;
                var command = byte.Parse(match.Groups[2].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                var data = match.Groups[3].Captures.Select(x =>
                    byte.Parse(x.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture));

                NetworkPacket packet = caret switch
                {
                    "<" => new ServerPacket(command, data),
                    _ => new ClientPacket(command, data)
                };

                _parsedPackets.Add(packet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing packet on line {Line}", i + 1);
                HasError = true;
            }
        }

        HasError = false;
        HasPackets = _parsedPackets.Count > 0;
    }
}