using System;
using System.Collections.Concurrent;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string DebugAddEntityFilterName = "Debug_AddEntityFilter";
    private const string DebugInteractFilterName = "Debug_InteractFilter";
    private const string DebugInteractMessageFilterName = "Debug_InteractMessageFilter";

    private static readonly TimeSpan InteractRequestTimeout = TimeSpan.FromSeconds(2);

    // Used to track interaction queries for a client to map back to the correct entity
    private readonly ConcurrentDictionary<uint, WorldEntity> _worldEntities = [];
    private readonly ConcurrentDictionary<int, ConcurrentQueue<(uint, DateTime)>> _interactRequests = [];

    private void AddDebugEntityFilters(DebugSettings settings)
    {
        if (settings.ShowNpcId || settings.ShowMonsterId || settings.ShowMonsterClickId)
        {
            _proxyServer.AddFilter(ServerCommand.AddEntity, new NetworkPacketFilter(HandleAddEntityMessage, settings)
            {
                Name = DebugAddEntityFilterName,
                Priority = DebugFilterPriority
            });
        }

        if (settings.ShowMonsterClickId)
        {
            _proxyServer.AddFilter(ClientCommand.Interact, new NetworkPacketFilter(HandleInteractMessage, settings)
            {
                Name = DebugInteractFilterName,
                Priority = DebugFilterPriority
            });

            _proxyServer.AddFilter(ServerCommand.WorldMessage,
                new NetworkPacketFilter(HandleInteractResponseMessage, settings)
                {
                    Name = DebugInteractMessageFilterName,
                    Priority = int.MaxValue
                });
        }
    }

    private void RemoveDebugEntityFilters()
    {
        _proxyServer.RemoveFilter(ServerCommand.AddEntity, DebugAddEntityFilterName);
        _proxyServer.RemoveFilter(ClientCommand.Interact, DebugInteractFilterName);
        _proxyServer.RemoveFilter(ServerCommand.WorldMessage, DebugInteractMessageFilterName);
    }

    private NetworkPacket HandleAddEntityMessage(ProxyConnection connection, NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { ShowMonsterId: false, ShowNpcId: false } ||
            !_serverMessageFactory.TryCreate<ServerAddEntityMessage>(serverPacket, out var message))
        {
            return packet;
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
            }
        }

        // Build a new packet with the modified entity data
        var builder = new NetworkPacketBuilder(ServerCommand.AddEntity);
        message.Serialize(builder);

        return builder.ToPacket();
    }

    private NetworkPacket HandleInteractMessage(ProxyConnection connection, NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ClientPacket clientPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { ShowMonsterClickId: false } ||
            !_clientMessageFactory.TryCreate<ClientInteractMessage>(clientPacket, out var message))
        {
            return packet;
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

        return packet;
    }

    private NetworkPacket HandleInteractResponseMessage(ProxyConnection connection, NetworkPacket packet,
        object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { ShowMonsterClickId: false } ||
            !_serverMessageFactory.TryCreate<ServerWorldMessageMessage>(serverPacket, out var message))
        {
            return packet;
        }

        var trimmedMessage = message.Message.Trim();

        // If the message is empty it's not a monster name
        if (string.IsNullOrWhiteSpace(trimmedMessage))
        {
            return packet;
        }

        // If the message ends with a period, question mark, or exclamation mark then it's not a monster name
        if (trimmedMessage.EndsWith('.') || trimmedMessage.EndsWith('!') || trimmedMessage.EndsWith('?'))
        {
            return packet;
        }

        // We can safely ingore any message that contains any of the following characters
        foreach (var c in trimmedMessage)
        {
            if (c is '[' or ']' or '(' or ')' or '<' or '>' or ':' or '!' or '?' or '"' or ',')
            {
                return packet;
            }
        }

        // If there is no queue or it is empty then not pending interactions
        if (!_interactRequests.TryGetValue(connection.Id, out var queue) || queue.IsEmpty)
        {
            return packet;
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

        if (!foundRequest)
        {
            return packet;
        }

        // Build a new packet with the modified world message data
        var builder = new NetworkPacketBuilder(ServerCommand.WorldMessage);
        message.Serialize(builder);

        return builder.ToPacket();
    }
}