using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceSearchViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CommandFilter))]
    private byte? _command;

    public string? CommandFilter
    {
        get => Command?.ToString("X2");
    }
    
    [RelayCommand]
    public void ClearCommandFilter()
    {
        Command = null;
    }
}