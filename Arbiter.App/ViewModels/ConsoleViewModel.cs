using System.Collections.ObjectModel;
using Arbiter.App.Logging;

namespace Arbiter.App.ViewModels;

public class ConsoleViewModel : ViewModelBase
{
    public ObservableCollection<ArbiterLogEntry> LogEntries { get; } = [];
    
    public ConsoleViewModel(ArbiterLoggerProvider provider)
    {
        provider.LogEntryCreated += (logEntry) =>
        {
            LogEntries.Add(logEntry);
        };
    }
}