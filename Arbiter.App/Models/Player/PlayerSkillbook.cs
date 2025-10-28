
using System;

namespace Arbiter.App.Models.Player;

public sealed class PlayerSkillbook : SlottedCollection<SkillbookItem>
{
    public const int TemuairSkillCount = 36;
    public const int MedeniaSkillCount = 36;
    public const int WorldSkillCount = 18;

    public PlayerSkillbook()
        : base(TemuairSkillCount + MedeniaSkillCount + WorldSkillCount)
    {

    }

    public bool HasSkill(string name) => TryFind(name, out _);
    public int FindSkill(string name) => TryFind(name, out var item) ? item.Slot : -1;

    public bool UpdateCooldown(int slot, TimeSpan duration)
    {
        if (slot < 1 || slot > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(slot),
                "Slot must be between 1 and the capacity of the collection");
        }

        var item = GetSlot(slot);
        if (item.Cooldown == duration)
        {
            return false;
        }
        
        var update = item with
        {
            Cooldown = duration
        };
        SetSlot(slot, update);
        return true;
    }
}