using System;
using System.Collections.Concurrent;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private static readonly TimeSpan InteractRequestTimeout = TimeSpan.FromSeconds(2);

    // Used to track interaction queries for a client to map back to the correct entity
    private readonly ConcurrentDictionary<uint, WorldEntity> _worldEntities = [];
    private readonly ConcurrentDictionary<int, ConcurrentQueue<(uint, DateTime)>> _interactRequests = [];

    private NetworkFilterRef? _debugAddEntityFilter;
    private NetworkFilterRef? _debugInteractFilter;
    private NetworkFilterRef? _debugInteractMessageFilter;

    private void AddDebugEntityFilters(DebugSettings settings)
    {
        if (settings.ShowNpcId || settings.ShowMonsterId || settings.ShowMonsterClickId)
        {
            _debugAddEntityFilter = _proxyServer.AddFilter(
                new ServerMessageFilter<ServerAddEntityMessage>(HandleAddEntityMessage, settings)
                {
                    Name = "Debug_AddEntityFilter",
                    Priority = DebugFilterPriority
                });
        }

        if (settings.ShowMonsterClickId)
        {
            _debugInteractFilter = _proxyServer.AddFilter(
                new ClientMessageFilter<ClientInteractMessage>(HandleInteractMessage, settings)
                {
                    Name = "Debug_InteractFilter",
                    Priority = DebugFilterPriority
                });

            _debugInteractMessageFilter = _proxyServer.AddFilter(
                new ServerMessageFilter<ServerWorldMessageMessage>(HandleInteractResponseMessage, settings)
                {
                    Name = "Debug_InteractMessageFilter",
                    Priority = DebugFilterPriority + 10
                });
        }
    }

    private void RemoveDebugEntityFilters()
    {
        _debugAddEntityFilter?.Unregister();
        _debugInteractFilter?.Unregister();
        _debugInteractMessageFilter?.Unregister();
    }

    private NetworkPacket HandleAddEntityMessage(ProxyConnection connection, ServerAddEntityMessage message,
        object? parameter, NetworkMessageFilterResult<ServerAddEntityMessage> result)
    {
        if (parameter is not DebugSettings filterSettings ||
            filterSettings is { ShowMonsterId: false, ShowNpcId: false })
        {
            return result.Passthrough();
        }

        // Update all entities in the world list
        foreach (var entity in message.Entities)
        {
            var entityType = entity switch
            {
                ServerCreatureEntity creatureEntity => creatureEntity.CreatureType == CreatureType.Mundane
                    ? WorldEntityType.Mundane
                    : WorldEntityType.Monster,
                _ => WorldEntityType.Item,
            };

            // Make a copy of the entity so we can store its original form
            var worldEntity = new WorldEntity(entityType, entity.Id, entity.X, entity.Y, entity.Sprite,
                entity is ServerCreatureEntity { Name: not null } nameEntity ? nameEntity.Name : null);

            _worldEntities.AddOrUpdate(entity.Id, worldEntity, (_, _) => worldEntity);
        }

        var hasChanges = false;

        // Inject NPC IDs into the entity names
        if (filterSettings.ShowNpcId)
        {
            foreach (var entity in message.Entities)
            {
                if (entity is not ServerCreatureEntity { CreatureType: CreatureType.Mundane } npcEntity)
                {
                    continue;
                }

                var name = npcEntity.Name ?? npcEntity.CreatureType.ToString();
                npcEntity.Name = $"{name} 0x{npcEntity.Id:X4}";
                hasChanges = true;
            }
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

                var name = monsterEntity.Name ?? monsterEntity.CreatureType.ToString();

                // Need to set the creature type to Mundane to display hover name
                monsterEntity.CreatureType = CreatureType.Mundane;
                monsterEntity.Name = $"{name} 0x{monsterEntity.Id:X4}";
                hasChanges = true;
            }
        }

        return hasChanges ? result.Replace(message) : result.Passthrough();
    }

    private NetworkPacket HandleInteractMessage(ProxyConnection connection, ClientInteractMessage message,
        object? parameter, NetworkMessageFilterResult<ClientInteractMessage> result)
    {
        if (parameter is not DebugSettings filterSettings || filterSettings is { ShowMonsterClickId: false })
        {
            return result.Passthrough();
        }

        // If the interaction type is Entity, queue the entity ID for later lookup when receiving the response
        if (message is { InteractionType: InteractionType.Entity, TargetId: not null })
        {
            // The entity in question should be a monster for us to queue it
            var isMonster = false;
            if (_worldEntities.TryGetValue(message.TargetId.Value, out var entity))
            {
                isMonster = entity.Type == WorldEntityType.Monster;
            }

            if (isMonster && _interactRequests.TryGetValue(connection.Id, out var queue))
            {
                queue.Enqueue((message.TargetId.Value, DateTime.Now));
            }
        }

        return result.Passthrough();
    }

    private NetworkPacket HandleInteractResponseMessage(ProxyConnection connection, ServerWorldMessageMessage message,
        object? parameter, NetworkMessageFilterResult<ServerWorldMessageMessage> result)
    {
        if (parameter is not DebugSettings filterSettings || filterSettings is { ShowMonsterClickId: false })
        {
            return result.Passthrough();
        }

        var trimmedMessage = message.Message.Trim();

        // If the message is empty it's not a monster name
        if (string.IsNullOrWhiteSpace(trimmedMessage))
        {
            return result.Passthrough();
        }

        // If the message ends with a period, question mark, or exclamation mark then it's not a monster name
        if (trimmedMessage.EndsWith('.') || trimmedMessage.EndsWith('!') || trimmedMessage.EndsWith('?'))
        {
            return result.Passthrough();
        }

        // We can safely ignore any message that contains any of the following characters
        foreach (var c in trimmedMessage)
        {
            if (c is '[' or ']' or '(' or ')' or '<' or '>' or ':' or '!' or '?' or '"' or ',')
            {
                return result.Passthrough();
            }
        }

        // If there is no queue or it is empty then not pending interactions
        if (!_interactRequests.TryGetValue(connection.Id, out var queue) || queue.IsEmpty)
        {
            return result.Passthrough();
        }

        // Look for a request that has not timed out
        var foundRequest = false;
        while (queue.TryDequeue(out var pair))
        {
            var (entityId, timestamp) = pair;
            var elapsed = DateTime.Now - timestamp;

            if (elapsed > InteractRequestTimeout)
            {
                continue;
            }

            message.Message = $"{trimmedMessage} 0x{entityId:X4}";

            // Also append the sprite index if we can find the entity in the world list
            if (_worldEntities.TryGetValue(entityId, out var entity))
            {
                message.Message = $"{message.Message} - Sprite {entity.Sprite}";
            }

            foundRequest = true;
        }

        return foundRequest ? result.Replace(message) : result.Passthrough();
    }
}