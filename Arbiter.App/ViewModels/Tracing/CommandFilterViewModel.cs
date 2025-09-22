using Arbiter.Net.Client;
using Arbiter.Net.Server;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Tracing;

public partial class CommandFilterViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private string _displayName = string.Empty;
    [ObservableProperty] private byte _value;

    public CommandFilterViewModel(ClientCommand command, bool isSelected = false)
        : this(command.ToString(), (byte)command, isSelected)
    {
    }

    public CommandFilterViewModel(ServerCommand command, bool isSelected = false)
        : this(command.ToString(), (byte)command, isSelected)
    {
    }

    public CommandFilterViewModel(string displayName, byte value, bool isSelected = false)
    {
        DisplayName = displayName;
        Value = value;
        IsSelected = isSelected;
    }
}