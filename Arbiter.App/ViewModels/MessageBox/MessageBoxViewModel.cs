using System;
using Arbiter.App.Models;
using Arbiter.App.Services.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.MessageBox;

public partial class MessageBoxViewModel : ViewModelBase, IDialogResult<bool?>
{
  
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