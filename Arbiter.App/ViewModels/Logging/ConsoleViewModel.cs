using System.Collections.Specialized;
using System.Linq;
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

    [ObservableProperty] private bool _scrollToEndRequested;

    private bool _showDebugMessages = true;
    private bool _showInfoMessages = true;
    private bool _showWarningMessages = true;
    private bool _showErrorMessages = true;

    public int DebugCount => _allLogEntries.SafeCountBy(log => log.Level is LogLevel.Debug or LogLevel.Trace);
    public int InfoCount => _allLogEntries.SafeCountBy(log => log.Level == LogLevel.Information);
    public int WarningCount => _allLogEntries.SafeCountBy(log => log.Level == LogLevel.Warning);
    public int ErrorCount => _allLogEntries.SafeCountBy(log => log.Level is LogLevel.Error or LogLevel.Critical);

    public bool IsEmpty => _allLogEntries.Count == 0;

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
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(DebugCount));
            OnPropertyChanged(nameof(InfoCount));
            OnPropertyChanged(nameof(WarningCount));
            OnPropertyChanged(nameof(ErrorCount));
        }, DispatcherPriority.Background);
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