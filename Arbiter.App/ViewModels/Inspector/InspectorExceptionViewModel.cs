using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Inspector;

public partial class InspectorExceptionViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExceptionName))] 
    [NotifyPropertyChangedFor(nameof(Message))]
    [NotifyPropertyChangedFor(nameof(FormattedStackTrace))]
    private Exception? _exception;

    public string ExceptionName => Exception?.GetType().Name ?? "Exception";

    public string Message => Exception?.Message ?? "Exception has no message.";
    
    public string FormattedStackTrace => Exception?.StackTrace is null ? "No stack trace available" : $"Stack Trace:{Environment.NewLine}{Exception.StackTrace}";
}