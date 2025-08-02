using Arbiter.App.Collections;
using Arbiter.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public partial class TraceViewModel : ViewModelBase
{
    private readonly ProxyServer _proxyServer;
    private readonly ConcurrentObservableCollection<TracePacketViewModel> _allPackets = [];
    
    public FilteredObservableCollection<TracePacketViewModel> FilteredPackets { get; }

    [ObservableProperty] private bool _scrollToEndRequested;
    [ObservableProperty] private bool _isRunning;

    public TraceViewModel(ProxyServer proxyServer)
    {
        _proxyServer = proxyServer;

        FilteredPackets = new FilteredObservableCollection<TracePacketViewModel>(_allPackets, MatchesFilter);
    }

    private bool MatchesFilter(TracePacketViewModel packet)
    {
        return true;
    }
    
    [RelayCommand]
    private void StartTracing()
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
    }
    
    [RelayCommand]
    private void StopTracing()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
    }

    [RelayCommand]
    private void ClearTrace()
    {
        _allPackets.Clear();
        OnPropertyChanged(nameof(FilteredPackets));
    }
}