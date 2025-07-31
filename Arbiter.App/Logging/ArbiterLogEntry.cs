using System;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.Logging;

public class ArbiterLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Category { get; set; } = string.Empty;
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}