using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

public partial class InspectorSectionViewModel : ViewModelBase
{
    public required string Header { get; set; }
    public int Order { get; set; }

    [ObservableProperty] private bool _isExpanded;

    public InspectorSectionViewModel(string header, int order = int.MaxValue, bool isExpanded = true)
    {
        Header = header;
        Order = order;

        _isExpanded = isExpanded;
    }
}