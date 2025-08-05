using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Arbiter.App.Collections;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.Net;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class TraceViewModel : ViewModelBase
{
    private static readonly FilePickerFileType JsonFileType = new("JSON Files")
    {
        Patterns = ["*.json"],
        MimeTypes = ["application/json"],
    };
    
    private readonly ILogger<TraceViewModel> _logger;
    private readonly IStorageProvider _storageProvider;
    private readonly IDialogService _dialogService;
    private readonly ProxyServer _proxyServer;
    private readonly ConcurrentObservableCollection<TracePacketViewModel> _allPackets = [];

    private PacketDisplayMode _packetDisplayMode = PacketDisplayMode.Decrypted;
    
    public FilteredObservableCollection<TracePacketViewModel> FilteredPackets { get; }
    
    [ObservableProperty] private DateTime _startTime;
    [ObservableProperty] private bool _scrollToEndRequested;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _showFilterBar;
    
    public bool IsEmpty => _allPackets.Count == 0;

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
        IDialogService dialogService, ProxyServer proxyServer)
    {
        _logger = logger;
        _storageProvider = storageProvider;
        _dialogService = dialogService;
        _proxyServer = proxyServer;

        FilteredPackets = new FilteredObservableCollection<TracePacketViewModel>(_allPackets, MatchesFilter);

        _allPackets.CollectionChanged += OnPacketCollectionChanged;
    }

    private void OnPacketCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => { OnPropertyChanged(nameof(IsEmpty)); });
    }

    private bool MatchesFilter(TracePacketViewModel packet)
    {
        return true;
    }

    private void OnPacketReceived(object? sender, ProxyConnectionDataEventArgs e)
    {
        var packetViewModel = new TracePacketViewModel(e.Connection, e.Packet, e.Payload)
            { DisplayMode = _packetDisplayMode };
        _allPackets.Add(packetViewModel);
    }

    [RelayCommand]
    public void StartTracing()
    {
        if (IsRunning)
        {
            return;
        }

        _proxyServer.PacketReceived += OnPacketReceived;
        
        StartTime = DateTime.Now;
        IsRunning = true;
        
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
        IsRunning = false;

        _logger.LogInformation("Trace stopped");
    }

    [RelayCommand]
    private Task LoadTrace()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task SaveTrace()
    {
        return Task.CompletedTask;
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

        _allPackets.Clear();
        OnPropertyChanged(nameof(FilteredPackets));
    }

    [RelayCommand]
    private void ScrollToEnd()
    {
        ScrollToEndRequested = true;
    }
}