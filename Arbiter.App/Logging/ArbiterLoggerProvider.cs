using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.Logging;

public class ArbiterLoggerProvider : ILoggerProvider
{
    private bool _isDisposed;
    private readonly ConcurrentDictionary<string, ArbiterLogger> _loggers = new();

    public event Action<ArbiterLogEntry>? LogEntryCreated;
    
    public ILogger CreateLogger(string categoryName)
        => _loggers.GetOrAdd(categoryName, name => new ArbiterLogger(name, this));

    internal void RaiseLogEntryCreated(ArbiterLogEntry logEntry)
    {
        LogEntryCreated?.Invoke(logEntry);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (isDisposing)
        {
            _loggers.Clear();
        }
        
        _isDisposed = true;
    }
}