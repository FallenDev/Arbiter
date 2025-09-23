using System;
using System.Collections.ObjectModel;
using System.Linq;
using Arbiter.App.Models;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceSearchViewModel : ViewModelBase
{
    [ObservableProperty] private CommandFilterViewModel? _selectedCommand;

    public ObservableCollection<CommandFilterViewModel> Commands { get; } =
    [
        // Placeholder 'None' command
        new(PacketDirection.Auto, "None", null)
    ];
    
    public TraceSearchViewModel()
    {
        InitializeCommands();
        SelectedCommand = Commands.FirstOrDefault(command => command.Value is null);
    }

    public void SelectCommand(ClientCommand command)
    {
        var matching = Commands.FirstOrDefault(c =>
            c.Direction == PacketDirection.Client && c.Value == (byte)command);

        if (matching is not null)
        {
            SelectedCommand = matching;
        }
    }

    public void SelectCommand(ServerCommand command)
    {
        var matching = Commands.FirstOrDefault(c =>
            c.Direction == PacketDirection.Server && c.Value == (byte)command);

        if (matching is not null)
        {
            SelectedCommand = matching;
        }
    }

    private void InitializeCommands()
    {
        var clientCommandModels = Enum.GetValues<ClientCommand>()
            .OrderBy(cmd => cmd == ClientCommand.Unknown ? 1 : 0)
            .ThenBy(cmd => cmd.ToString())
            .Select(cmd => new CommandFilterViewModel(cmd));

        var serverCommandModels = Enum.GetValues<ServerCommand>()
            .OrderBy(cmd => cmd == ServerCommand.Unknown ? 1 : 0)
            .ThenBy(cmd => cmd.ToString())
            .Select(cmd => new CommandFilterViewModel(cmd));

        foreach (var vm in clientCommandModels)
        {
            Commands.Add(vm);
        }

        foreach (var vm in serverCommandModels)
        {
            Commands.Add(vm);
        }
    }
    
    [RelayCommand]
    private void ClearCommandSearch()
    {
        SelectedCommand = null;
    }
}