using System;

namespace Arbiter.App.Models.Player;

public sealed class SpellbookItem : ISlotted
{
    public int Slot { get; init; }
    public ushort Sprite { get; init; }
    public string Name { get; init; }
    public int CurrentLevel { get; init; }
    public int MaxLevel { get; init; }
    public int CastLines { get; init; }
    public TimeSpan Cooldown { get; init; }

    public SpellbookItem(int slot, ushort sprite, string name, int castLines = 0, int currentLevel = 0,
        int maxLevel = 0, TimeSpan? cooldown = null)
    {
        Slot = slot;
        Sprite = sprite;
        Name = name;
        CastLines = castLines;
        CurrentLevel = currentLevel;
        MaxLevel = maxLevel;
        Cooldown = cooldown ?? TimeSpan.Zero;
    }

    public override string ToString()
        => MaxLevel > 0 ? $"{Name} (Level: {CurrentLevel}/{MaxLevel})" : Name;
}