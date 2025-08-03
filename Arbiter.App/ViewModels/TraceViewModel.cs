using System.Collections.Specialized;
using Arbiter.App.Collections;
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
    
    public FilteredObservableCollection<TracePacketViewModel> FilteredPackets { get; }

    [ObservableProperty] private bool _scrollToEndRequested;
    [ObservableProperty] private bool _isRunning;

    public bool IsEmpty => _allPackets.Count == 0;
    
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
        _allPackets.Add(new TracePacketViewModel(e.Connection, e.Packet, e.Payload));
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