using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Arbiter.App.Collections;
using Arbiter.App.Extensions;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using Avalonia;
using Avalonia.Input;
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
        var packetViewModel = new TracePacketViewModel(e.Packet, e.RawData, e.Connection.Name)
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
    }

    public async Task LoadFromFileAsync(string inputPath, bool append = false)
    {
        var traceFile = await _traceService.LoadTraceFileAsync(inputPath);
        var packets = traceFile.Packets;

        StopTracing();

        if (!append)
        {
            _allPackets.Clear();
        }

        foreach (var packet in packets)
        {
            var vm = TracePacketViewModel.FromTracePacket(packet, _packetDisplayMode);
            AddPacketToTrace(vm);
        }
        
        RefreshSearchResults();
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
        var append = _keyboardService.IsModifierPressed(KeyModifiers.Shift);

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

        if (IsRunning && !IsEmpty)
        {
            var confirm = await _dialogService.ShowMessageBoxAsync(new MessageBoxDetails
            {
                Title = "Confirm Load Trace",
                Message = "You have an active trace running.\nAre you sure you want to load?",
                Description = append
                    ? "This will stop your current trace."
                    : "This will stop and replace your current trace.",
                Style = MessageBoxStyle.YesNo
            });

            if (confirm is not true)
            {
                return;
            }
        }

        var inputPath = result[0].Path.AbsolutePath;
        var filename = Path.GetFileName(inputPath);

        await LoadFromFileAsync(inputPath, append);
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

        SelectedPackets.Clear();

        _logger.LogInformation("Trace cleared");
    }

    [RelayCommand]
    private void ScrollToEnd()
    {
        ScrollToEndRequested = true;
    }
}