using System;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Observers;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Client;

public partial class ClientViewModel
{
    private const string FilterPrefix = nameof(ClientViewModel);

    private NetworkObserverRef? _walkMessageObserver;
    private NetworkObserverRef? _userIdMessageObserver;
    private NetworkObserverRef? _mapInfoMessageObserver;
    private NetworkObserverRef? _mapLocationMessageObserver;
    private NetworkObserverRef? _selfProfileMessageObserver;
    private NetworkObserverRef? _updateStatsMessageObserver;

    private void AddObservers()
    {
        _walkMessageObserver = _connection.AddObserver<ClientWalkMessage>(OnClientWalkMessage);
        _userIdMessageObserver = _connection.AddObserver<ServerUserIdMessage>(OnUserIdMessage);
        _mapInfoMessageObserver = _connection.AddObserver<ServerMapInfoMessage>(OnMapInfoMessage);
        _mapLocationMessageObserver = _connection.AddObserver<ServerMapLocationMessage>(OnMapLocationMessage);
        _selfProfileMessageObserver = _connection.AddObserver<ServerSelfProfileMessage>(OnSelfProfileMessage);
        _updateStatsMessageObserver = _connection.AddObserver<ServerUpdateStatsMessage>(OnUpdateStatsMessage);
    }

    private void RemoveObservers()
    {
        _walkMessageObserver?.Unregister();
        _userIdMessageObserver?.Unregister();
        _mapInfoMessageObserver?.Unregister();
        _mapLocationMessageObserver?.Unregister();
        _selfProfileMessageObserver?.Unregister();
        _updateStatsMessageObserver?.Unregister();
    }

    private void OnClientWalkMessage(ProxyConnection connection, ClientWalkMessage message, object? parameter)
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
    }

    private void OnUserIdMessage(ProxyConnection connection, ServerUserIdMessage message, object? parameter)
    {
        EntityId = message.UserId;
        Class = message.Class.ToString();
        Player.UserId = message.UserId;
        Player.Class = Class;
    }

    private void OnMapInfoMessage(ProxyConnection connection, ServerMapInfoMessage message, object? parameter)
    {
        MapName = message.Name;
        MapId = message.MapId;
        Player.MapName = MapName;
        Player.MapId = MapId;
    }

    private void OnMapLocationMessage(ProxyConnection connection, ServerMapLocationMessage message, object? parameter)
    {
        MapX = message.X;
        MapY = message.Y;
        Player.MapX = MapX;
        Player.MapY = MapY;
    }

    private void OnSelfProfileMessage(ProxyConnection connection, ServerSelfProfileMessage message, object? parameter)
    {
        Class = string.Equals(message.DisplayClass, "Master", StringComparison.OrdinalIgnoreCase)
            ? message.Class.ToString()
            : message.DisplayClass;
    }

    private void OnUpdateStatsMessage(ProxyConnection connection, ServerUpdateStatsMessage message, object? parameter)
    {
        Level = message.Level ?? Level;
        AbilityLevel = message.AbilityLevel ?? AbilityLevel;
        CurrentHealth = message.Health ?? CurrentHealth;
        MaxHealth = message.MaxHealth ?? MaxHealth;
        CurrentMana = message.Mana ?? CurrentMana;
        MaxMana = message.MaxMana ?? MaxMana;
    }
}
