using Arbiter.App.Models.Player;

namespace Arbiter.App.ViewModels.Player;

public sealed class DesignPlayerSkillbookViewModel : PlayerSkillbookViewModel
{
    public DesignPlayerSkillbookViewModel()
        : base(CreateTestSkillbook())
    {
    }

    public static PlayerSkillbook CreateTestSkillbook()
    {
        var skillbook = new PlayerSkillbook();

        var skill = new SkillbookItem(1, 1, "Assail", 80, 100);
        skillbook.SetSlot(1, skill);

        return skillbook;
    }
}