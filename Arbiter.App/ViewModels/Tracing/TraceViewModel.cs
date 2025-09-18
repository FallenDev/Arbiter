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
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
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
    private readonly List<int> _searchResultIndexes = [];

    private bool _isEmpty = true;

    private PacketDisplayMode _packetDisplayMode = PacketDisplayMode.Decrypted;
    private readonly Dictionary<string, Regex> _nameFilterRegexes = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty] private DateTime _startTime;
    [ObservableProperty] private bool _scrollToEndRequested;
    [ObservableProperty] private int? _scrollToIndexRequested;
    [ObservableProperty] private bool _isRunning;

    [ObservableProperty] private bool _showFilterBar;

    [ObservableProperty] private bool _showSearchBar;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedSearchResultsText))]
    [NotifyPropertyChangedFor(nameof(HasSearchResults))]
    private int _selectedSearchIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedSearchResultsText))]
    [NotifyPropertyChangedFor(nameof(HasSearchResults))]
    private int _searchResultCount;

    public event Action<TracePacketViewModel?>? SelectedPacketChanged;

    public int SelectedIndex => SelectedPackets.Count > 0 ? FilteredPackets.IndexOf(SelectedPackets[0]) : -1;
    public int SelectionCount => SelectedPackets.Count;
    
    public TracePacketViewModel? SelectedPacket => SelectedPackets.Count > 0 ? SelectedPackets[0] : null;
    public ObservableCollection<TracePacketViewModel> SelectedPackets { get; } = [];

    public FilteredObservableCollection<TracePacketViewModel> FilteredPackets { get; }
    public TraceFilterViewModel FilterParameters { get; } = new();
    public TraceSearchViewModel SearchParameters { get; } = new();

    public string? FormattedSearchResultsText =>
        SearchParameters.Command is not null
            ? SearchResultCount > 0 ? $"{Math.Max(1, SelectedSearchIndex)} of {SearchResultCount}" : "no matches"
            : null;

    public bool HasSearchResults => SearchResultCount > 0;

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

    private void OnSelectedPacketsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SelectedPacketChanged?.Invoke(SelectedPacket);

        Dispatcher.UIThread.Post(() =>
        {
            OnPropertyChanged(nameof(SelectedIndex));
            OnPropertyChanged(nameof(SelectionCount));
            OnPropertyChanged(nameof(SelectedPacket));
        }, DispatcherPriority.Background);
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

    private void OnFilterParametersChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Do not filter on this, wait for pattern list to change
        if (e.PropertyName == nameof(FilterParameters.NameFilter))
        {
            return;
        }

        // Do not filter on this, wait for command list to change
        if (e.PropertyName == nameof(FilterParameters.CommandFilter))
        {
            return;
        }

        FilteredPackets.Refresh();
    }

    private void OnSearchParametersChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Do not filter on this, wait for actual command byte to change
        if (e.PropertyName == nameof(SearchParameters.CommandFilter))
        {

        }

        RefreshSearchResults();
    }

    private void RefreshSearchResults()
    {
        _searchResultIndexes.Clear();
        SearchResultCount = 0;
        
        for (var i = 0; i < FilteredPackets.Count; i++)
        {
            var packet = FilteredPackets[i];
            var isMatch = MatchesSearch(packet);
            if (isMatch)
            {
                AddSearchResultIndex(i);
            }

            packet.Opacity = isMatch ? 1 : 0.5;
        }

        SelectedSearchIndex = 0;
    }

    private void AddSearchResultIndex(int index)
    {
        _searchResultIndexes.Add(index);
        SearchResultCount = _searchResultIndexes.Count;
    }

    private void SelectItemByIndex(int index)
    {
        SelectedPackets.Clear();
        SelectedPackets.Add(FilteredPackets[index]);
        ScrollToIndexRequested = index;
    }

    private bool MatchesFilter(TracePacketViewModel vm)
    {
        var direction = vm.Packet switch
        {
            ClientPacket => PacketDirection.Client,
            ServerPacket => PacketDirection.Server,
            _ => PacketDirection.None
        };

        // Filter by packet direction
        if (!FilterParameters.PacketDirection.HasFlag(direction))
        {
            return false;
        }

        // Filter by command
        if (FilterParameters.CommandFilterRanges.Count > 0)
        {
            if (!FilterParameters.CommandFilterRanges.Any(range => range.Contains(vm.Packet.Command)))
            {
                return false;
            }
        }

        // Filter by client name matches
        if (FilterParameters.NameFilterPatterns.Count > 0)
        {
            var nameMatches = FilterParameters.NameFilterPatterns.Any(namePattern =>
            {
                var regex = GetRegexForFilter(namePattern);
                return vm.ClientName is not null && regex.IsMatch(vm.ClientName);
            });

            if (!nameMatches)
            {
                return false;
            }
        }

        return true;
    }

    private bool MatchesSearch(TracePacketViewModel vm)
    {
        if (SearchParameters.Command is null)
        {
            return true;
        }

        return vm.Packet.Command == SearchParameters.Command;
    }

    [RelayCommand]
    private void GotoPreviousSearchResult()
    {
        if (_searchResultIndexes.Count == 0)
        {
            return;
        }

        var currentIndex = SelectedIndex;
        var pos = _searchResultIndexes.FindLastIndex(x => x < currentIndex);
        if (pos == -1)
        {
            pos = _searchResultIndexes.Count - 1;
        }
        
        SelectedSearchIndex = pos + 1;
        var packetIndex = _searchResultIndexes[pos];
        SelectItemByIndex(packetIndex);
    }

    [RelayCommand]
    private void GotoNextSearchResult()
    {
        if (_searchResultIndexes.Count == 0)
        {
            return;
        }

        var currentIndex = SelectedIndex;
        var pos = _searchResultIndexes.FindIndex(x => x > currentIndex);
        if (pos == -1)
        {
            pos = 0;
        }

        SelectedSearchIndex = pos + 1;
        var packetIndex = _searchResultIndexes[pos];
        SelectItemByIndex(packetIndex);
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

    private Regex GetRegexForFilter(string namePattern)
    {
        if (_nameFilterRegexes.TryGetValue(namePattern, out var regex))
        {
            return regex;
        }

        var escaped = Regex.Escape(namePattern);
        var regexPattern = escaped.Replace("\\*", ".*").Replace("\\?", ".");
        var newRegex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        _nameFilterRegexes.Add(namePattern, newRegex);
        return newRegex;
    }
}