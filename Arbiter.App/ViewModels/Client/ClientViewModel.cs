using System;
using Arbiter.App.Models;
using Arbiter.App.ViewModels.Player;
using Arbiter.Net;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Client;

public partial class ClientViewModel : ViewModelBase
{
    public int Id { get; init; }
    public required string Name { get; set; }

    private readonly ProxyConnection _connection;

    public PlayerViewModel Player { get; }

    public ClientViewModel(ProxyConnection connection, PlayerState player)
    {
        _connection = connection;
        Player = new PlayerViewModel(player);
    }

    public event EventHandler? BringToFrontRequested;
    
    public void Subscribe() => Player.Subscribe(_connection);
    public void Unsubscribe() => Player.Unsubscribe();

    public bool EnqueuePacket(NetworkPacket packet, NetworkPriority priority = NetworkPriority.Normal) =>
        _connection.EnqueuePacket(packet, priority);

    public bool EnqueueMessage(IClientMessage packet, NetworkPriority priority = NetworkPriority.Normal) =>
        _connection.EnqueueMessage(packet, priority);

    public bool EnqueueMessage(IServerMessage packet, NetworkPriority priority = NetworkPriority.Normal) =>
        _connection.EnqueueMessage(packet, priority);

    private static bool CanBringToFront() => OperatingSystem.IsWindows();

    [RelayCommand(CanExecute = nameof(CanBringToFront))]
    private void BringToFront()
    {
        BringToFrontRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanDisconnect() => _connection.IsConnected;

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    public void Disconnect()
    {
        _connection.Disconnect();
        DisconnectCommand.NotifyCanExecuteChanged();
    }
}