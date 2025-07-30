using System;
using Arbiter.App.Models;
using Arbiter.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public partial class MessageBoxViewModel : ViewModelBase, IDialogResult<bool?>
{
    public static MessageBoxViewModel DesignInstance => new()
    {
        Title = "Message Box",
        Message = "This is a message box that can display a message.\nIt can even span multiple lines.",
        Description = "This is a description of the message box.",
        Style = MessageBoxStyle.YesNo,
        AcceptButtonText = "OK",
        CancelButtonText = "Cancel"
    };
    
    [ObservableProperty] private string _title = "Message Box";

    [ObservableProperty] private string _message = string.Empty;

    [ObservableProperty] private string? _description;

    [ObservableProperty] private MessageBoxStyle _style = MessageBoxStyle.Ok;

    [ObservableProperty] private string _acceptButtonText = "OK";

    [ObservableProperty] private string _cancelButtonText = "Cancel";

    public event Action<bool?>? RequestClose;

    [RelayCommand]
    private void HandleOk()
    {
        RequestClose?.Invoke(true);
    }

    [RelayCommand]
    private void HandleCancel()
    {
        RequestClose?.Invoke(false);
    }
}