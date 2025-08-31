using System;
using Arbiter.App.Logging;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Logging;

public class LogEntryViewModel(ArbiterLogEntry logEntry) : ViewModelBase
{
    public DateTime Timestamp { get; } = logEntry.Timestamp;
    public string Category { get; } = logEntry.Category;
    public LogLevel Level { get; } = logEntry.Level;
    public string Message { get; } = logEntry.Message;
    public Exception? Exception { get; } = logEntry.Exception;

    public string FormattedMessage => Message;

    public string FormattedException =>
        Exception is null ? string.Empty : $"{Exception.GetType().Name}: {Exception.Message}";

    public string FormattedStackTrace =>
        Exception?.StackTrace is null ? string.Empty : $"Stack Trace:{Environment.NewLine}{Exception.StackTrace}";
}