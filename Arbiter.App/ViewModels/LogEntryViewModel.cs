using System;
using Arbiter.App.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class LogEntryViewModel : ViewModelBase
{
    [ObservableProperty]
    private DateTime _timestamp;
    
    [ObservableProperty]
    private string _category;
    
    [ObservableProperty]
    private LogLevel _level;
    
    [ObservableProperty]
    private string _message;
    
    [ObservableProperty]
    private Exception? _exception;

    public LogEntryViewModel(ArbiterLogEntry logEntry)
    {
        Timestamp = logEntry.Timestamp;
        Category = logEntry.Category;
        Level = logEntry.Level;
        Message = logEntry.Message;
        Exception = logEntry.Exception;
    }
}