using System.Collections.ObjectModel;
using System.Linq;
using Arbiter.Net;

namespace Arbiter.App.ViewModels.Client;

public class ClientManagerViewModel : ViewModelBase
{
    private readonly ProxyServer _proxyServer;

    public ObservableCollection<ClientViewModel> Clients { get; } = [];

    public ClientManagerViewModel(ProxyServer proxyServer)
    {
        _proxyServer = proxyServer;

        _proxyServer.ClientAuthenticated += OnClientAuthenticated;
        _proxyServer.ClientRedirected += OnClientRedirected;
        _proxyServer.ClientDisconnected += OnClientDisconnected;
    }

    private void OnClientAuthenticated(object? sender, ProxyConnectionEventArgs e)
    {
        var connection = e.Connection;
        var client = new ClientViewModel(connection) { Id = connection.Id, Name = connection.Name ?? string.Empty };

        client.Subscribe();
        Clients.Add(client);
    }

    private void OnClientRedirected(object? sender, ProxyConnectionRedirectEventArgs e)
    {
        // Do nothing for now
    }

    private void OnClientDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        var client = Clients.FirstOrDefault(c => c.Id == e.Connection.Id);
        client?.Unsubscribe();

        if (client is not null)
        {
            Clients.Remove(client);
        }
    }
}