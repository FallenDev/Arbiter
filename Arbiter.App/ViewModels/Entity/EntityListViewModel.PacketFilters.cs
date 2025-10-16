using System;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Entity;

public partial class EntityListViewModel
{
    private void AddPacketFilters(ProxyServer proxyServer)
    {
        proxyServer.AddFilter(new ServerMessageFilter<ServerAddEntityMessage>(OnAddEntityMessage)
        {
            Name = "EntityView_AddEntityFilter",
            Priority = int.MaxValue
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerShowUserMessage>(OnShowUserMessage)
        {
            Name = "EntityView_ShowUserFilter",
            Priority = int.MaxValue
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerShowDialogMessage>(OnShowDialogMessage)
        {
            Name = "EntityView_ShowDialogFilter",
            Priority = int.MaxValue
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerShowDialogMenuMessage>(OnShowDialogMenuMessage)
        {
            Name = "EntityView_ShowDialogMenuFilter",
            Priority = int.MaxValue
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerEntityWalkMessage>(OnEntityWalkMessage)
        {
            Name = "EntityView_EntityWalkFilter",
            Priority = int.MaxValue
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerRemoveEntityMessage>(OnRemoveEntityMessage)
        {
            Name = "EntityView_RemoveEntityFilter",
            Priority = int.MaxValue
        });
    }

    private NetworkPacket OnAddEntityMessage(ProxyConnection connection, ServerAddEntityMessage message,
        object? parameter, NetworkMessageFilterResult<ServerAddEntityMessage> result)
    {
        foreach (var entity in message.Entities)
        {
            var flags = entity switch
            {
                ServerCreatureEntity creature => creature.CreatureType == CreatureType.Mundane
                    ? EntityFlags.Mundane
                    : EntityFlags.Monster,
                _ => EntityFlags.Item
            };

            var name = entity switch
            {
                ServerCreatureEntity creature => creature.Name,
                _ => null
            };

            var mapId = _playerService.TryGetState(connection.Id, out var ps1) ? ps1.MapId : null;
            var mapName = _playerService.TryGetState(connection.Id, out ps1) ? ps1.MapName : null;
            var gameEntity = new GameEntity
            {
                Flags = flags,
                Id = entity.Id,
                Name = name,
                Sprite = entity.Sprite,
                MapId = mapId,
                MapName = mapName,
                X = entity.X,
                Y = entity.Y,
            };

            _entityStore.AddOrUpdateEntity(gameEntity, out _);
        }

        // Do not alter the packet
        return result.Passthrough();
    }

    private NetworkPacket OnShowUserMessage(ProxyConnection connection, ServerShowUserMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowUserMessage> result)
    {
        var mapId2 = _playerService.TryGetState(connection.Id, out var ps2) ? ps2.MapId : null;
        var mapName2 = _playerService.TryGetState(connection.Id, out ps2) ? ps2.MapName : null;
        var entity = new GameEntity
        {
            Flags = EntityFlags.Player,
            Id = message.EntityId,
            Name = message.Name,
            Sprite = message.BodySprite.HasValue
                ? (ushort)message.BodySprite.Value
                : message.MonsterSprite ?? 0,
            MapId = mapId2,
            MapName = mapName2,
            X = message.X,
            Y = message.Y
        };

        _entityStore.AddOrUpdateEntity(entity, out _);

        // Do not alter the packet
        return result.Passthrough();
    }

    private NetworkPacket OnShowDialogMessage(ProxyConnection connection, ServerShowDialogMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowDialogMessage> result)
    {
        // Skip on invalid entity ID
        if (message.EntityId is null or 0)
        {
            return result.Passthrough();
        }

        var wasFound = _entityStore.TryGetEntity(message.EntityId.Value, out _);
        if (wasFound)
        {
            return result.Passthrough();
        }

        var flags = message.EntityType switch
        {
            EntityTypeFlags.Creature => EntityFlags.Mundane,
            EntityTypeFlags.Item => EntityFlags.Item,
            EntityTypeFlags.Reactor => EntityFlags.Reactor,
            _ => EntityFlags.Monster
        };

        var mapIdD = _playerService.TryGetState(connection.Id, out var psD) ? psD.MapId : null;
        var mapNameD = _playerService.TryGetState(connection.Id, out psD) ? psD.MapName : null;
        var entity = new GameEntity
        {
            Flags = flags,
            Id = message.EntityId.Value,
            Name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString(),
            Sprite = message.Sprite ?? 0,
            MapId = mapIdD,
            MapName = mapNameD,
        };

        _entityStore.AddOrUpdateEntity(entity, out _);

        // Do not alter the packet
        return result.Passthrough();
    }

    private NetworkPacket OnShowDialogMenuMessage(ProxyConnection connection, ServerShowDialogMenuMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowDialogMenuMessage> result)
    {
        // Skip on invalid entity ID
        if (message.EntityId is null or 0)
        {
            return result.Passthrough();
        }

        var wasFound = _entityStore.TryGetEntity(message.EntityId.Value, out _);
        if (wasFound)
        {
            return result.Passthrough();
        }

        var flags = message.EntityType switch
        {
            EntityTypeFlags.Creature => EntityFlags.Mundane,
            EntityTypeFlags.Item => EntityFlags.Item,
            EntityTypeFlags.Reactor => EntityFlags.Reactor,
            _ => EntityFlags.Monster
        };

        var mapIdM = _playerService.TryGetState(connection.Id, out var psM) ? psM.MapId : null;
        var mapNameM = _playerService.TryGetState(connection.Id, out psM) ? psM.MapName : null;
        var entity = new GameEntity
        {
            Flags = flags,
            Id = message.EntityId.Value,
            Name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString(),
            Sprite = message.Sprite ?? 0,
            MapId = mapIdM,
            MapName = mapNameM,
        };

        _entityStore.AddOrUpdateEntity(entity, out _);

        // Do not alter the packet
        return result.Passthrough();
    }

    private NetworkPacket OnEntityWalkMessage(ProxyConnection connection, ServerEntityWalkMessage message,
        object? parameter, NetworkMessageFilterResult<ServerEntityWalkMessage> result)
    {
        var existing = _entityStore.TryGetEntity(message.EntityId, out var entity);
        if (!existing)
        {
            return result.Passthrough();
        }

        var newEntity = entity with
        {
            X = message.Direction switch
            {
                WorldDirection.Left => Math.Max(0, message.OriginX - 1),
                WorldDirection.Right => message.OriginX + 1,
                _ => message.OriginX
            },
            Y = message.Direction switch
            {
                WorldDirection.Up => Math.Max(0, message.OriginY - 1),
                WorldDirection.Down => message.OriginY + 1,
                _ => message.OriginY
            }
        };

        _entityStore.AddOrUpdateEntity(newEntity, out _);

        // Do not alter the packet
        return result.Passthrough();
    }

    private NetworkPacket OnRemoveEntityMessage(ProxyConnection connection, ServerRemoveEntityMessage message,
        object? parameter, NetworkMessageFilterResult<ServerRemoveEntityMessage> result)
    {
        // Only remove monster and item entities since they are more ephemeral than player/npc entities
        if (_entityStore.TryGetEntity(message.EntityId, out var existing) && existing.Flags == EntityFlags.Monster ||
            existing.Flags == EntityFlags.Item)
        {
            _entityStore.RemoveEntity(message.EntityId, out _);
        }

        // Do not alter the packet
        return result.Passthrough();
    }
}