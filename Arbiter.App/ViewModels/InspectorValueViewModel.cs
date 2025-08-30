using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

public partial class InspectorValueViewModel : InspectorItemViewModel
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FormattedValue))]
    private object? _value;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FormattedValue))]
    private string? _stringFormat;

    public string FormattedValue => string.Format(StringFormat ?? "{0}", Value);

    protected override string GetCopyableValue() => FormattedValue;
}