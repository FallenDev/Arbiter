using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Arbiter.App.Extensions;
using Arbiter.App.ViewModels.Client;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class SendPacketViewModel : ViewModelBase
{
    private static readonly char[] NewLineCharacters = ['\r', '\n'];

    private static readonly Regex PacketLineRegex =
        new(@"^(<|>)?\s*([0-9a-f]{1,2})(?:\s+([0-9a-f]{1,2}))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly ILogger<SendPacketViewModel> _logger;
    private readonly ClientManagerViewModel _clientManager;

    private readonly List<NetworkPacket> _parsedPackets = [];
    private CancellationTokenSource? _cancellationTokenSource;
    
    private string _inputText = string.Empty;

    public string InputText
    {
        get => _inputText;
        set
        {
            if (!SetProperty(ref _inputText, value))
            {
                return;
            }
            OnPropertyChanged(nameof(IsEmpty));
            
            var lines = value.Split(NewLineCharacters, StringSplitOptions.RemoveEmptyEntries);
            ValidationError = !TryParsePackets(lines, out var validationError) ? validationError : null;
            ValidationErrorOpacity = !string.IsNullOrWhiteSpace(ValidationError) ? 1 : 0;
            
            StartSendCommand.NotifyCanExecuteChanged();
            ClearAllCommand.NotifyCanExecuteChanged();

            if (validationError is not null)
            {
                throw new ValidationException(validationError);
            }
        }
    }
    
    public bool IsEmpty => string.IsNullOrWhiteSpace(InputText);
    
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private bool _hasClients;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private bool _hasPackets;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private string? _validationError;

    [ObservableProperty] private double? _validationErrorOpacity;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private ClientViewModel? _selectedClient;

    [ObservableProperty] private TimeSpan _selectedDelay = TimeSpan.Zero;
    [ObservableProperty] private TimeSpan _selectedRate = TimeSpan.FromMilliseconds(100);
    [ObservableProperty] private bool _repeatEnabled;
    [ObservableProperty] private int _repeatCount;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSendCommand))]
    private bool _isSending;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CopyToClipboardCommand))]
    private int _selectionStart;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CopyToClipboardCommand))]
    private int _selectionEnd;

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
        TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(600),
        TimeSpan.FromMilliseconds(700), TimeSpan.FromMilliseconds(800), TimeSpan.FromMilliseconds(900),
        TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(4),
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

        // Create a fresh CTS for each run
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        _ = RunSendLoopAsync(_cancellationTokenSource.Token);
    }

    private bool CanCancelSend() => IsSending;

    [RelayCommand(CanExecute = nameof(CanCancelSend))]
    public void CancelSend()
    {
        if (!IsSending)
        {
            return;
        }

        try
        {
            _cancellationTokenSource?.Cancel();
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            IsSending = false;
        }
    }

    private async Task RunSendLoopAsync(CancellationToken token)
    {
        // Add 1 since we are technically sending N+1 times
        int? repeatCount = RepeatEnabled
            ? RepeatCount >= 0 ? RepeatCount+1 : null
            : 0;
        
        var packetsToSend = _parsedPackets.ToList();

        var initialDelay = SelectedDelay;
        var interval = SelectedRate;
        
        try
        {
            var client = SelectedClient;
            if (client is null || packetsToSend.Count == 0)
            {
                return;
            }

            if (initialDelay > TimeSpan.Zero)
            {
                await Task.Delay(initialDelay, token).ConfigureAwait(false);
            }

            do
            {
                for (var i = 0; i < packetsToSend.Count; i++)
                {
                    token.ThrowIfCancellationRequested();

                    var packet = packetsToSend[i];
                    var queued = client.EnqueuePacket(packet);
                    if (!queued)
                    {
                        _logger.LogWarning("[{ClientName}] Failed to enqueue packet {Index}",
                            client.Name ?? client.Id.ToString(), i + 1);
                    }

                    var hasMore = i < packetsToSend.Count - 1;
                    if (interval > TimeSpan.Zero && (hasMore || repeatCount is not 0))
                    {
                        await Task.Delay(interval, token).ConfigureAwait(false);
                    }
                }

                // Decrement the repeat count
                if (repeatCount is > 0)
                {
                    repeatCount--;
                }

                // Add a small delay between iterations to prevent busy loop
                await Task.Delay(1, token).ConfigureAwait(false);
            }
            while (!token.IsCancellationRequested && repeatCount is not 0);
        }
        catch (OperationCanceledException)
        {
            // Do nothing
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending packets");
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            // Notify the UI that we are done sending
            Dispatcher.UIThread.Post(() => { IsSending = false; }, DispatcherPriority.Background);
        }
    }

    private bool TryParsePackets(IReadOnlyList<string> lines, out string? validationError)
    {
        validationError = null;

        _parsedPackets.Clear();
        HasPackets = false;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // Allow comments
            if (line.StartsWith('#') || line.StartsWith("//"))
            {
                continue;
            }

            var match = PacketLineRegex.Match(line.Trim());
            if (!match.Success)
            {
                validationError = $"Invalid packet format on line {i + 1}";
                return false;
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
                validationError = $"Invalid packet on line {i + 1}";
                return false;
            }
        }

        HasPackets = _parsedPackets.Count > 0;
        return true;
    }

    private bool CanCopyToClipboard(string fieldName) => fieldName switch
    {
        "selection" => Math.Abs(SelectionEnd - SelectionStart) > 0,
        _ => true
    };

    [RelayCommand(CanExecute = nameof(CanCopyToClipboard))]
    private async Task CopyToClipboard(string fieldName)
    {
        var clipboard = Application.Current?.TryGetClipboard();
        if (clipboard is null)
        {
            return;
        }

        // Ensure that the selection is not reversed
        var selectionStart = Math.Min(SelectionStart, SelectionEnd);
        var selectionEnd = Math.Max(SelectionStart, SelectionEnd);

        var textToCopy = fieldName switch
        {
            "selection" => InputText.Substring(selectionStart, selectionEnd - selectionStart),
            _ => InputText
        };

        if (!string.IsNullOrEmpty(textToCopy))
        {
            await clipboard.SetTextAsync(textToCopy);
        }
    }

    [RelayCommand]
    private async Task PasteFromClipboard()
    {
        var clipboard = Application.Current?.TryGetClipboard();
        if (clipboard is null)
        {
            return;
        }

        var newText = await clipboard.GetTextAsync();
        if (!string.IsNullOrWhiteSpace(newText))
        {
            InputText = newText;
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputText = string.Empty;
    }
}