using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Arbiter.App.Collections;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.Net.Proxy;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceViewModel : ViewModelBase
{
    private static readonly string TracesDirectory = AppHelper.GetRelativePath("traces");

    private static readonly FilePickerFileType JsonFileType = new("JSON Files")
    {
        Patterns = ["*.json"],
        MimeTypes = ["application/json"],
    };

    private readonly ILogger<TraceViewModel> _logger;
    private readonly IStorageProvider _storageProvider;
    private readonly IKeyboardService _keyboardService;
    private readonly IDialogService _dialogService;
    private readonly ITraceService _traceService;
    private readonly ProxyServer _proxyServer;

    private readonly ConcurrentObservableCollection<TracePacketViewModel> _allPackets = [];

    private bool _isEmpty = true;
    private PacketDisplayMode _packetDisplayMode = PacketDisplayMode.Decrypted;

    [ObservableProperty] private int _maxTraceHistory;
    [ObservableProperty] private DateTime _startTime;
    [ObservableProperty] private bool _scrollToEndRequested;
    [ObservableProperty] private int? _scrollToIndexRequested;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isLive;
    [ObservableProperty] private bool _isDirty;

    [ObservableProperty] private TraceClientViewModel? _selectedTraceClient;

    [ObservableProperty] private string? _traceClientName;

    public ObservableCollection<TraceClientViewModel> TraceClients { get; } = [new("All Clients")];
    
    public bool IsEmpty => _isEmpty;

    public bool ShowRawPackets
    {
        get => _packetDisplayMode == PacketDisplayMode.Raw;
        set
        {
            var newValue = value ? PacketDisplayMode.Raw : PacketDisplayMode.Decrypted;
            if (!SetProperty(ref _packetDisplayMode, newValue))
            {
                return;
            }

            OnPropertyChanged();
            foreach (var packet in _allPackets)
            {
                packet.DisplayMode = newValue;
            }
        }
    }

    public TraceViewModel(ILogger<TraceViewModel> logger, IStorageProvider storageProvider,
        IKeyboardService keyboardService,
        IDialogService dialogService, ITraceService traceService, ProxyServer proxyServer)
    {
        _logger = logger;
        _storageProvider = storageProvider;
        _keyboardService = keyboardService;
        _dialogService = dialogService;
        _traceService = traceService;
        _proxyServer = proxyServer;

        _proxyServer.ClientAuthenticated += OnClientAuthenticated;
        _proxyServer.ClientDisconnected += OnClientDisconnected;

        SelectedTraceClient = TraceClients.FirstOrDefault();
        FilteredPackets = new FilteredObservableCollection<TracePacketViewModel>(_allPackets, MatchesFilter);

        _allPackets.CollectionChanged += OnPacketCollectionChanged;
        SelectedPackets.CollectionChanged += OnSelectedPacketsCollectionChanged;

        FilterParameters.PropertyChanged += OnFilterParametersChanged;
        SearchParameters.PropertyChanged += OnSearchParametersChanged;
    }

    private void OnClientAuthenticated(object? sender, ProxyConnectionEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Connection.Name))
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var client = new TraceClientViewModel(e.Connection.Name, e.Connection.Name);
            TraceClients.Add(client);

            // Re-select the client if it was previously selected (can happen after redirect)
            if (string.Equals(client.Name, TraceClientName, StringComparison.OrdinalIgnoreCase))
            {
                SelectedTraceClient = client;
            }

        }, DispatcherPriority.Background);
    }

    private void OnClientDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Connection.Name))
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            // Remove the client from the list
            var client = TraceClients.FirstOrDefault(c =>
                string.Equals(c.Name, e.Connection.Name, StringComparison.OrdinalIgnoreCase));

            if (client is not null)
            {
                TraceClients.Remove(client);
            }

            // If not running, select "all clients"
            if (!IsRunning)
            {
                SelectedTraceClient = TraceClients.FirstOrDefault();
            }
        }, DispatcherPriority.Background);
    }

    private void OnPacketCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var collection = (ConcurrentObservableCollection<TracePacketViewModel>)sender!;
        if (SetProperty(ref _isEmpty, collection.Count == 0))
        {
            Dispatcher.UIThread.Post(() => OnPropertyChanged(nameof(IsEmpty)), DispatcherPriority.Background);
        }

        // When packets are removed/reset, prune client filters that no longer exist in any remaining packet
        if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset)
        {
            PruneClientsNotInPackets();
        }
    }

    private void OnPacketReceived(object? sender, ProxyConnectionDataEventArgs e)
    {
        // Ignore packets from other clients if a client is selected
        var name = e.Connection.Name;
        if (!string.IsNullOrWhiteSpace(TraceClientName) &&
            !string.Equals(TraceClientName, name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var packetViewModel = new TracePacketViewModel(e.Encrypted, e.Decrypted, e.FilterResult, e.Connection.Name)
            { DisplayMode = _packetDisplayMode };

        AddPacketToTrace(packetViewModel);
    }

    private void OnPacketQueued(object? sender, ProxyConnectionDataEventArgs e)
    {
        // Ignore packets from other clients if a client is selected
        var name = e.Connection.Name;
        if (!string.IsNullOrWhiteSpace(TraceClientName) &&
            !string.Equals(TraceClientName, name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var packetViewModel = new TracePacketViewModel(e.Encrypted, e.Decrypted, e.FilterResult, e.Connection.Name)
            { DisplayMode = _packetDisplayMode };

        AddPacketToTrace(packetViewModel);
    }

    private void AddPacketToTrace(TracePacketViewModel vm, bool pruneHistory = true)
    {
        var matchesSearch = MatchesSearch(vm);
        if (matchesSearch)
        {
            AddSearchResultIndex(_allPackets.Count);
        }

        vm.Opacity = matchesSearch ? 1 : 0.5;

        _allPackets.Add(vm);
        FilterParameters.TryAddClient(vm.ClientName ?? string.Empty);
        IsDirty = true;

        while (pruneHistory && _allPackets.Count > MaxTraceHistory)
        {
            _allPackets.RemoveAt(0);
        }
    }

    private void ClearPackets()
    {
        _allPackets.Clear();
        SelectedPackets.Clear();

        FilterParameters.ClearClients();

        IsDirty = false;
        OnPropertyChanged(nameof(FilteredPackets));
    }

    [RelayCommand]
    public void StartTracing()
    {
        if (IsRunning)
        {
            return;
        }

        TraceClientName = SelectedTraceClient?.Name;

        _proxyServer.PacketReceived += OnPacketReceived;
        _proxyServer.PacketQueued += OnPacketQueued;

        StartTime = DateTime.Now;
        IsRunning = true;
        IsLive = true;

        _logger.LogInformation("Trace started");
    }

    [RelayCommand]
    public void StopTracing()
    {
        if (!IsRunning)
        {
            return;
        }

        _proxyServer.PacketReceived -= OnPacketReceived;
        _proxyServer.PacketQueued -= OnPacketQueued;

        IsRunning = false;

        SelectedTraceClient ??= TraceClients.FirstOrDefault();

        _logger.LogInformation("Trace stopped");
    }

    [RelayCommand]
    private async Task ClearTrace()
    {
        var confirm = await _dialogService.ShowMessageBoxAsync(new MessageBoxDetails
        {
            Title = "Confirm Clear Trace",
            Message = "Are you sure you want to clear?\nThis will remove all packets from the trace.",
            Description = "This action cannot be undone.",
            Style = MessageBoxStyle.YesNo
        });

        if (confirm is not true)
        {
            return;
        }

        ClearPackets();
        _logger.LogInformation("Trace cleared");
    }

    [RelayCommand]
    private void ScrollToEnd()
    {
        ScrollToEndRequested = true;
    }
}