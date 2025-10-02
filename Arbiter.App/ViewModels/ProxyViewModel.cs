using System;
using System.Linq;
using System.Net;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public class ProxyViewModel : ViewModelBase
{
    private const string DebugAddEntityFilterName = "DebugAddEntityFilter";
    
    private readonly ILogger<ProxyViewModel> _logger;
    private readonly ProxyServer _proxyServer;
    private readonly IServerMessageFactory _serverMessageFactory = new ServerMessageFactory();
    
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
        var debugFilter = new NetworkPacketFilter(HandleAddEntityPacket, settings)
        {
            Name = DebugAddEntityFilterName,
            Priority = int.MaxValue
        };
        
        // Add debug filters (named filters will be replaced, not duplicated)
        _proxyServer.AddFilter(ServerCommand.AddEntity, debugFilter);
        
        DebugFiltersEnabled = true;
        _logger.LogInformation("Debug packet filters enabled");
    }

    public void RemoveDebugFilters()
    {
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
        _logger.LogInformation("[{Name}] Client connected -> {Endpoint}", name, e.Connection.LocalEndpoint);
    }

    private void OnServerConnected(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Server connected <- {Endpoint}", name, e.Connection.RemoteEndpoint);
    }

    private void OnClientDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Client disconnected -> {Endpoint}", name, e.Connection.LocalEndpoint);
    }

    private void OnServerDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();
        _logger.LogInformation("[{Name}] Server disconnected <- {Endpoint}", name, e.Connection.RemoteEndpoint);
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
        _logger.LogInformation("[{Name}] Client redirected -> {Endpoint}", name, e.RemoteEndpoint);
    }

    private void OnClientException(object? sender, ProxyConnectionExceptionEventArgs e)
    {
        var name = e.Connection.Name ?? e.Connection.Id.ToString();

        var packetString = string.Join(' ', e.Packet.Data.Select(b => b.ToString("X2")));
        _logger.LogWarning("[{Name}] Bad packet: {Packet}", name, packetString);
    }

    private NetworkPacket? HandleAddEntityPacket(NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        // If no debug settings enabled, ignore the packet
        if (filterSettings is { ShowMonsterId: false, ShowNpcId: false })
        {
            return packet;
        }

        // Ignore if the packet could not be read as the expected message type
        if (!_serverMessageFactory.TryCreate<ServerAddEntityMessage>(serverPacket, out var message))
        {
            return packet;
        }

        // Inject monster IDs into the entity names
        if (filterSettings.ShowMonsterId)
        {
            foreach (var entity in message.Entities)
            {
                if (entity is not ServerCreatureEntity { CreatureType: CreatureType.Monster } monsterEntity)
                {
                    continue;
                }

                var name = monsterEntity.Name ?? "Monster";
                
                // Need to set the creature type to Mundane to display hover name
                monsterEntity.CreatureType = CreatureType.Mundane;
                monsterEntity.Name = $"{name} 0x{monsterEntity.Id:x4}";
            }
        }

        // Inject NPC IDs into the entity names
        if (filterSettings.ShowNpcId)
        {
            foreach (var entity in message.Entities)
            {
                if (entity is not ServerCreatureEntity { CreatureType: CreatureType.Mundane } npcEntity)
                {
                    continue;
                }

                var name = npcEntity.Name ?? "Mundane";
                if (!name.StartsWith("Monster") && !name.EndsWith(')'))
                {
                    npcEntity.Name = $"{name} 0x{npcEntity.Id:x4}";
                }
            }
        }

        // Build a new packet with the modified entity data
        var builder = new NetworkPacketBuilder(ServerCommand.AddEntity);
        message.Serialize(builder);

        return builder.ToPacket();
    }
}