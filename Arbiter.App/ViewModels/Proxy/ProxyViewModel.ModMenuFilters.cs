using System.Collections.Generic;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private static readonly List<(string Text, ushort PursuitId)> ModMenuChoices =
    [
        ("Interact", 0xFF00),
        ("Buy", 64),
        ("Sell", 65),
        ("Fix Item", 71),
        ("Fix All Items", 72),
        ("Deposit Money", 66),
        ("Withdraw Money", 68),
        ("Deposit Item", 67),
        ("Withdraw Item", 69),
        ("Send Parcel", 96),
        ("Receive Parcel", 97),
    ];

    private NetworkFilterRef? _modMenuInteractFilter;

    private void AddModMenuFilters(DebugSettings settings)
    {
        _modMenuInteractFilter = _proxyServer.AddFilter(
            new ClientMessageFilter<ClientInteractMessage>(HandleModMenuInteractMessage, settings)
            {
                Name = $"{FilterPrefix}_ModMenu_ClientInteract",
                Priority = int.MinValue // Lowest priority
            });

        _modMenuInteractFilter = _proxyServer.AddFilter(
            new ClientMessageFilter<ClientDialogMenuChoiceMessage>(HandleModMenuDialogChoiceMessage, settings)
            {
                Name = $"{FilterPrefix}_ModMenu_ClientInteract",
                Priority = int.MinValue // Lowest priority
            });
    }

    private void RemoveModMenuFilters()
    {
        _modMenuInteractFilter?.Unregister();
    }

    private NetworkPacket? HandleModMenuInteractMessage(ProxyConnection connection, ClientInteractMessage message,
        object? parameter, NetworkMessageFilterResult<ClientInteractMessage> result)
    {
        if (parameter is not DebugSettings { EnableNpcModMenu: true })
        {
            return result.Passthrough();
        }

        // If the interaction is not a entity interaction, ignore it
        if (message.InteractionType != InteractionType.Entity || message.TargetId is null)
        {
            return result.Passthrough();
        }

        // If the entity is not found or not a NPC, ignore it
        if (!_entityStore.TryGetEntity(message.TargetId.Value, out var entity) ||
            !entity.Flags.HasFlag(EntityFlags.Mundane))
        {
            return result.Passthrough();
        }

        // Build a new dialog menu instead and send it
        var dialog = GetModMenuDialogForEntity(entity);
        connection.EnqueueMessage(dialog);

        return result.Block();
    }

    private NetworkPacket? HandleModMenuDialogChoiceMessage(ProxyConnection connection,
        ClientDialogMenuChoiceMessage message,
        object? parameter, NetworkMessageFilterResult<ClientDialogMenuChoiceMessage> result)
    {
        if (parameter is not DebugSettings { EnableNpcModMenu: true })
        {
            return result.Passthrough();
        }

        _logger.LogInformation("User selected dialog choice: {PursuitId:X4}", message.PursuitId);

        // Treat as interact request
        if (message.PursuitId == 0xFF00)
        {
            var interactMessage = new ClientInteractMessage
            {
                InteractionType = InteractionType.Entity,
                TargetId = message.EntityId
            };
            connection.EnqueueMessage(interactMessage);
            return result.Block();
        }
        
        // Only block our virtual menu pursuits
        return message.PursuitId >= 0xFF00 ? result.Block() : result.Passthrough();
    }

    private static ServerShowDialogMenuMessage GetModMenuDialogForEntity(GameEntity entity)
    {
        var dialog = new ServerShowDialogMenuMessage
        {
            MenuType = DialogMenuType.Menu,
            EntityType = entity.Flags switch
            {
                EntityFlags.Item => EntityTypeFlags.Item,
                EntityFlags.Reactor => EntityTypeFlags.Reactor,
                _ => EntityTypeFlags.Creature
            },
            EntityId = (uint)entity.Id,
            Sprite = entity.Sprite,
            SpriteType = SpriteType.Monster,
            ShowGraphic = true,
            PursuitId = 0xFFFF,
            Name = entity.Name,
            Content =
                "{=gThis is the NPC mod menu injected by {=eArbiter{=g.\n\n{=hYou can disable it in Settings -> NPCs."
        };

        foreach (var choice in ModMenuChoices)
        {
            dialog.MenuChoices.Add(new ServerDialogMenuChoice
            {
                Text = choice.Text,
                PursuitId = choice.PursuitId
            });
        }

        return dialog;
    }
}