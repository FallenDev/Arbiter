using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Tracing;

public partial class ClientFilterViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private string _displayName = string.Empty;
}