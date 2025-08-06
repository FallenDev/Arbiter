using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arbiter.App.Collections;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

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
    private readonly IDialogService _dialogService;
    private readonly ITraceService _traceService;
    private readonly ProxyServer _proxyServer;
    private readonly ConcurrentObservableCollection<TracePacketViewModel> _allPackets = [];

    private PacketDisplayMode _packetDisplayMode = PacketDisplayMode.Decrypted;
    private PacketDirection _packetDirectionFilter = PacketDirection.Both;
    
    public FilteredObservableCollection<TracePacketViewModel> FilteredPackets { get; }
    
    [ObservableProperty] private DateTime _startTime;
    [ObservableProperty] private bool _scrollToEndRequested;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _showFilterBar = true;
    
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

    public PacketDirection PacketDirectionFilter
    {
        get => _packetDirectionFilter;
        set
        {
            if (SetProperty(ref _packetDirectionFilter, value))
            {
                OnPropertyChanged();
                FilteredPackets.Refresh();
            }
        }
    }

    public TraceViewModel(ILogger<TraceViewModel> logger, IStorageProvider storageProvider,
        IDialogService dialogService, ITraceService traceService, ProxyServer proxyServer)
    {
        _logger = logger;
        _storageProvider = storageProvider;
        _dialogService = dialogService;
        _traceService = traceService;
        _proxyServer = proxyServer;

        FilteredPackets = new FilteredObservableCollection<TracePacketViewModel>(_allPackets, MatchesFilter);

        _allPackets.CollectionChanged += OnPacketCollectionChanged;
    }

    private void OnPacketCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => { OnPropertyChanged(nameof(IsEmpty)); });
    }

    private bool MatchesFilter(TracePacketViewModel vm)
    {
        var direction = vm.Packet switch
        {
            ClientPacket => PacketDirection.Client,
            ServerPacket => PacketDirection.Server,
            _ => PacketDirection.None
        };

        if (!_packetDirectionFilter.HasFlag(direction))
        {
            return false;
        }

        return true;
    }

    private void OnPacketReceived(object? sender, ProxyConnectionDataEventArgs e)
    {
        var packetViewModel = new TracePacketViewModel(e.Packet, e.Payload, e.Connection.Name)
            { DisplayMode = _packetDisplayMode };
        _allPackets.Add(packetViewModel);
    }

    public async Task LoadFromFileAsync(string inputPath)
    {
        var traceFile = await _traceService.LoadTraceFileAsync(inputPath);
        var packets = traceFile.Packets;
        
        StopTracing();
        _allPackets.Clear();

        foreach (var packet in packets)
        {
            var vm = TracePacketViewModel.FromTracePacket(packet, _packetDisplayMode);
            _allPackets.Add(vm);
        }
    }

    public async Task SaveToFileAsync(string outputPath)
    {
        var snapshot = _allPackets.ToList();
        var packets = snapshot.Select(vm => vm.ToTracePacket());
        
        var traceFile = new TraceFile { Packets = packets.ToList() };
        await _traceService.SaveTraceFileAsync(traceFile, outputPath);
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
    private async Task LoadTrace()
    {
        var tracesDirectory = await _storageProvider.TryGetFolderFromPathAsync(TracesDirectory);
        var result = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Trace File",
            FileTypeFilter = [JsonFileType],
            SuggestedFileName = "trace.json",
            SuggestedStartLocation = tracesDirectory,
            AllowMultiple = false
        });

        if (result.Count == 0)
        {
            return;
        }

        if (IsRunning)
        {
            var confirm = await _dialogService.ShowMessageBoxAsync(new MessageBoxDetails
            {
                Title = "Confirm Load Trace",
                Message = "You have a trace running.\nAre you sure you want to load?",
                Description = "This will stop and replace your current trace.",
                Style = MessageBoxStyle.YesNo
            });

            if (confirm is not true)
            {
                return;
            }
        }

        var inputPath = result[0].Path.AbsolutePath;
        var filename = Path.GetFileName(inputPath);

        await LoadFromFileAsync(inputPath);
        _logger.LogInformation("Trace loaded from {Filename}", filename);
    }

    [RelayCommand]
    private async Task SaveTrace()
    {
        if (!Directory.Exists(TracesDirectory))
        {
            Directory.CreateDirectory(TracesDirectory);
        }
        
        var tracesDirectory = await _storageProvider.TryGetFolderFromPathAsync(TracesDirectory);
        var result = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Trace File",
            FileTypeChoices = [JsonFileType],
            SuggestedFileName = $"{StartTime:yyyy-MM-dd_HH-mm-ss}-trace.json",
            SuggestedStartLocation = tracesDirectory
        });

        if (result is null)
        {
            return;
        }

        var outputPath = result.Path.AbsolutePath;
        var filename = Path.GetFileName(outputPath);
        
        await SaveToFileAsync(outputPath);
        _logger.LogInformation("Trace saved to {Filename}", filename);
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

        _logger.LogInformation("Trace cleared");
    }

    [RelayCommand]
    private void ScrollToEnd()
    {
        ScrollToEndRequested = true;
    }
}