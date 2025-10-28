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

        skillbook.SetSlot(1, new SkillbookItem
        {
            Slot = 1,
            Name = "Assail",
            Sprite = 13,
            CurrentLevel = 80,
            MaxLevel = 100
        });
        
        return skillbook;
    }
}