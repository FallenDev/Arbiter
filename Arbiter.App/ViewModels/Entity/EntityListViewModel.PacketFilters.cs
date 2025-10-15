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

            var gameEntity = new GameEntity
            {
                Flags = flags,
                Id = entity.Id,
                Sprite = entity.Sprite,
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
        var entity = new GameEntity
        {
            Flags = EntityFlags.Player,
            Id = message.EntityId,
            Sprite = message.BodySprite.HasValue
                ? (ushort)message.BodySprite.Value
                : message.MonsterSprite ?? 0,
            X = message.X,
            Y = message.Y
        };

        _entityStore.AddOrUpdateEntity(entity, out _);

        // Do not alter the packet
        return result.Passthrough();
    }

    private NetworkPacket OnRemoveEntityMessage(ProxyConnection connection, ServerRemoveEntityMessage message,
        object? parameter, NetworkMessageFilterResult<ServerRemoveEntityMessage> result)
    {
        _entityStore.RemoveEntity(message.EntityId, out _);

        // Do not alter the packet
        return result.Passthrough();
    }
}