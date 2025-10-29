using System;
using Arbiter.App.Models.Player;

namespace Arbiter.App.ViewModels.Player;

public sealed class PlayerSkillSlotViewModel : ViewModelBase
{
    private readonly SkillbookItem? _skill;

    public int Slot { get; }

    public int RelativeSlot
    {
        get
        {
            return Slot switch
            {
                <= 36 => Slot,
                <= 72 => Slot - 36,
                _ => Slot - 72
            };
        }
    }
    public bool IsEmpty => _skill is null;
    public string Name => _skill?.Name ?? string.Empty;
    public ushort Sprite => _skill?.Sprite ?? 0;
    public int CurrentLevel => _skill?.CurrentLevel ?? 0;
    public int MaxLevel => _skill?.MaxLevel ?? 0;
    public TimeSpan Cooldown => _skill?.Cooldown ?? TimeSpan.Zero;
    public bool HasLevel => _skill?.MaxLevel > 0;
    public bool HasCooldown => Cooldown > TimeSpan.Zero;

    public PlayerSkillSlotViewModel(int slot, SkillbookItem? skill = null)
    {
        Slot = slot;
        _skill = skill;
    }

    public override string ToString() => IsEmpty ? "<empty>" : Name;
}