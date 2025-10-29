using System;
using Arbiter.App.Models.Player;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Player;

public sealed class PlayerSpellSlotViewModel : ViewModelBase
{
    private readonly SpellbookItem? _spell;

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

    public bool IsEmpty => _spell is null;
    public string Name => _spell?.Name ?? string.Empty;
    public SpellTargetType TargetType => _spell?.TargetType ?? SpellTargetType.None;
    public ushort Sprite => _spell?.Sprite ?? 0;
    public int CurrentLevel => _spell?.CurrentLevel ?? 0;
    public int MaxLevel => _spell?.MaxLevel ?? 0;
    public TimeSpan Cooldown => _spell?.Cooldown ?? TimeSpan.Zero;
    public bool HasLevel => _spell?.MaxLevel > 0;
    public bool HasCooldown => Cooldown > TimeSpan.Zero;
    public int CastLines => _spell?.CastLines ?? 0;
    public string CastLinesText => CastLines switch
    {
        0 => "Instant",
        1 => "1 line",
        _ => $"{CastLines} lines"
    };

    public string TargetTypeText => TargetType switch
    {
        SpellTargetType.Target => "Target",
        SpellTargetType.Prompt => "Text Prompt",
        SpellTargetType.PromptOneNumber => "Numeric Prompt",
        SpellTargetType.PromptTwoNumbers => "Numeric Prompt (2)",
        SpellTargetType.PromptThreeNumbers => "Numeric Prompt (3)",
        SpellTargetType.PromptFourNumbers => "Numeric Prompt (4)",
        _ => "No Target"
    };

    public PlayerSpellSlotViewModel(int slot, SpellbookItem? spell = null)
    {
        Slot = slot;
        _spell = spell;
    }
}