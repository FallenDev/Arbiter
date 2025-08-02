using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public partial class TraceViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isRunning;
    
    [RelayCommand]
    private void StartTracing()
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
    }
    
    [RelayCommand]
    private void StopTracing()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
    }
}