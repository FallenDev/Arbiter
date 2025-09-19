using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Arbiter.App.Services;
using Arbiter.Net.Proxy;
using Avalonia.Threading;

namespace Arbiter.App.ViewModels.Client;

public class ClientManagerViewModel : ViewModelBase
{
    private readonly ProxyServer _proxyServer;
    private readonly IGameClientService _gameClientService;
    
    private readonly Lock _clientLock = new();
    private readonly Dictionary<int, ClientViewModel> _clientMap = [];

    public ObservableCollection<ClientViewModel> Clients { get; } = [];

    public ClientManagerViewModel(ProxyServer proxyServer, IGameClientService gameClientService)
    {
        _proxyServer = proxyServer;
        _gameClientService = gameClientService;

        _proxyServer.ClientAuthenticated += OnClientAuthenticated;
        _proxyServer.ClientLoggedIn += OnClientLoggedIn;
        _proxyServer.ClientLoggedOut += OnClientLoggedOut;
        _proxyServer.ClientRedirected += OnClientRedirected;
        _proxyServer.ClientDisconnected += OnClientDisconnected;
    }

    private void OnClientAuthenticated(object? sender, ProxyConnectionEventArgs e)
    {
        var connection = e.Connection;
        var client = new ClientViewModel(connection) { Id = connection.Id, Name = connection.Name ?? string.Empty };
        
        // Start listening for packets and updating state
        client.Subscribe();

        using var _ = _clientLock.EnterScope();
        _clientMap[connection.Id] = client;
    }

    private void OnClientLoggedIn(object? sender, ProxyConnectionEventArgs e)
    {
        using var _ = _clientLock.EnterScope();
        var client = _clientMap.GetValueOrDefault(e.Connection.Id);

        if (client is null)
        {
            return;
        }

        var windowTitle = !string.IsNullOrWhiteSpace(e.Connection.Name)
            ? $"Darkages - {e.Connection.Name}"
            : "Darkages";
        
        SetClientWindowTitle(client, windowTitle);
        client.BringToFrontRequested += OnClientBringToFront;

        // We are fully logged in and can display this client in the list
        Dispatcher.UIThread.Post(() => Clients.Add(client));
    }

    private void OnClientLoggedOut(object? sender, ProxyConnectionEventArgs e)
    {
        using var _ = _clientLock.EnterScope();
        var client = _clientMap.GetValueOrDefault(e.Connection.Id);

        if (client is null)
        {
            return;
        }
        
        SetClientWindowTitle(client, "Darkages");
        client.BringToFrontRequested -= OnClientBringToFront;
        
        // We are fully logged out and can remove the client
        Dispatcher.UIThread.Post(() => Clients.Remove(client));
    }

    private void OnClientRedirected(object? sender, ProxyConnectionRedirectEventArgs e)
    {
        // Do nothing for now
    }

    private void OnClientDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        var client = Clients.FirstOrDefault(c => c.Id == e.Connection.Id);
        client?.Unsubscribe();

        if (client is null)
        {
            return;
        }
        
        SetClientWindowTitle(client, "Darkages");
        client.BringToFrontRequested -= OnClientBringToFront;

        // Try removing the client from the list
        Dispatcher.UIThread.Post(() => Clients.Remove(client));

        using var _ = _clientLock.EnterScope();
        _clientMap.Remove(client.Id);
    }

    private void OnClientBringToFront(object? sender, EventArgs e)
    {
        if (sender is not ClientViewModel clientVm || !OperatingSystem.IsWindows())
        {
            return;
        }

        var clientWindow = _gameClientService.GetGameClients().FirstOrDefault(gc => gc.CharacterName == clientVm.Name);
        clientWindow?.BringToFront();
    }

    private void SetClientWindowTitle(ClientViewModel vm, string windowTitle)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
        
        var clientWindow = _gameClientService.GetGameClients().FirstOrDefault(gc => gc.CharacterName == vm.Name);
        clientWindow?.SetWindowTitle(windowTitle);
    }
}