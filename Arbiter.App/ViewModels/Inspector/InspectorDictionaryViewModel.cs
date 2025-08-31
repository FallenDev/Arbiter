using System.Collections.ObjectModel;

namespace Arbiter.App.ViewModels.Inspector;

public class InspectorDictionaryViewModel : InspectorItemViewModel
{
    public ObservableCollection<InspectorValueViewModel> Values { get; } = [];
    
    protected override bool CanCopyToClipboard() => false;

    protected override string? GetCopyableValue() => null;
}