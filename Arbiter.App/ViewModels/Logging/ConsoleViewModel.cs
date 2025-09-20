using System.Collections.Specialized;
using Arbiter.App.Collections;
using Arbiter.App.Logging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Logging;

public partial class ConsoleViewModel : ViewModelBase
{
    private readonly ConcurrentObservableCollection<LogEntryViewModel> _allLogEntries = [];

    public FilteredObservableCollection<LogEntryViewModel> FilteredLogEntries { get; }

    [ObservableProperty] private bool _isEmpty;
    [ObservableProperty] private int _debugCount;
    [ObservableProperty] private int _infoCount;
    [ObservableProperty] private int _warningCount;
    [ObservableProperty] private int _errorCount;
    
    [ObservableProperty] private bool _scrollToEndRequested;

    private bool _showDebugMessages = true;
    private bool _showInfoMessages = true;
    private bool _showWarningMessages = true;
    private bool _showErrorMessages = true;

    public bool ShowDebugMessages
    {
        get => _showDebugMessages;
        set
        {
            if (SetProperty(ref _showDebugMessages, value))
            {
                FilteredLogEntries.Refresh();
                OnPropertyChanged(nameof(FilteredLogEntries));
            }
        }
    }

    public bool ShowInfoMessages
    {
        get => _showInfoMessages;
        set
        {
            if (SetProperty(ref _showInfoMessages, value))
            {
                FilteredLogEntries.Refresh();
                OnPropertyChanged(nameof(FilteredLogEntries));
            }
        }
    }

    public bool ShowWarningMessages
    {
        get => _showWarningMessages;
        set
        {
            if (SetProperty(ref _showWarningMessages, value))
            {
                FilteredLogEntries.Refresh();
                OnPropertyChanged(nameof(FilteredLogEntries));
            }
        }
    }

    public bool ShowErrorMessages
    {
        get => _showErrorMessages;
        set
        {
            if (SetProperty(ref _showErrorMessages, value))
            {
                FilteredLogEntries.Refresh();
                OnPropertyChanged(nameof(FilteredLogEntries));
            }
        }
    }

    public ConsoleViewModel(ArbiterLoggerProvider provider)
    {
        FilteredLogEntries = new FilteredObservableCollection<LogEntryViewModel>(_allLogEntries, MatchesFilter);

        _allLogEntries.CollectionChanged += OnLogCollectionChanged;

        provider.LogEntryCreated += logEntry => { _allLogEntries.Add(new LogEntryViewModel(logEntry)); };
    }

    private void OnLogCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _allLogEntries.WithinLock(() =>
            {
                IsEmpty = _allLogEntries.Count == 0;
                UpdateCounts();
            });
            
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(DebugCount));
            OnPropertyChanged(nameof(InfoCount));
            OnPropertyChanged(nameof(WarningCount));
            OnPropertyChanged(nameof(ErrorCount));
        }, DispatcherPriority.Background);
    }

    private void UpdateCounts()
    {
        var debugCount = 0;
        var infoCount = 0;
        var warningCount = 0;
        var errorCount = 0;

        foreach (var logEntry in _allLogEntries)
        {
            switch (logEntry.Level)
            {
                case LogLevel.Debug or LogLevel.Trace:
                    debugCount++;
                    break;
                case LogLevel.Information:
                    infoCount++;
                    break;
                case LogLevel.Warning:
                    warningCount++;
                    break;
                case LogLevel.Error or LogLevel.Critical:
                    errorCount++;
                    break;
            }
        }

        DebugCount = debugCount;
        InfoCount = infoCount;
        WarningCount = warningCount;
        ErrorCount = errorCount;
    }

    private bool MatchesFilter(LogEntryViewModel logEntry)
    {
        return logEntry.Level switch
        {
            LogLevel.Error or LogLevel.Critical => ShowErrorMessages,
            LogLevel.Warning => ShowWarningMessages,
            LogLevel.Information => ShowInfoMessages,
            LogLevel.Debug or LogLevel.Trace => ShowDebugMessages,
            _ => true
        };
    }

    [RelayCommand]
    private void ClearLogs()
    {
        _allLogEntries.Clear();
        OnPropertyChanged(nameof(FilteredLogEntries));
    }

    [RelayCommand]
    private void ScrollToEnd()
    {
        ScrollToEndRequested = true;
    }
}