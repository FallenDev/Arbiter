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
using Arbiter.App.Threading;
using Arbiter.App.ViewModels.Client;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using Avalonia;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class SendPacketViewModel : ViewModelBase
{
    private readonly struct SendItem
    {
        public static SendItem Disconnect => new() { IsDisconnect = true };
        
        public SendItem(NetworkPacket packet)
        {
            Packet = packet;
            Wait = null;
        }

        public SendItem(TimeSpan wait)
        {
            Wait = wait;
            Packet = null;
        }

        public bool IsDisconnect { get; init; }
        public NetworkPacket? Packet { get; }
        public TimeSpan? Wait { get; }
        public bool IsWait => Wait.HasValue;
    }

    private static readonly char[] NewLineCharacters = ['\r', '\n'];

    private static readonly Regex PacketLineRegex =
        new(@"^(<|>)?\s*([0-9a-f]{1,2})(?:\s+([0-9a-f]{1,2}))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex WaitLineRegex =
        new(@"^@wait\s+(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex DisconnectLineRegex =
        new(@"^@(disconnect|dc)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private readonly ILogger<SendPacketViewModel> _logger;
    private readonly ClientManagerViewModel _clientManager;

    private readonly List<SendItem> _parsedItems = [];
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly Debouncer _validationDebouncer = new(TimeSpan.FromMilliseconds(300), Dispatcher.UIThread);

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

            // Debounce validation when the input changes
            _validationDebouncer.Execute(PerformValidation);

            StartSendCommand.NotifyCanExecuteChanged();
            ClearAllCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(InputText);

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private bool _hasClients;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private bool _hasPackets;
    
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartSendCommand))]
    private bool _hasDisconnects;

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


    private void PerformValidation()
    {
        var lines = InputText.Split(NewLineCharacters, StringSplitOptions.RemoveEmptyEntries);
        var success = TryParseSendItems(lines, out var items, out var validationError);
        ValidationError = success ? null : validationError;
        ValidationErrorOpacity = !string.IsNullOrWhiteSpace(ValidationError) ? 1 : 0;
        
        HasPackets = success && items.Any(x => x.Packet is not null);
        HasDisconnects = success && items.Any(x => x.IsDisconnect);
        
        StartSendCommand.NotifyCanExecuteChanged();

        if (validationError is not null)
        {
            throw new ValidationException(validationError);
        }
    }

    private bool CanSend() => !IsSending && (HasPackets || HasDisconnects )&& SelectedClient is not null && HasValidInput();

    private bool HasValidInput()
    {
        // For immediate validation check (used by send command), parse without debouncing
        var lines = InputText.Split(NewLineCharacters, StringSplitOptions.RemoveEmptyEntries);
        return TryParseSendItems(lines, out _, out _);
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    public void StartSend()
    {
        if (IsSending || SelectedClient is null)
        {
            return;
        }

        // Perform immediate validation before sending
        var lines = InputText.Split(NewLineCharacters, StringSplitOptions.RemoveEmptyEntries);
        if (!TryParseSendItems(lines, out var items, out var validationError))
        {
            // Update UI to show the validation error immediately
            ValidationError = validationError;
            ValidationErrorOpacity = 1;
            StartSendCommand.NotifyCanExecuteChanged();
            return;
        }

        // Commit parsed items for sending
        _parsedItems.Clear();
        _parsedItems.AddRange(items);
        
        HasPackets = _parsedItems.Any(x => x.Packet is not null);
        HasDisconnects = _parsedItems.Any(x => x.IsDisconnect);
        
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
            ? RepeatCount >= 0 ? RepeatCount + 1 : null
            : 0;

        var sendItems = _parsedItems.ToList();

        var initialDelay = SelectedDelay;
        var interval = SelectedRate;

        try
        {
            var client = SelectedClient;
            if (client is null || sendItems.Count == 0)
            {
                return;
            }

            if (initialDelay > TimeSpan.Zero)
            {
                await Task.Delay(initialDelay, token).ConfigureAwait(false);
            }

            do
            {
                for (var i = 0; i < sendItems.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var item = sendItems[i];

                    // Handle disconnects
                    if (item.IsDisconnect)
                    {
                        client.Disconnect();
                        continue;
                    }
                    
                    // Handle wait delays
                    if (item.IsWait)
                    {
                        var wait = item.Wait!.Value;
                        if (wait > TimeSpan.Zero)
                        {
                            await Task.Delay(wait, token).ConfigureAwait(false);
                        }

                        // Do not apply interval after explicit wait line
                        continue;
                    }

                    if (item.Packet is not { } packet)
                    {
                        continue;
                    }

                    // Handle the packet and send to the client
                    var queued = client.EnqueuePacket(packet);
                    if (!queued)
                    {
                        _logger.LogWarning("[{ClientName}] Failed to enqueue packet {Index}",
                            client.Name ?? client.Id.ToString(), i + 1);
                    }

                    var hasMore = i < sendItems.Count - 1;
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
            } while (!token.IsCancellationRequested && repeatCount is not 0);
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

    // Pure parsing routine: parse lines to send items without mutating state
    private bool TryParseSendItems(IReadOnlyList<string> lines, out List<SendItem> items, out string? validationError)
    {
        validationError = null;
        items = new List<SendItem>();

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var trimmed = line.Trim();

            // Allow comments
            if (trimmed.StartsWith('#') || trimmed.StartsWith("//"))
            {
                continue;
            }

            // Empty command
            if (trimmed == "@")
            {
                continue;
            }

            // Disconnect command
            var disconnectMatch = DisconnectLineRegex.Match(trimmed);
            if (disconnectMatch.Success)
            {
                items.Add(SendItem.Disconnect);
                continue;           
            }
            
            // Wait command
            var waitMatch = WaitLineRegex.Match(trimmed);
            if (waitMatch.Success)
            {
                if (!int.TryParse(waitMatch.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms))
                {
                    validationError = $"Invalid wait value on line {i + 1}";
                    items = [];
                    return false;
                }

                if (ms <= 0)
                {
                    validationError = $"Wait value must be a positive, non-zero integer on line {i + 1}";
                    items = [];
                    return false;
                }

                items.Add(new SendItem(TimeSpan.FromMilliseconds(ms)));
                continue;
            }

            if (trimmed.StartsWith('@'))
            {
                validationError = $"Invalid command syntax on line {i + 1}";
                items = [];
                return false;
            }

            var match = PacketLineRegex.Match(trimmed);
            if (!match.Success)
            {
                validationError = $"Invalid packet format on line {i + 1}";
                items = [];
                return false;
            }

            try
            {
                var caret = match.Groups[1].Value;
                var command = byte.Parse(match.Groups[2].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                var data = match.Groups[3].Captures.Select(x => byte.Parse(x.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture));

                NetworkPacket packet = caret switch
                {
                    "<" => new ServerPacket(command, data),
                    _ => new ClientPacket(command, data)
                };

                items.Add(new SendItem(packet));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing packet on line {Line}", i + 1);
                validationError = $"Invalid packet on line {i + 1}";
                items = [];
                return false;
            }
        }

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