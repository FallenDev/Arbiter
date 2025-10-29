using System;

namespace Arbiter.App.Models.Player;

public sealed class PlayerSpellbook : SlottedCollection<SpellbookItem>
{
    public const int TemuairSpellCount = 36;
    public const int MedeniaSpellCount = 36;
    public const int WorldSpellCount = 18;

    public PlayerSpellbook() : base(TemuairSpellCount + MedeniaSpellCount + WorldSpellCount)
    {
    }

    public bool HasSpell(string name) => TryFind(name, out _);

    public int FindSpell(string name) => TryFind(name, out var item) ? item.Slot : -1;
    public int GetCastLines(string name) => TryFind(name, out var item) ? item.CastLines : -1;

    public bool UpdateCooldown(int slot, TimeSpan duration)
    {
        if (slot < 1 || slot > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(slot),
                "Slot must be between 1 and the capacity of the collection");
        }

        var item = GetSlot(slot);
        if (item is null || item.Cooldown == duration)
        {
            return false;
        }

        var update = new SpellbookItem(slot, item.Sprite, item.Name, item.TargetType, item.CastLines, item.CurrentLevel,
            item.MaxLevel, duration);
        SetSlot(slot, update);
        return true;
    }
}