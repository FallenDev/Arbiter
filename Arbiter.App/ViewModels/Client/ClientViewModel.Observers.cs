using System;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Observers;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Client;

public partial class ClientViewModel
{
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

        var currentX = Player.MapX;
        var newX = direction switch
        {
            WorldDirection.Left => currentX - 1,
            WorldDirection.Right => currentX + 1,
            _ => currentX
        };

        var currentY = Player.MapY;
        var newY = direction switch
        {
            WorldDirection.Up => currentY - 1,
            WorldDirection.Down => currentY + 1,
            _ => currentY
        };

        Player.MapX = newX;
        Player.MapY = newY;
    }

    private void OnUserIdMessage(ProxyConnection connection, ServerUserIdMessage message, object? parameter)
    {
        Player.EntityId = message.UserId;
        Player.Class = message.Class.ToString();
    }

    private void OnMapInfoMessage(ProxyConnection connection, ServerMapInfoMessage message, object? parameter)
    {
        Player.MapId = message.MapId;
        Player.MapName = message.Name;
    }

    private void OnMapLocationMessage(ProxyConnection connection, ServerMapLocationMessage message, object? parameter)
    {
        Player.MapX = message.X;
        Player.MapY = message.Y;
    }

    private void OnSelfProfileMessage(ProxyConnection connection, ServerSelfProfileMessage message, object? parameter)
    {
        Player.Class = string.Equals(message.DisplayClass, "Master", StringComparison.OrdinalIgnoreCase)
            ? message.Class.ToString()
            : message.DisplayClass;
    }

    private void OnUpdateStatsMessage(ProxyConnection connection, ServerUpdateStatsMessage message, object? parameter)
    {
        Player.Level = message.Level ?? Player.Level;
        Player.AbilityLevel = message.AbilityLevel ?? Player.AbilityLevel;
        Player.CurrentHealth = message.Health ?? Player.CurrentHealth;
        Player.MaxHealth = message.MaxHealth ?? Player.MaxHealth;
        Player.CurrentMana = message.Mana ?? Player.CurrentMana;
        Player.MaxMana = message.MaxMana ?? Player.MaxMana;
    }
}
