using System;
using System.Linq;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private NetworkFilterRef? _debugDialogFilter;
    private NetworkFilterRef? _debugDialogMenuFilter;
    private NetworkFilterRef? _debugDialogMenuItemQuantityFilter;

    private void AddDebugDialogFilters(DebugSettings settings)
    {
        _debugDialogFilter = _proxyServer.AddFilter<ServerShowDialogMessage>(HandleDialogMessage,
            $"{FilterPrefix}_Dialog_ServerShowDialog", DebugFilterPriority, settings);

        _debugDialogMenuFilter = _proxyServer.AddFilter<ServerShowDialogMenuMessage>(HandleDialogMenuMessage,
            $"{FilterPrefix}_Dialog_ServerShowDialogMenu", DebugFilterPriority, settings);

        _debugDialogMenuItemQuantityFilter = _proxyServer.AddFilter<ServerShowDialogMenuMessage>(
            HandleDialogItemQuantityMenuMessage,
            $"{FilterPrefix}_Dialog_ItemQuantity_ServerShowDialogMenu", int.MinValue, settings);
    }

    private void RemoveDebugDialogFilters()
    {
        _debugDialogFilter?.Unregister();
        _debugDialogMenuFilter?.Unregister();
        _debugDialogMenuItemQuantityFilter?.Unregister();
    }

    private static NetworkPacket HandleDialogMessage(ProxyConnection connection, ServerShowDialogMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowDialogMessage> result)
    {
        if (parameter is not DebugSettings settings || settings is { ShowDialogId: false, ShowPursuitId: false })
        {
            return result.Passthrough();
        }

        var hasChanges = false;

        if (settings.ShowDialogId)
        {
            var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
            message.Name = $"{name} {{=h[0x{message.EntityId:X4}]";
            hasChanges = true;
        }

        if (settings.ShowPursuitId)
        {
            var pursuitText = $"{{=ePursuit {message.PursuitId} => Step {message.StepId}";
            message.Name = $"{message.Name} {pursuitText}";
            hasChanges = true;

        }

        return hasChanges ? result.Replace(message) : result.Passthrough();
    }

    private static NetworkPacket HandleDialogMenuMessage(ProxyConnection connection,
        ServerShowDialogMenuMessage message, object? parameter,
        NetworkMessageFilterResult<ServerShowDialogMenuMessage> result)
    {
        if (parameter is not DebugSettings settings || settings is { ShowDialogId: false, ShowPursuitId: false })
        {
            return result.Passthrough();
        }

        var hasChanges = false;

        if (settings.ShowDialogId)
        {
            var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
            message.Name = $"{name} {{=h[0x{message.EntityId:X4}]";
            hasChanges = true;
        }

        if (settings.ShowPursuitId)
        {
            if (message.PursuitId is > 0)
            {
                var pursuitText = $"{{=ePursuit {message.PursuitId}";
                message.Name = $"{message.Name} - {pursuitText}";
                hasChanges = true;
            }

            if (message.MenuChoices.Count > 0)
            {
                foreach (var choice in message.MenuChoices)
                {
                    var menuPursuitText = $"{{=j[{choice.PursuitId}]";
                    var maxChoiceLength = 50 - menuPursuitText.Length;
                    var choiceText = choice.Text.Length > maxChoiceLength
                        ? string.Concat(choice.Text.AsSpan(0, maxChoiceLength - 3), "...")
                        : choice.Text;

                    choice.Text = $"{choiceText} {menuPursuitText}";
                }

                hasChanges = true;
            }
        }

        return hasChanges ? result.Replace(message) : result.Passthrough();
    }

    private NetworkPacket HandleDialogItemQuantityMenuMessage(ProxyConnection connection,
        ServerShowDialogMenuMessage message, object? parameter,
        NetworkMessageFilterResult<ServerShowDialogMenuMessage> result)
    {
        if (parameter is not DebugSettings settings || settings is { ShowDialogItemQuantity: false } ||
            message.MenuType != DialogMenuType.UserInventory)
        {
            return result.Passthrough();
        }

        // If unable to get the player OR no stackable items just passthrough
        if (!_playerService.TryGetState(connection.Id, out var player) || !player.Inventory.Any(i => i.IsStackable))
        {
            return result.Passthrough();
        }
        
        SendItemQuantityOverrides(connection, player.Inventory);
        return result.Passthrough();
    }

    private static void SendItemQuantityOverrides(ProxyConnection connection, PlayerInventory inventory)
    {
        // First we update the client with the stack size in the name
        var addItemMessages = inventory.Where(i => i.IsStackable)
            .Select(item =>
            {
                var baseName = StripItemQuantitySuffix(item.Name);
                return new ServerAddItemMessage
                {
                    Slot = (byte)item.Slot,
                    Name = $"{baseName} [{item.Quantity}]",
                    Sprite = item.Sprite,
                    Color = (DyeColor)item.Color,
                    Quantity = (uint)item.Quantity,
                    IsStackable = item.IsStackable,
                    Durability = (uint)(item.Durability ?? 0),
                    MaxDurability = (uint)(item.MaxDurability ?? 0)
                };
            });
        connection.EnqueueMessages(addItemMessages, NetworkPriority.High);
        
        // Then we revert it after a short delay which will happen after the dialog is shown
        // This happens very quickly so its not noticeable to the user
        var revertItemMessages = inventory.Where(i => i.IsStackable)
            .Select(item =>
            {
                var baseName = StripItemQuantitySuffix(item.Name);
                return new ServerAddItemMessage
                {
                    Slot = (byte)item.Slot,
                    Name = baseName,
                    Sprite = item.Sprite,
                    Color = (DyeColor)item.Color,
                    Quantity = (uint)item.Quantity,
                    IsStackable = item.IsStackable,
                    Durability = (uint)(item.Durability ?? 0),
                    MaxDurability = (uint)(item.MaxDurability ?? 0)
                };
            });
        connection.EnqueueMessagesAfter(revertItemMessages, TimeSpan.FromMilliseconds(100), NetworkPriority.High);
    }

    private static string StripItemQuantitySuffix(string name)
    {
        if (string.IsNullOrEmpty(name) || name[^1] != ']')
        {
            return name;
        }

        var bracketIndex = name.LastIndexOf(" [", StringComparison.Ordinal);
        if (bracketIndex < 0)
        {
            return name;
        }

        // Ensure it ends with ']' and content between '[' and ']' are digits only
        var start = bracketIndex + 2; // skip space and '['
        var end = name.Length - 1; // position of ']'
        if (start >= end) return name;

        for (var i = start; i < end; i++)
        {
            if (!char.IsDigit(name[i]))
            {
                return name;
            }
        }

        // Looks like a quantity suffix; strip it
        return name[..bracketIndex];
    }
}