using System.Linq;
using Arbiter.Net.Types;

namespace Arbiter.App.Models.Player;

public readonly struct SpellCastParameters
{
    public SpellTargetType TargetType { get; init; }
    public long CasterId { get; init; }
    public long TargetId { get; init; }
    public int TargetX { get; init; }
    public int TargetY { get; init; }
    public object[] Arguments { get; init; }

    public static SpellCastParameters NoTarget(long casterId) => new()
        { CasterId = casterId, TargetType = SpellTargetType.NoTarget };

    public static SpellCastParameters Target(long casterId, long targetId, int targetX, int targetY) => new()
    {
        CasterId = casterId, TargetType = SpellTargetType.Target, TargetId = targetId, TargetX = targetX,
        TargetY = targetY
    };

    public static SpellCastParameters TextInput(long casterId, string textInput) => new()
        { CasterId = casterId, TargetType = SpellTargetType.Target, Arguments = [textInput] };

    public static SpellCastParameters NumericInput(long casterId, params int[] values) => new()
        { CasterId = casterId, TargetType = SpellTargetType.Target, Arguments = values.Cast<object>().ToArray() };
}