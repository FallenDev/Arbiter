using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

public partial class InspectorValueViewModel : InspectorItemViewModel
{
    private bool _showHex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedValue))]
    [NotifyPropertyChangedFor(nameof(IsInteger))]
    private object? _value;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FormattedValue))]
    private string? _stringFormat;
    
    [ObservableProperty] private string? _toolTip;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanReveal))]
    private char? _maskCharacter;

    [ObservableProperty] private bool _isRevealed = true;

    public bool CanReveal => MaskCharacter is not null;

    public bool IsInteger => Value is sbyte or byte or short or ushort or int or uint or long or ulong;

    public bool ShowHex
    {
        get => _showHex;
        set
        {
            if (SetProperty(ref _showHex, value))
            {
                OnPropertyChanged(nameof(FormattedValue));
            }
        }
    }

    public string FormattedValue
    {
        get
        {
            if (Value is IEnumerable<byte> bytes)
            {
                return string.Join(' ', bytes.Select(x => x.ToString("X2")));
            }

            return string.Format(ShowHex && IsInteger ? "0x{0:X}" : StringFormat ?? "{0}", Value);
        }
    }

    protected override string GetCopyableValue() => FormattedValue;

    protected override bool CanCopyToClipboard()
    {
        return Value is not null && !string.IsNullOrWhiteSpace(FormattedValue);
    }
}