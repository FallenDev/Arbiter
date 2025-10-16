using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Arbiter.App.Models;

namespace Arbiter.App.Services;

public sealed class PlayerService : IPlayerService
{
    private readonly ConcurrentDictionary<int, PlayerState> _players = new();

    public IEnumerable<PlayerState> AllPlayers => _players.Values;

    public void Register(int connectionId, PlayerState state)
    {
        _players[connectionId] = state;
    }

    public bool Unregister(int connectionId)
    {
        return _players.TryRemove(connectionId, out _);
    }

    public bool TryGetState(int connectionId, [NotNullWhen(true)] out PlayerState? state) =>
        _players.TryGetValue(connectionId, out state);

}