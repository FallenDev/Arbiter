using System.Collections.ObjectModel;

namespace Arbiter.App.ViewModels;

public class InspectorListViewModel : InspectorItemViewModel
{
    public ObservableCollection<InspectorItemViewModel> Items { get; } = [];

    public int Count => Items.Count;

    protected override bool CanCopyToClipboard() => false;
    protected override string? GetCopyableValue() => null;
}