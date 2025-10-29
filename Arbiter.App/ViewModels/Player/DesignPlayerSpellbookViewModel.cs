using Arbiter.App.Models.Player;

namespace Arbiter.App.ViewModels.Player;

public sealed class DesignPlayerSpellbookViewModel : PlayerSpellbookViewModel
{
    public DesignPlayerSpellbookViewModel()
        : base(CreateTestSpellbook())
    {
    }

    private static PlayerSpellbook CreateTestSpellbook()
    {
        var spellbook = new PlayerSpellbook();

        
        
        return spellbook;
    }
}