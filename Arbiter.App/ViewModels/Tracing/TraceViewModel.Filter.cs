using System;
using System.ComponentModel;
using System.Linq;
using Arbiter.App.Collections;
using Arbiter.App.Models;
using Arbiter.App.Threading;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceViewModel
{
    private readonly Debouncer _filterRefreshDebouncer = new(TimeSpan.FromMilliseconds(50), Dispatcher.UIThread);

    [ObservableProperty] private bool _showFilterBar = true;

    public FilteredObservableCollection<TracePacketViewModel> FilteredPackets { get; }
    public TraceFilterViewModel FilterParameters { get; } = new();

    private void OnFilterParametersChanged(object? sender, PropertyChangedEventArgs e)
    {
        RequestFilterRefresh();
    }

    private void RequestFilterRefresh()
    {
        // This is debounced to avoid excessive refreshing when multiple changes are made rapidly
        _filterRefreshDebouncer.Execute(() =>
        {
            FilteredPackets.Refresh();
        });
    }

    private bool MatchesFilter(TracePacketViewModel vm)
    {
        if (vm.Direction == PacketDirection.Client)
        {
            var packet = vm.DecryptedPacket as ClientPacket;
            var clientCommands = FilterParameters.SelectedClientCommands;
            if (clientCommands.All(cmd => cmd != (byte)packet!.Command))
            {
                return false;
            }
        }
        else if (vm.Direction == PacketDirection.Server)
        {
            var packet = vm.DecryptedPacket as ServerPacket;
            var serverCommands = FilterParameters.SelectedServerCommands;
            if (serverCommands.All(cmd => cmd != (byte)packet!.Command))
            {
                return false;
            }
        }
        
        // Filter by client name matches
        if (FilterParameters.Clients.Count > 0)
        {
            if (string.IsNullOrEmpty(vm.ClientName))
            {
                return true;
            }

            var nameMatches = FilterParameters.Clients.Any(client =>
                client.IsSelected &&
                string.Equals(client.DisplayName, vm.ClientName, StringComparison.OrdinalIgnoreCase));

            if (!nameMatches)
            {
                return false;
            }
        }

        return true;
    }

    [RelayCommand]
    private void SelectAllCommands()
    {
        foreach (var command in FilterParameters.Commands)
        {
            command.IsSelected = true;
        }
    }
    
    [RelayCommand]
    private void SelectAllClients()
    {
        foreach (var client in FilterParameters.Clients)
        {
            client.IsSelected = true;
        }
    }
}