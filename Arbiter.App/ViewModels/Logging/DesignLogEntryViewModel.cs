using System;
using Arbiter.App.Logging;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Logging;

// This is only used at design-time to make it easier to adjust XAML view layout.
public class DesignLogEntryViewModel : LogEntryViewModel
{
    private static readonly ArbiterLogEntry MockLogEntry = new()
    {
        Timestamp = new DateTime(2025, 9, 16, 12, 34, 56),
        Category = "Design",
        Level = LogLevel.Information,
        Message = "The quick brown fox jumps over the lazy dog.",
        Exception = null
    };

    public DesignLogEntryViewModel()
        : base(MockLogEntry)
    {

    }
}