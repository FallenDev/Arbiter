using System;
using Arbiter.Net.Types;

namespace Arbiter.App.Models.Player;

public sealed class SpellbookItem : ISlotted
{
    public int Slot { get; init; }
    public ushort Sprite { get; init; }
    public string Name { get; init; }
    public SpellTargetType TargetType { get; init; }
    public int CurrentLevel { get; init; }
    public int MaxLevel { get; init; }
    public int CastLines { get; init; }
    public TimeSpan Cooldown { get; init; }
    public bool IsVirtual { get; init; }
    public Action<SpellCastParameters>? OnCast { get; init; }

    public SpellbookItem(int slot, ushort sprite, string name, SpellTargetType targetType, int castLines = 0,
        int currentLevel = 0,
        int maxLevel = 0, TimeSpan? cooldown = null)
    {
        Slot = slot;
        Sprite = sprite;
        Name = name;
        TargetType = targetType;
        CastLines = castLines;
        CurrentLevel = currentLevel;
        MaxLevel = maxLevel;
        Cooldown = cooldown ?? TimeSpan.Zero;
    }

    public override string ToString()
        => MaxLevel > 0 ? $"{Name} (Level: {CurrentLevel}/{MaxLevel})" : Name;
}