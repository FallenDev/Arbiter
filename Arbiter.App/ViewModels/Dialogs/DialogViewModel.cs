using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Dialogs;

public partial class DialogViewModel : ViewModelBase
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _content;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigatePreviousCommand))]
    private bool _canNavigatePrevious;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateNextCommand))]
    private bool _canNavigateNext;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateTopCommand))]
    private bool _canNavigateTop;
    
    [RelayCommand(CanExecute = nameof(CanNavigatePrevious))]
    private void NavigatePrevious()
    {
        if (!CanNavigatePrevious)
        {
            return;
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateNext))]
    private void NavigateNext()
    {
        if (!CanNavigateNext)
        {
            return;
        }   
    }

    [RelayCommand(CanExecute = nameof(CanNavigateTop))]
    private void NavigateTop()
    {
        if (!CanNavigateTop)
        {
            return;
        }
    }
    
    [RelayCommand]
    private void CloseDialog()
    {
        
    }
    
    [RelayCommand]
    private void LoadDialog()
    {
        
    }

    [RelayCommand]
    private void SaveDialog()
    {
        
    }
}