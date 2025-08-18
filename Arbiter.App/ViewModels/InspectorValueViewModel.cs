namespace Arbiter.App.ViewModels;

public class InspectorValueViewModel : InspectorItemViewModel
{
    private object? _value;
    private string? _stringFormat;

    public object? Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value))
            {
                OnPropertyChanged(nameof(FormattedValue));
            }
        }
    }

    public string? StringFormat
    {
        get => _stringFormat;
        set
        {
            if (SetProperty(ref _stringFormat, value))
            {
                OnPropertyChanged(nameof(FormattedValue));
            }
        }
    }

    public string FormattedValue => string.Format(StringFormat ?? "{0}", Value);
}