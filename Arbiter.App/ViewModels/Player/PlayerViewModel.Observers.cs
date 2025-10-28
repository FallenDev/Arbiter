using System;
using Arbiter.App.Models.Player;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Observers;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerViewModel
{
    private NetworkObserverRef? _walkMessageObserver;
    private NetworkObserverRef? _userIdMessageObserver;
    private NetworkObserverRef? _mapInfoMessageObserver;
    private NetworkObserverRef? _mapLocationMessageObserver;
    private NetworkObserverRef? _selfProfileMessageObserver;
    private NetworkObserverRef? _updateStatsMessageObserver;
    private NetworkObserverRef? _addItemObserver;
    private NetworkObserverRef? _removeItemObserver;

    public void Subscribe(ProxyConnection connection)
    {
        const int observerPriority = int.MaxValue - 100;

        _userIdMessageObserver = connection.AddObserver<ServerUserIdMessage>(OnUserIdMessage, observerPriority);

        _walkMessageObserver = connection.AddObserver<ClientWalkMessage>(OnClientWalkMessage, observerPriority);
        _mapInfoMessageObserver = connection.AddObserver<ServerMapInfoMessage>(OnMapInfoMessage, observerPriority);
        _mapLocationMessageObserver =
            connection.AddObserver<ServerMapLocationMessage>(OnMapLocationMessage, observerPriority);
        _selfProfileMessageObserver =
            connection.AddObserver<ServerSelfProfileMessage>(OnSelfProfileMessage, observerPriority);
        _updateStatsMessageObserver =
            connection.AddObserver<ServerUpdateStatsMessage>(OnUpdateStatsMessage, observerPriority);

        // Inventory
        _addItemObserver = connection.AddObserver<ServerAddItemMessage>(OnAddItemMessage);
        _removeItemObserver = connection.AddObserver<ServerRemoveItemMessage>(OnRemoveItemMessage);
    }

    public void Unsubscribe()
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
        EntityId = message.UserId;
        Class = message.Class.ToString();
    }

    #region Location Observers

    private void OnClientWalkMessage(ProxyConnection connection, ClientWalkMessage message, object? parameter)
    {
        // Update predicted client position on UI thread
        var direction = message.Direction;

        var currentX = MapX;
        var newX = direction switch
        {
            WorldDirection.Left => currentX - 1,
            WorldDirection.Right => currentX + 1,
            _ => currentX
        };

        var currentY = MapY;
        var newY = direction switch
        {
            WorldDirection.Up => currentY - 1,
            WorldDirection.Down => currentY + 1,
            _ => currentY
        };

        MapX = newX;
        MapY = newY;
    }

    private void OnMapInfoMessage(ProxyConnection connection, ServerMapInfoMessage message, object? parameter)
    {
        MapId = message.MapId;
        MapName = message.Name;
    }

    private void OnMapLocationMessage(ProxyConnection connection, ServerMapLocationMessage message, object? parameter)
    {
        MapX = message.X;
        MapY = message.Y;
    }

    #endregion

    #region Stats Observers

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

        Inventory.SetSlot(message.Slot, item);
    }

    private void OnRemoveItemMessage(ProxyConnection connection, ServerRemoveItemMessage message, object? parameter)
    {
        Inventory.ClearSlot(message.Slot);
    }

    #endregion
}