using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Arbiter.App.Models;
using Arbiter.Net.Client;
using Arbiter.Net.Server;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceFilterViewModel : ViewModelBase
{
    public ObservableCollection<ClientFilterViewModel> Clients { get; } = [];
    public ObservableCollection<CommandFilterViewModel> Commands { get; } = [];

    public IEnumerable<CommandFilterViewModel> SelectedCommands => Commands.Where(x => x.IsSelected);

    public IEnumerable<byte> SelectedClientCommands { get; set; } = [];
    public IEnumerable<byte> SelectedServerCommands { get; set; } = [];

    public IEnumerable<string> SelectedClientNames { get; set; } = [];
    
    public TraceFilterViewModel()
    {
        InitializeCommands();
    }

    public void UnselectCommand(ClientCommand command)
    {
        var matching =
            SelectedCommands.FirstOrDefault(c => c.Direction == PacketDirection.Client && c.Value == (byte)command);
        if (matching is not null)
        {
            matching.IsSelected = false;
        }
    }

    public void UnselectCommand(ServerCommand command)
    {
        var matching =
            SelectedCommands.FirstOrDefault(c => c.Direction == PacketDirection.Server && c.Value == (byte)command);
        if (matching is not null)
        {
            matching.IsSelected = false;
        }
    }

    public bool TryAddClient(string name, bool isSelected = true)
    {
        var existing = Clients.FirstOrDefault(client =>
            string.Equals(client.DisplayName, name, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            return false;
        }

        var newClient = new ClientFilterViewModel { DisplayName = name, IsSelected = isSelected };
        newClient.PropertyChanged += OnClientFilterPropertyChanged;
        
        Clients.Add(newClient);
        return true;
    }

    public bool TryRemoveClient(string name)
    {
        var existing = Clients.FirstOrDefault(client =>
            string.Equals(client.DisplayName, name, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            return false;
        }

        existing.PropertyChanged -= OnClientFilterPropertyChanged;
        Clients.Remove(existing);

        return true;
    }

    public void ClearClients()
    {
        Clients.Clear();
    }

    #region Command Filter Management
    private void InitializeCommands()
    {
        var clientCommandModels = Enum.GetValues<ClientCommand>()
            .OrderBy(cmd => cmd == ClientCommand.Unknown ? 1 : 0)
            .ThenBy(cmd => cmd.ToString())
            .Select(cmd => new CommandFilterViewModel(cmd, isSelected: true));

        var serverCommandModels = Enum.GetValues<ServerCommand>()
            .OrderBy(cmd => cmd == ServerCommand.Unknown ? 1 : 0)
            .ThenBy(cmd => cmd.ToString())
            .Select(cmd => new CommandFilterViewModel(cmd, isSelected: true));

        foreach (var vm in clientCommandModels)
        {
            vm.PropertyChanged += OnCommandFilterPropertyChanged;
            Commands.Add(vm);
        }

        foreach (var vm in serverCommandModels)
        {
            vm.PropertyChanged += OnCommandFilterPropertyChanged;
            Commands.Add(vm);
        }
        
        UpdateSelectedCommands();
    }

    private void OnCommandFilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CommandFilterViewModel.IsSelected))
        {
            UpdateSelectedCommands();
            NotifyCommandsChanged();
        }
    }

    private void UpdateSelectedCommands()
    {
        SelectedClientCommands = SelectedCommands
            .Where(x => x is { Direction: PacketDirection.Client, Value: not null })
            .Select(x => x.Value ?? 0xFF)
            .ToList();
        SelectedServerCommands = SelectedCommands
            .Where(x => x is { Direction: PacketDirection.Server, Value: not null })
            .Select(x => x.Value ?? 0xFF)
            .ToList();
    }

    private void NotifyCommandsChanged()
    {
        OnPropertyChanged(nameof(SelectedClientCommands));
        OnPropertyChanged(nameof(SelectedServerCommands));
        OnPropertyChanged(nameof(SelectedCommands));
    }
    #endregion
    
    #region Client Name Filter Management

    private void OnClientFilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClientFilterViewModel.IsSelected))
        {
            UpdateSelectedClients();
            NotifyClientsChanged();
        }
    }

    private void UpdateSelectedClients()
    {
        SelectedClientNames = Clients.Where(x => x.IsSelected).Select(x => x.DisplayName).ToList();
    }

    private void NotifyClientsChanged()
    {
        OnPropertyChanged(nameof(SelectedClientNames));
    }
    #endregion
}