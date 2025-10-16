using System.Collections.Generic;
using Arbiter.App.Models;

namespace Arbiter.App.Services;

public interface IPlayerService
{
    IEnumerable<PlayerState> AllPlayers { get; }
    
    void Register(int connectionId, PlayerState state);
    bool Unregister(int connectionId);

    bool TryGetState(int connectionId, out PlayerState state);
}