using System;

namespace Arbiter.App.Models.Player;

public sealed class SkillbookItem : ISlotted
{
    public int Slot { get; init; }
    public ushort Sprite { get; init; }
    public string Name { get; init; }
    public int CurrentLevel { get; init; }
    public int MaxLevel { get; init; }
    public TimeSpan Cooldown { get; init; }
    public bool IsVirtual { get; init; }
    public Action? OnUse { get; init; }

    public SkillbookItem(int slot, ushort sprite, string name, int currentLevel = 0, int maxLevel = 0,
        TimeSpan? cooldown = null)
    {
        Slot = slot;
        Sprite = sprite;
        Name = name;
        CurrentLevel = currentLevel;
        MaxLevel = maxLevel;
        Cooldown = cooldown ?? TimeSpan.Zero;
    }

    public override string ToString()
        => MaxLevel > 0 ? $"{Name} (Level: {CurrentLevel}/{MaxLevel})" : Name;
}