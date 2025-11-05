using Arbiter.App.Collections;
using Arbiter.App.Models.Player;

namespace Arbiter.App.ViewModels.Player;

public sealed class DesignPlayerSpellbookViewModel : PlayerSpellbookViewModel
{
    public DesignPlayerSpellbookViewModel()
        : base(CreateTestSpellbook())
    {
    }

    private static ISlottedCollection<SpellbookItem> CreateTestSpellbook()
    {
        var spellbook = new SlottedCollection<SpellbookItem>(PlayerState.MaxTemuairSpells +
                                                             PlayerState.MaxMedeniaSpells + PlayerState.MaxWorldSpells);



        return spellbook;
    }
}