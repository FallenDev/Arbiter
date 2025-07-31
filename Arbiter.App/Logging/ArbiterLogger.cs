using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.App.Logging;

public class ArbiterLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ArbiterLoggerProvider _provider;

    public ArbiterLogger(string categoryName, ArbiterLoggerProvider provider)
    {
        _categoryName = categoryName;
        _provider = provider;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        
        var message = formatter(state, exception);
        var logEntry = new ArbiterLogEntry
        {
            Timestamp = DateTime.Now,
            Category = _categoryName,
            Level = logLevel,
            Message = message,
            Exception = exception
        };
        
        _provider.RaiseLogEntryCreated(logEntry);
    }

    private class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}