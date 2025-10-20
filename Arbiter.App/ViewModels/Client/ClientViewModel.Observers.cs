using System;
using Arbiter.App.Models;
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
    private NetworkObserverRef? _addItemObserver;
    private NetworkObserverRef? _removeItemObserver;

    private void AddObservers()
    {
        const int observerPriority = int.MaxValue - 100;
        
        _userIdMessageObserver = _connection.AddObserver<ServerUserIdMessage>(OnUserIdMessage, observerPriority);

        _walkMessageObserver = _connection.AddObserver<ClientWalkMessage>(OnClientWalkMessage, observerPriority);
        _mapInfoMessageObserver = _connection.AddObserver<ServerMapInfoMessage>(OnMapInfoMessage, observerPriority);
        _mapLocationMessageObserver = _connection.AddObserver<ServerMapLocationMessage>(OnMapLocationMessage, observerPriority);
        _selfProfileMessageObserver = _connection.AddObserver<ServerSelfProfileMessage>(OnSelfProfileMessage, observerPriority);
        _updateStatsMessageObserver = _connection.AddObserver<ServerUpdateStatsMessage>(OnUpdateStatsMessage, observerPriority);

        // Inventory
        _addItemObserver = _connection.AddObserver<ServerAddItemMessage>(OnAddItemMessage);
        _removeItemObserver = _connection.AddObserver<ServerRemoveItemMessage>(OnRemoveItemMessage);
    }

    private void RemoveObservers()
    {
        _userIdMessageObserver?.Unregister();
        _walkMessageObserver?.Unregister();
        _mapInfoMessageObserver?.Unregister();
        _mapLocationMessageObserver?.Unregister();
        _selfProfileMessageObserver?.Unregister();
        _updateStatsMessageObserver?.Unregister();
        _addItemObserver?.Unregister();
        _removeItemObserver?.Unregister();
    }

    private void OnUserIdMessage(ProxyConnection connection, ServerUserIdMessage message, object? parameter)
    {
        Player.EntityId = message.UserId;
        Player.Class = message.Class.ToString();
    }
    
    #region Location Observers

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

    #endregion

    #region Stats Observers

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

    #endregion

    #region Inventory Observers

    private void OnAddItemMessage(ProxyConnection connection, ServerAddItemMessage message, object? parameter)
    {
        var item = new InventoryItem
        {
            Slot = message.Slot,
            Name = message.Name,
            Sprite = message.Sprite,
            Color = (byte)message.Color,
            Quantity = message.Quantity,
            IsStackable = message.IsStackable,
            Durability = message.Durability,
            MaxDurability = message.MaxDurability,
        };
        Player.Inventory.SetSlot(message.Slot, item);
    }

    private void OnRemoveItemMessage(ProxyConnection connection, ServerRemoveItemMessage message, object? parameter)
    {
        Player.Inventory.ClearSlot(message.Slot);
    }

    #endregion
}
