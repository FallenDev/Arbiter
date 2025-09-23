using System;
using System.Collections.Specialized;
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

    [ObservableProperty] private DateTime _startTime;
    [ObservableProperty] private bool _scrollToEndRequested;
    [ObservableProperty] private int? _scrollToIndexRequested;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isLive;
    [ObservableProperty] private bool _isDirty;
    
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

        FilteredPackets = new FilteredObservableCollection<TracePacketViewModel>(_allPackets, MatchesFilter);

        _allPackets.CollectionChanged += OnPacketCollectionChanged;
        SelectedPackets.CollectionChanged += OnSelectedPacketsCollectionChanged;

        FilterParameters.PropertyChanged += OnFilterParametersChanged;
        SearchParameters.PropertyChanged += OnSearchParametersChanged;
    }

    private void OnPacketCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var collection = (ConcurrentObservableCollection<TracePacketViewModel>)sender!;
        if (SetProperty(ref _isEmpty, collection.Count == 0))
        {
            Dispatcher.UIThread.Post(() => OnPropertyChanged(nameof(IsEmpty)), DispatcherPriority.Background);
        }
    }

    private void OnPacketReceived(object? sender, ProxyConnectionDataEventArgs e)
    {
        var packetViewModel = new TracePacketViewModel(e.Encrypted, e.Decrypted, e.Connection.Name)
            { DisplayMode = _packetDisplayMode };

        AddPacketToTrace(packetViewModel);
    }
    
    private void OnPacketQueued(object? sender, ProxyConnectionDataEventArgs e)
    {
        var packetViewModel = new TracePacketViewModel(e.Encrypted, e.Decrypted, e.Connection.Name)
            { DisplayMode = _packetDisplayMode };

        AddPacketToTrace(packetViewModel);
    }

    private void AddPacketToTrace(TracePacketViewModel vm)
    {
        var matchesSearch = MatchesSearch(vm);
        if (matchesSearch)
        {
            AddSearchResultIndex(_allPackets.Count);
        }

        vm.Opacity = matchesSearch ? 1 : 0.5;

        _allPackets.Add(vm);
        IsDirty = true;
    }

    private void ClearPackets()
    {
        _allPackets.Clear();
        SelectedPackets.Clear();
        
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