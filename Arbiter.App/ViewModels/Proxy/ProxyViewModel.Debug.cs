using Arbiter.App.Models;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    public void ApplyDebugFilters(DebugSettings settings)
    {
        RemoveDebugFilters();
        
        // Ideally, these would all be moved to Lua scripts
        // For now they are done in code for simplicity

        if (settings.ShowNpcId || settings.ShowMonsterId || settings.ShowMonsterClickId)
        {
            AddDebugEntityFilters(settings);
        }

        if (settings.ShowDialogId)
        {
            AddDebugDialogFilters(settings);
        }

        if (settings.ShowHiddenPlayers || settings.ShowPlayerNames)
        {
            AddDebugPlayerFilters(settings);
        }

        if (settings.UseClassicEffects || settings.DisableBlind)
        {
            AddDebugEffectsFilters(settings);
        }

        if (settings.EnableTabMap || settings.EnableZoomedOutMap || settings.DisableWeatherEffects ||
            settings.DisableDarkness)
        {
            AddDebugMapFilters(settings);
        }

        if (settings.IgnoreEmptyMessages)
        {
            AddDebugMessageFilters(settings);
        }
    }

    public void RemoveDebugFilters()
    {
        RemoveDebugEntityFilters();
        RemoveDebugDialogFilters();
        RemoveDebugPlayerFilters();
        RemoveDebugEffectsFilters();
        RemoveDebugMapFilters();
        RemoveDebugMessageFilters();
    }
}