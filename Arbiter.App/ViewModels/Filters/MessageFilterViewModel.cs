using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Filters;

public partial class MessageFilterViewModel : ViewModelBase
{
    [ObservableProperty] private string _pattern = string.Empty;

    public override string ToString() => Pattern;
}