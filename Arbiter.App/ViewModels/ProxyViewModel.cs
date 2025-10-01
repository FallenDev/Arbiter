using System;
using System.Linq;
using System.Net;
using System.Threading;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public class ProxyViewModel : ViewModelBase
{
    private const string DebugAddEntityFilterName = "DebugAddEntityFilter";
    
    private readonly Lock _debugLock = new();
    private readonly ILogger<ProxyViewModel> _logger;
    private readonly ProxyServer _proxyServer;
    
    public bool DebugFiltersEnabled { get; private set; }

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
        _proxyServer.ClientLoggedIn += OnClientLoggedIn;
        _proxyServer.ClientLoggedOut += OnClientLoggedOut;
        _proxyServer.ClientRedirected += OnClientRedirected;
        _proxyServer.PacketException += OnClientException;

        _proxyServer.Start(localPort, remoteIpAddress, remotePort);
        OnPropertyChanged(nameof(IsRunning));

        _logger.LogInformation("Proxy started on 127.0.0.1:{Port}", localPort);
    }

    public void ApplyDebugFilters(DebugSettings settings)
    {
        using var _ = _debugLock.EnterScope();

        var debugFilter = new NetworkPacketFilter(HandleAddEntityPacket, settings)
        {
            Name = DebugAddEntityFilterName,
            Priority = int.MaxValue
        };
        
        // Add debug filters
        _proxyServer.AddFilter(ServerCommand.AddEntity, debugFilter);
        
        DebugFiltersEnabled = true;
        _logger.LogInformation("Debug packet filters enabled");
    }

    public void RemoveDebugFilters()
    {
        using var _ = _debugLock.EnterScope();

        if (!DebugFiltersEnabled)
        {
            return;
        }
        
        // Remove debug filters
        _proxyServer.RemoveFilter(ServerCommand.AddEntity, DebugAddEntityFilterName);

        DebugFiltersEnabled = false;
        _logger.LogInformation("Debug packet filters disabled");
    }

    private void OnClientConnected(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Client connected: {Endpoint}", name, e.Connection.LocalEndpoint);
    }

    private void OnServerConnected(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Server connected: {Endpoint}", name, e.Connection.RemoteEndpoint);
    }

    private void OnClientDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Client disconnected: {Endpoint}", name, e.Connection.LocalEndpoint);
    }

    private void OnServerDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Server disconnected: {Endpoint}", name, e.Connection.RemoteEndpoint);
    }

    private void OnClientAuthenticated(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Client authenticated", name);
    }

    private void OnClientLoggedIn(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Client logged in", name);
    }

    private void OnClientLoggedOut(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Client logged out", name);
    }

    private void OnClientRedirected(object? sender, ProxyConnectionRedirectEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Client redirected to {Endpoint}", name, e.RemoteEndpoint);
    }

    private void OnClientException(object? sender, ProxyConnectionExceptionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();

        var packetString = string.Join(' ', e.Packet.Data.Select(b => b.ToString("X2")));
        _logger.LogWarning("[{Name}] Bad packet: {Packet}", name, packetString);
    }

    private static NetworkPacket? HandleAddEntityPacket(NetworkPacket packet, object? parameter)
    {
        // Ignore if settings are not present
        if (parameter is not DebugSettings settings)
        {
            return packet;
        }

        var reader = new NetworkPacketReader(packet);

        return packet;
    }
}