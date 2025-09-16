using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Inspector;

public partial class InspectorDictionaryViewModel : InspectorItemViewModel
{
    [ObservableProperty] private string? _typeName;
    
    [ObservableProperty] private bool _isExpanded = true;

    public ObservableCollection<InspectorValueViewModel> Values { get; } = [];

    protected override bool CanCopyToClipboard() => false;

    protected override string? GetCopyableValue() => null;
}