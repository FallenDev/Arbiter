using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Filters;

public partial class MessageFilterViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isEnabled = true;
}