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
}