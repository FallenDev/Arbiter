using System.Collections.Generic;
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
    public string? TextInput { get; init; }
    public IReadOnlyList<ushort>? NumericInputs { get; init; }

    public static SpellCastParameters NoTarget(long casterId) => new()
        { CasterId = casterId, TargetType = SpellTargetType.NoTarget };

    public static SpellCastParameters Target(long casterId, long targetId, int targetX, int targetY) => new()
    {
        CasterId = casterId, TargetType = SpellTargetType.Target, TargetId = targetId, TargetX = targetX,
        TargetY = targetY
    };

    public static SpellCastParameters WithTextInput(long casterId, string textInput) => new()
        { CasterId = casterId, TargetType = SpellTargetType.Target, TextInput = textInput };

    public static SpellCastParameters WithNumericInput(long casterId, IEnumerable<ushort> values) => new()
        { CasterId = casterId, TargetType = SpellTargetType.Target, NumericInputs = values.ToList() };
}