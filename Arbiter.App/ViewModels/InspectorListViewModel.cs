using System.Collections.ObjectModel;

namespace Arbiter.App.ViewModels;

public class InspectorListViewModel : InspectorItemViewModel
{
    public ObservableCollection<InspectorItemViewModel> Items { get; } = [];

    public int Count => Items.Count;

    protected override bool CanCopyToClipboard() => true;
    protected override string? GetCopyableValue() => Items.Count.ToString();
}