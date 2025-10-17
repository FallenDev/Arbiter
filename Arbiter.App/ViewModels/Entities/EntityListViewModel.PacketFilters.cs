using System;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Entities;

public partial class EntityListViewModel
{
    private void AddPacketFilters(ProxyServer proxyServer)
    {
        proxyServer.AddFilter(new ServerMessageFilter<ServerAddEntityMessage>(OnAddEntityMessage)
        {
            Name = "EntityView_AddEntityFilter",
            Priority = int.MaxValue - 10
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerShowUserMessage>(OnShowUserMessage)
        {
            Name = "EntityView_ShowUserFilter",
            Priority = int.MaxValue - 10
        });
        
        proxyServer.AddFilter(new ServerMessageFilter<ServerUserProfileMessage>(OnUserProfileMessage)
        {
            Name = "EntityView_UserProfileFilter",
            Priority = int.MaxValue - 10
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerPublicMessageMessage>(OnPublicMessage)
        {
            Name = "EntityView_PublicMessageFilter",
            Priority = int.MaxValue - 10
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerShowDialogMessage>(OnShowDialogMessage)
        {
            Name = "EntityView_ShowDialogFilter",
            Priority = int.MaxValue - 10
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerShowDialogMenuMessage>(OnShowDialogMenuMessage)
        {
            Name = "EntityView_ShowDialogMenuFilter",
            Priority = int.MaxValue - 10
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerEntityWalkMessage>(OnEntityWalkMessage)
        {
            Name = "EntityView_EntityWalkFilter",
            Priority = int.MaxValue - 10
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerWalkResponseMessage>(OnSelfWalkMessage)
        {
            Name = "EntityView_SelfWalkFilter",
            Priority = int.MaxValue - 10
        });

        proxyServer.AddFilter(new ServerMessageFilter<ServerRemoveEntityMessage>(OnRemoveEntityMessage)
        {
            Name = "EntityView_RemoveEntityFilter",
            Priority = int.MaxValue - 10
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

            // Try to get the player so we can get map context
            _playerService.TryGetState(connection.Id, out var player);

            var gameEntity = new GameEntity
            {
                Flags = flags,
                Id = entity.Id,
                Name = name,
                Sprite = entity.Sprite,
                MapId = player?.MapId,
                MapName = player?.MapName,
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
        // Try to get the player so we can get map context
        _playerService.TryGetState(connection.Id, out var player);

        var entity = new GameEntity
        {
            Flags = EntityFlags.Player,
            Id = message.EntityId,
            Name = message.Name,
            Sprite = message.BodySprite.HasValue
                ? (ushort)message.BodySprite.Value
                : message.MonsterSprite ?? 0,
            MapId = player?.MapId,
            MapName = player?.MapName,
            X = message.X,
            Y = message.Y
        };

        _entityStore.AddOrUpdateEntity(entity, out _);

        // Do not alter the packet
        return result.Passthrough();
    }
    
    private NetworkPacket OnUserProfileMessage(ProxyConnection connection, ServerUserProfileMessage message,
        object? parameter, NetworkMessageFilterResult<ServerUserProfileMessage> result)
    {
        // Try to get the player so we can get map context
        _playerService.TryGetState(connection.Id, out var player);

        var entity = new GameEntity
        {
            Flags = EntityFlags.Player,
            Id = message.EntityId,
            Name = message.Name,
            MapId = player?.MapId,
            MapName = player?.MapName
        };

        _entityStore.AddOrUpdateEntity(entity, out _);

        // Do not alter the packet
        return result.Passthrough();
    }
    
    private NetworkPacket OnPublicMessage(ProxyConnection connection, ServerPublicMessageMessage message,
        object? parameter, NetworkMessageFilterResult<ServerPublicMessageMessage> result)
    {
        // Ignore world shouts
        if (message.SenderId == 0)
        {
            return result.Passthrough();
        }
        
        // Try to get the player so we can get map context
        _playerService.TryGetState(connection.Id, out var player);
        
        // Assume it might be a player ghost
        var entityFlags = EntityFlags.Player;
        var entitySprite = (ushort)BodySprite.MaleGhost;
        
        // Try to get the existing entity, this should rule out monsters/npcs that talk
        if (_entityStore.TryGetEntity(message.SenderId, out var existing))
        {
            entityFlags = existing.Flags;
            entitySprite = existing.Sprite ?? entitySprite;
        }
        
        // The name should come before the symbol, so split on the first symbol (chat or shout)
        var senderName = message.Message.Split(':', '!', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        var entity = new GameEntity
        {
            Flags = entityFlags,
            Id = message.SenderId,
            Sprite = entitySprite,
            Name = senderName,
            MapId = player?.MapId,
            MapName = player?.MapName
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

        // Try to get the player so we can get map context
        _playerService.TryGetState(connection.Id, out var player);

        var entity = new GameEntity
        {
            Flags = flags,
            Id = message.EntityId.Value,
            Name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString(),
            Sprite = message.Sprite ?? 0,
            MapId = player?.MapId,
            MapName = player?.MapName,
            // Reactors should assume the player's location when encountering
            X = flags.HasFlag(EntityFlags.Reactor) ? player?.MapX ?? 0 : 0,
            Y = flags.HasFlag(EntityFlags.Reactor) ? player?.MapY ?? 0 : 0,
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

        // Try to get the player so we can get map context
        _playerService.TryGetState(connection.Id, out var player);

        var entity = new GameEntity
        {
            Flags = flags,
            Id = message.EntityId.Value,
            Name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString(),
            Sprite = message.Sprite ?? 0,
            MapId = player?.MapId,
            MapName = player?.MapName
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

    private NetworkPacket OnSelfWalkMessage(ProxyConnection connection, ServerWalkResponseMessage message,
        object? parameter, NetworkMessageFilterResult<ServerWalkResponseMessage> result)
    {
        // If no active player, ignore
        if (!_playerService.TryGetState(connection.Id, out var player) || player.UserId is null)
        {
            return result.Passthrough();
        }

        if (!_entityStore.TryGetEntity(player.UserId.Value, out var selfEntity))
        {
            return result.Passthrough();
        }

        var newEntity = selfEntity with
        {
            Name = player.Name,
            MapId = player.MapId,
            MapName = player.MapName,
            X = message.Direction switch
            {
                WorldDirection.Left => Math.Max(0, message.PreviousX - 1),
                WorldDirection.Right => message.PreviousX + 1,
                _ => message.PreviousX
            },
            Y = message.Direction switch
            {
                WorldDirection.Up => Math.Max(0, message.PreviousY - 1),
                WorldDirection.Down => message.PreviousX + 1,
                _ => message.PreviousY
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