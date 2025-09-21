using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceSearchViewModel : ViewModelBase
{
    private byte? _command;
    private string _commandFilter = string.Empty;

    public byte? Command => _command;

    public string? CommandFilter
    {
        get => _commandFilter;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                SetProperty(ref _commandFilter, string.Empty);
                SetProperty(ref _command, null);
                return;
            }

            if (!byte.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var command))
            {
                throw new ValidationException("Invalid command filter");
            }

            SetProperty(ref _commandFilter, value);
            SetProperty(ref _command, command);
        }
    }

    [RelayCommand]
    private void ClearCommandFilter()
    {
        CommandFilter = string.Empty;
    }
}