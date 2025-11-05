using System;
using Arbiter.Net.Types;

namespace Arbiter.App.Models.Player;

public readonly struct SpellbookItem
{
    public ushort Sprite { get; init; }
    public required string Name { get; init; }
    public SpellTargetType TargetType { get; init; }
    public int CurrentLevel { get; init; }
    public int MaxLevel { get; init; }
    public int CastLines { get; init; }
    public string? Prompt { get; init; }
    public TimeSpan Cooldown { get; init; }
    public bool IsVirtual { get; init; }
    public Action<SpellCastParameters>? OnCast { get; init; }

    public override string ToString()
        => MaxLevel > 0 ? $"{Name} (Level: {CurrentLevel}/{MaxLevel})" : Name;
}