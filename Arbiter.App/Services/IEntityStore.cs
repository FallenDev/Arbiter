using System;
using System.Collections.Generic;
using Arbiter.App.Models;

namespace Arbiter.App.Services;

public interface IEntityStore
{
    bool IsEmpty { get; }
    int Count { get; }

    IEnumerable<GameEntity> Entities { get; }

    event Action<GameEntity>? EntityAdded;
    event Action<GameEntity>? EntityUpdated;
    event Action<GameEntity>? EntityRemoved;
}