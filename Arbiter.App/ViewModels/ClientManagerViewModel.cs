using System.Collections.ObjectModel;
using System.Linq;
using Arbiter.Net;

namespace Arbiter.App.ViewModels;

public partial class ClientManagerViewModel : ViewModelBase
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
        var client = new ClientViewModel { Id = connection.Id, Name = connection.Name ?? string.Empty };

        Clients.Add(client);
    }

    private void OnClientRedirected(object? sender, ProxyConnectionRedirectEventArgs e)
    {

    }

    private void OnClientDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        var client = Clients.FirstOrDefault(c => c.Id == e.Connection.Id);
        if (client is not null)
        {
            Clients.Remove(client);
        }
    }
}