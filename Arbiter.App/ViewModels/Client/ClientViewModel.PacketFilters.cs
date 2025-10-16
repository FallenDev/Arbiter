using System;
using Arbiter.Net;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Avalonia.Threading;

namespace Arbiter.App.ViewModels.Client;

public partial class ClientViewModel
{
    private const string FilterPrefix = "ClientView_";

    private NetworkFilterRef? _walkMessageFilter;
    private NetworkFilterRef? _userIdMessageFilter;
    private NetworkFilterRef? _mapInfoMessageFilter;
    private NetworkFilterRef? _mapLocationMessageFilter;
    private NetworkFilterRef? _selfProfileMessageFilter;
    private NetworkFilterRef? _updateStatsMessageFilter;
    
    private void AddPacketFilters()
    {
        // Client -> Server filters
        _walkMessageFilter = _connection.AddFilter(new ClientMessageFilter<ClientWalkMessage>(OnClientWalkMessage)
        {
            Name = "ClientView_ClientWalk",
            Priority = int.MaxValue
        });

        // Server -> Client filters
        _userIdMessageFilter = _connection.AddFilter(new ServerMessageFilter<ServerUserIdMessage>(OnServerUserIdMessage)
            {
                Name = "ClientView_ServerUserId",
                Priority = int.MaxValue
            });

        _mapInfoMessageFilter = _connection.AddFilter(new ServerMessageFilter<ServerMapInfoMessage>(OnServerMapInfoMessage)
            {
                Name = "ClientView_ServerMapInfo",
                Priority = int.MaxValue
            });

        _mapLocationMessageFilter = _connection.AddFilter(new ServerMessageFilter<ServerMapLocationMessage>(OnServerMapLocationMessage)
            {
                Name = "ClientView_ServerMapLocation",
                Priority = int.MaxValue
            });

        _selfProfileMessageFilter = _connection.AddFilter(new ServerMessageFilter<ServerSelfProfileMessage>(OnServerSelfProfileMessage)
            {
                Name = "ClientView_ServerSelfProfile",
                Priority = int.MaxValue
            });

        _updateStatsMessageFilter = _connection.AddFilter(new ServerMessageFilter<ServerUpdateStatsMessage>(OnServerUpdateStatsMessage)
            {
                Name = "ClientView_ServerUpdateStats",
                Priority = int.MaxValue
            });
    }

    private void RemovePacketFilters()
    {
        _walkMessageFilter?.Unregister();
        _userIdMessageFilter?.Unregister();
        _mapInfoMessageFilter?.Unregister();
        _mapLocationMessageFilter?.Unregister();
        _selfProfileMessageFilter?.Unregister();
        _updateStatsMessageFilter?.Unregister();
    }

    private NetworkPacket OnClientWalkMessage(ProxyConnection connection, ClientWalkMessage message,
        object? parameter, NetworkMessageFilterResult<ClientWalkMessage> result)
    {
        // Update predicted client position on UI thread
        Dispatcher.UIThread.Post(() => HandleClientMessage(message));
        return result.Passthrough();
    }

    private NetworkPacket OnServerUserIdMessage(ProxyConnection connection, ServerUserIdMessage message,
        object? parameter, NetworkMessageFilterResult<ServerUserIdMessage> result)
    {
        Dispatcher.UIThread.Post(() => HandleServerMessage(message));
        return result.Passthrough();
    }

    private NetworkPacket OnServerMapInfoMessage(ProxyConnection connection, ServerMapInfoMessage message,
        object? parameter, NetworkMessageFilterResult<ServerMapInfoMessage> result)
    {
        Dispatcher.UIThread.Post(() => HandleServerMessage(message));
        return result.Passthrough();
    }

    private NetworkPacket OnServerMapLocationMessage(ProxyConnection connection, ServerMapLocationMessage message,
        object? parameter, NetworkMessageFilterResult<ServerMapLocationMessage> result)
    {
        Dispatcher.UIThread.Post(() => HandleServerMessage(message));
        return result.Passthrough();
    }

    private NetworkPacket OnServerSelfProfileMessage(ProxyConnection connection, ServerSelfProfileMessage message,
        object? parameter, NetworkMessageFilterResult<ServerSelfProfileMessage> result)
    {
        Dispatcher.UIThread.Post(() => HandleServerMessage(message));
        return result.Passthrough();
    }

    private NetworkPacket OnServerUpdateStatsMessage(ProxyConnection connection, ServerUpdateStatsMessage message,
        object? parameter, NetworkMessageFilterResult<ServerUpdateStatsMessage> result)
    {
        Dispatcher.UIThread.Post(() => HandleServerMessage(message));
        return result.Passthrough();
    }
}
