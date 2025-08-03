using System;
using System.Net;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
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
        _proxyServer.ClientRedirected += OnClientRedirected;
        
        _proxyServer.Start(localPort, remoteIpAddress, remotePort);
        OnPropertyChanged(nameof(IsRunning));
    }

    private void OnClientConnected(object? sender, ProxyConnectionEventArgs e)
    {
        _logger.LogInformation("[{Id}] Client connected: {Endpoint}", e.Connection.Id, e.Connection.LocalEndpoint);
    }

    private void OnServerConnected(object? sender, ProxyConnectionEventArgs e)
    {
        _logger.LogInformation("[{Id}] Server connected: {Endpoint}", e.Connection.Id, e.Connection.RemoteEndpoint);
    }

    private void OnClientDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        _logger.LogInformation("[{Id}] Client disconnected: {Endpoint}", e.Connection.Id, e.Connection.LocalEndpoint);
    }

    private void OnServerDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        _logger.LogInformation("[{Id}] Server disconnected: {Endpoint}", e.Connection.Id, e.Connection.RemoteEndpoint);
    }
    
    private void OnClientRedirected(object? sender, ProxyConnectionRedirectEventArgs e)
    {
        _logger.LogInformation("[{Id}] Client redirected to {Endpoint}", e.Connection.Id, e.RemoteEndpoint);
    }
}