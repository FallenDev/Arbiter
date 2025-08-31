using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Inspector;

public partial class InspectorSectionViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isExpanded = true;

    public required string Header { get; set; }
    public int Order { get; set; } = int.MaxValue;

    public ObservableCollection<InspectorItemViewModel> Items { get; } = [];
}