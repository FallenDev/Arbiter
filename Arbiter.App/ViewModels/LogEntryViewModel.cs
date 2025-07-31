using System;
using Arbiter.App.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class LogEntryViewModel : ViewModelBase
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FormattedMessage))]
    private DateTime _timestamp;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FormattedMessage))]
    private string _category;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FormattedMessage))]
    private LogLevel _level;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FormattedMessage))]
    private string _message;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedMessage))]
    [NotifyPropertyChangedFor(nameof(FormattedException))]
    [NotifyPropertyChangedFor(nameof(FormattedStackTrace))]
    private Exception? _exception;

    public string FormattedMessage => Message;

    public string FormattedException =>
        Exception is null ? string.Empty : $"{Exception.GetType().Name}: {Exception.Message}";
    
    public string FormattedStackTrace =>
        Exception?.StackTrace is null ? string.Empty : $"Stack Trace:{Environment.NewLine}{Exception.StackTrace}";

    public LogEntryViewModel(ArbiterLogEntry logEntry)
    {
        Timestamp = logEntry.Timestamp;
        Category = logEntry.Category;
        Level = logEntry.Level;
        Message = logEntry.Message;
        Exception = logEntry.Exception;
    }
}