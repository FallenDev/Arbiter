using System;
using System.Collections.Generic;
using Arbiter.App.Models;

namespace Arbiter.App.Services;

public interface IEntityStore
{
    event Action<GameEntity>? EntityAdded;
    event Action<GameEntity>? EntityUpdated;
    event Action<GameEntity>? EntityRemoved;
    
    bool IsEmpty { get; }
    int Count { get; }

    IEnumerable<GameEntity> Entities { get; }

    bool TryGetEntity(long id, out GameEntity entity);
    
    void AddOrUpdateEntity(GameEntity entity, out bool wasUpdated);
    
    bool RemoveEntity(long id, out GameEntity entity);
}