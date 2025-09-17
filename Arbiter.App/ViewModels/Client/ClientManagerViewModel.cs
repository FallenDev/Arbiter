using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Arbiter.Net;
using Avalonia.Threading;

namespace Arbiter.App.ViewModels.Client;

public class ClientManagerViewModel : ViewModelBase
{
    private readonly ProxyServer _proxyServer;

    private readonly Lock _clientLock = new();
    private readonly Dictionary<int, ClientViewModel> _clientMap = [];
    
    public ObservableCollection<ClientViewModel> Clients { get; } = [];

    public ClientManagerViewModel(ProxyServer proxyServer)
    {
        _proxyServer = proxyServer;

        _proxyServer.ClientAuthenticated += OnClientAuthenticated;
        _proxyServer.ClientLoggedIn += OnClientLoggedIn;
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

        // We are fully logged in and can display this client in the list
        Dispatcher.UIThread.Invoke(() => Clients.Add(client));
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
        
        Dispatcher.UIThread.Invoke(() => Clients.Remove(client));
        
        using var _ = _clientLock.EnterScope();
        _clientMap.Remove(client.Id);
    }
}