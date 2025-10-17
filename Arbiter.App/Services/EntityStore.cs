using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Arbiter.App.Models;

namespace Arbiter.App.Services;

public sealed class EntityStore : IEntityStore
{
    private readonly Lock _lock = new();
    private readonly Dictionary<long, GameEntity> _entities = [];
    private readonly Dictionary<string, GameEntity> _playerEntities = new (StringComparer.OrdinalIgnoreCase);
    
    public event Action<GameEntity>? EntityAdded;
    public event Action<GameEntity>? EntityUpdated;
    public event Action<GameEntity>? EntityRemoved;

    public IEnumerable<GameEntity> Entities
    {
        get
        {
            using var _ = _lock.EnterScope();
            return _entities.Values.ToList();
        }
    }

    public bool IsEmpty => Count == 0;

    public int Count
    {
        get
        {
            using var _ = _lock.EnterScope();
            return _entities.Count;
        }
    }

    public bool TryGetEntity(long id, out GameEntity entity)
    {
        using var _ = _lock.EnterScope();
        return _entities.TryGetValue(id, out entity);
    }

    public void AddOrUpdateEntity(GameEntity entity, out bool wasUpdated)
    {
        wasUpdated = false;

        using var _ = _lock.EnterScope();

        // Handle players differently since they can re-appear with new IDs due to re-logging
        if (entity.Flags.HasFlag(EntityFlags.Player))
        {
            _playerEntities.TryGetValue(entity.Name ?? string.Empty, out var existingPlayer);
            AddOrUpdatePlayerEntity(existingPlayer, entity, out var _);
            return;
        }

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
            Name = !string.IsNullOrWhiteSpace(entity.Name) ? entity.Name : existingEntity.Name,
            Sprite = entity.Sprite ?? existingEntity.Sprite,
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
        using var _ = _lock.EnterScope();
        
        var wasRemoved = _entities.Remove(id, out entity);
        if (wasRemoved)
        {
            OnEntityRemoved(entity);
        }

        return wasRemoved;
    }

    private void AddOrUpdatePlayerEntity(GameEntity? oldValue, GameEntity newValue, out bool wasReplaced)
    {
        wasReplaced = false;

        if (oldValue is not null)
        {
            _playerEntities.Remove(oldValue.Value.Name ?? string.Empty);
            RemoveEntity(oldValue.Value.Id, out _);

            OnEntityRemoved(oldValue.Value);
            wasReplaced = true;
        }

        if (_entities.TryAdd(newValue.Id, newValue))
        {
            if (!string.IsNullOrWhiteSpace(newValue.Name))
            {
                _playerEntities.TryAdd(newValue.Name ?? string.Empty, newValue);
            }

            OnEntityAdded(newValue);
        }
    }

    private void OnEntityAdded(GameEntity entity) => EntityAdded?.Invoke(entity);
    private void OnEntityUpdated(GameEntity entity) => EntityUpdated?.Invoke(entity);
    private void OnEntityRemoved(GameEntity entity) => EntityRemoved?.Invoke(entity);
}