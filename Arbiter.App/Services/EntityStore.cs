using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Arbiter.App.Models;

namespace Arbiter.App.Services;

public sealed class EntityStore : IEntityStore
{
    private readonly ConcurrentDictionary<long, GameEntity> _entities = [];

    public event Action<GameEntity>? EntityAdded;
    public event Action<GameEntity>? EntityUpdated;
    public event Action<GameEntity>? EntityRemoved;

    public IEnumerable<GameEntity> Entities => _entities.Values;

    public bool IsEmpty => _entities.IsEmpty;
    public int Count => _entities.Count;

    public bool TryGetEntity(long id, out GameEntity entity) => _entities.TryGetValue(id, out entity);

    public void AddOrUpdateEntity(GameEntity entity, out bool wasUpdated)
    {
        wasUpdated = false;

        if (_entities.TryAdd(entity.Id, entity))
        {
            OnEntityAdded(entity);
            return;
        }

        if (!_entities.TryGetValue(entity.Id, out var existingEntity))
        {
            return;
        }

        // Preserve existing map data values
        var updatedEntity = existingEntity with
        {
            MapId = entity.MapId ?? existingEntity.MapId,
            MapName = entity.MapName ?? existingEntity.MapName,
            X = entity.X,
            Y = entity.Y
        };

        _entities[entity.Id] = updatedEntity;
        OnEntityUpdated(updatedEntity);
        wasUpdated = true;
    }

    public bool RemoveEntity(long id, out GameEntity entity)
    {
        var wasRemoved = _entities.TryRemove(id, out entity);
        if (wasRemoved)
        {
            OnEntityRemoved(entity);
        }

        return wasRemoved;
    }

    private void OnEntityAdded(GameEntity entity)
    {
        EntityAdded?.Invoke(entity);
    }

    private void OnEntityUpdated(GameEntity entity)
    {
        EntityUpdated?.Invoke(entity);
    }

    private void OnEntityRemoved(GameEntity entity)
    {
        EntityRemoved?.Invoke(entity);
    }
}