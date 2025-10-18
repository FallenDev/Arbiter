using System;
using Arbiter.Net;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Client;

public partial class ClientViewModel
{
    private const string FilterPrefix = nameof(ClientViewModel);
    
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
            Name = $"{FilterPrefix}_ClientWalk",
            Priority = int.MaxValue
        });

        // Server -> Client filters
        _userIdMessageFilter = _connection.AddFilter(new ServerMessageFilter<ServerUserIdMessage>(OnServerUserIdMessage)
        {
            Name = $"{FilterPrefix}_ServerUserId",
            Priority = int.MaxValue
        });

        _mapInfoMessageFilter = _connection.AddFilter(
            new ServerMessageFilter<ServerMapInfoMessage>(OnServerMapInfoMessage)
            {
                Name = $"{FilterPrefix}_ServerMapInfo",
                Priority = int.MaxValue
            });

        _mapLocationMessageFilter = _connection.AddFilter(
            new ServerMessageFilter<ServerMapLocationMessage>(OnServerMapLocationMessage)
            {
                Name = $"{FilterPrefix}_ServerMapLocation",
                Priority = int.MaxValue
            });

        _selfProfileMessageFilter = _connection.AddFilter(
            new ServerMessageFilter<ServerSelfProfileMessage>(OnServerSelfProfileMessage)
            {
                Name = $"{FilterPrefix}_ServerSelfProfile",
                Priority = int.MaxValue
            });

        _updateStatsMessageFilter = _connection.AddFilter(
            new ServerMessageFilter<ServerUpdateStatsMessage>(OnServerUpdateStatsMessage)
            {
                Name = $"{FilterPrefix}_ServerUpdateStats",
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
        var direction = message.Direction;

        MapX = direction switch
        {
            WorldDirection.Left => MapX - 1,
            WorldDirection.Right => MapX + 1,
            _ => MapX
        };

        MapY = direction switch
        {
            WorldDirection.Up => MapY - 1,
            WorldDirection.Down => MapY + 1,
            _ => MapY
        };

        Player.MapX = MapX;
        Player.MapY = MapY;

        // Do not alter packet
        return result.Passthrough();
    }

    private NetworkPacket OnServerUserIdMessage(ProxyConnection connection, ServerUserIdMessage message,
        object? parameter, NetworkMessageFilterResult<ServerUserIdMessage> result)
    {
        EntityId = message.UserId;
        Class = message.Class.ToString();
        Player.UserId = message.UserId;
        Player.Class = Class;

        // Do not alter packet
        return result.Passthrough();
    }

    private NetworkPacket OnServerMapInfoMessage(ProxyConnection connection, ServerMapInfoMessage message,
        object? parameter, NetworkMessageFilterResult<ServerMapInfoMessage> result)
    {
        MapName = message.Name;
        MapId = message.MapId;
        Player.MapName = MapName;
        Player.MapId = MapId;

        // Do not alter packet
        return result.Passthrough();
    }

    private NetworkPacket OnServerMapLocationMessage(ProxyConnection connection, ServerMapLocationMessage message,
        object? parameter, NetworkMessageFilterResult<ServerMapLocationMessage> result)
    {
        MapX = message.X;
        MapY = message.Y;
        Player.MapX = MapX;
        Player.MapY = MapY;

        // Do not alter packet
        return result.Passthrough();
    }

    private NetworkPacket OnServerSelfProfileMessage(ProxyConnection connection, ServerSelfProfileMessage message,
        object? parameter, NetworkMessageFilterResult<ServerSelfProfileMessage> result)
    {
        Class = string.Equals(message.DisplayClass, "Master", StringComparison.OrdinalIgnoreCase)
            ? message.Class.ToString()
            : message.DisplayClass;

        // Do not alter packet
        return result.Passthrough();
    }

    private NetworkPacket OnServerUpdateStatsMessage(ProxyConnection connection, ServerUpdateStatsMessage message,
        object? parameter, NetworkMessageFilterResult<ServerUpdateStatsMessage> result)
    {
        Level = message.Level ?? Level;
        AbilityLevel = message.AbilityLevel ?? AbilityLevel;
        CurrentHealth = message.Health ?? CurrentHealth;
        MaxHealth = message.MaxHealth ?? MaxHealth;
        CurrentMana = message.Mana ?? CurrentMana;
        MaxMana = message.MaxMana ?? MaxMana;

        // Do not alter packet
        return result.Passthrough();
    }
}
