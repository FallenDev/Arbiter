using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Arbiter.App.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class ConsoleViewModel : ViewModelBase
{
    private readonly ObservableCollection<LogEntryViewModel> _allLogEntries = [];
    
    public ObservableCollection<LogEntryViewModel> FilteredLogEntries { get; } = [];
    
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
                RefilterMessages();
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
                RefilterMessages();
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
                RefilterMessages();
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
                RefilterMessages();
                OnPropertyChanged(nameof(FilteredLogEntries));
            }
        }
    }

    public ConsoleViewModel(ArbiterLoggerProvider provider)
    {
        _allLogEntries.CollectionChanged += OnLogEntriesChanged;
        provider.LogEntryCreated += logEntry => { _allLogEntries.Add(new LogEntryViewModel(logEntry)); };
    }

    private void OnLogEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
        {
            foreach (LogEntryViewModel entry in e.NewItems)
            {
                if (MatchesFilter(entry))
                {
                    FilteredLogEntries.Add(entry);
                }
            }
        }
        else if (e is { Action: NotifyCollectionChangedAction.Remove, OldItems: not null })
        {
            foreach (LogEntryViewModel entry in e.OldItems)
            {
                FilteredLogEntries.Remove(entry);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            RefilterMessages();
        }
    }

    private void RefilterMessages()
    {
        FilteredLogEntries.Clear();
        foreach (var entry in _allLogEntries)
        {
            if (MatchesFilter(entry))
            {
                FilteredLogEntries.Add(entry);
            }
        }
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
}