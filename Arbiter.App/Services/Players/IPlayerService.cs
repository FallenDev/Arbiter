using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Arbiter.App.Models.Player;

namespace Arbiter.App.Services.Players;

public interface IPlayerService
{
    IEnumerable<PlayerState> AllPlayers { get; }
    
    void Register(int connectionId, PlayerState state);
    bool Unregister(int connectionId);

    bool TryGetState(int connectionId, [NotNullWhen(true)] out PlayerState? state);
}