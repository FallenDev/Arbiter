using System;
using System.Net;
using Arbiter.Net;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class ProxyViewModel : ViewModelBase
{
    private readonly ILogger<ProxyViewModel> _logger;
    private readonly ProxyServer _proxyServer;

    public bool IsRunning => _proxyServer.IsRunning;

    public ProxyViewModel(ILogger<ProxyViewModel> logger, ProxyServer proxyServer)
    {
        _logger = logger;
        _proxyServer = proxyServer;
    }

    public void Start(int localPort, IPAddress remoteIpAddress, int remotePort)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("Proxy is already running.");
        }

        _proxyServer.ClientConnected += OnClientConnected;
        _proxyServer.ServerConnected += OnServerConnected;
        _proxyServer.ClientDisconnected += OnClientDisconnected;
        _proxyServer.ServerDisconnected += OnServerDisconnected;
        _proxyServer.ClientAuthenticated += OnClientAuthenticated;
        _proxyServer.ClientRedirected += OnClientRedirected;

        _proxyServer.Start(localPort, remoteIpAddress, remotePort);
        OnPropertyChanged(nameof(IsRunning));
    }

    private void OnClientConnected(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.IsAuthenticated ? e.Connection.Name : e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Client connected: {Endpoint}", name, e.Connection.LocalEndpoint);
    }

    private void OnServerConnected(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.IsAuthenticated ? e.Connection.Name : e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Server connected: {Endpoint}", name, e.Connection.RemoteEndpoint);
    }

    private void OnClientDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.IsAuthenticated ? e.Connection.Name : e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Client disconnected: {Endpoint}", name, e.Connection.LocalEndpoint);
    }

    private void OnServerDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.IsAuthenticated ? e.Connection.Name : e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Server disconnected: {Endpoint}", name, e.Connection.RemoteEndpoint);
    }

    private void OnClientAuthenticated(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.IsAuthenticated ? e.Connection.Name : e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Client authenticated successfully", name);
    }

    private void OnClientRedirected(object? sender, ProxyConnectionRedirectEventArgs e)
    {
        var name = e.Connection.IsAuthenticated ? e.Connection.Name : e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Client redirected to {Endpoint}", name, e.RemoteEndpoint);
    }
}