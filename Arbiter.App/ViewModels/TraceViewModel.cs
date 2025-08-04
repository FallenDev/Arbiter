using System.Collections.Specialized;
using Arbiter.App.Collections;
using Arbiter.App.Models;
using Arbiter.Net;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class TraceViewModel : ViewModelBase
{
    private readonly ILogger<TraceViewModel> _logger;
    private readonly ProxyServer _proxyServer;
    private readonly ConcurrentObservableCollection<TracePacketViewModel> _allPackets = [];

    private PacketDisplayMode _packetDisplayMode = PacketDisplayMode.Decrypted;
    
    public FilteredObservableCollection<TracePacketViewModel> FilteredPackets { get; }
    
    [ObservableProperty] private bool _scrollToEndRequested;
    [ObservableProperty] private bool _isRunning;
    
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

    public TraceViewModel(ILogger<TraceViewModel> logger, ProxyServer proxyServer)
    {
        _logger = logger;
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
    private void ClearTrace()
    {
        _allPackets.Clear();
        OnPropertyChanged(nameof(FilteredPackets));
    }
}