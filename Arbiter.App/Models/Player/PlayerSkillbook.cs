
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
}