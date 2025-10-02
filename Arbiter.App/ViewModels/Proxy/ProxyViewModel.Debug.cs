using Arbiter.App.Models;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    public void ApplyDebugFilters(DebugSettings settings)
    {
        RemoveDebugFilters();

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

        if (settings.DisableBlind)
        {
            AddDebugEffectsFilters(settings);
        }

        if (settings.EnableTabMap || settings.DisableWeatherEffects || settings.DisableDarkness)
        {
            AddDebugMapFilters(settings);
        }

        if (settings.IgnoreEmptyMessages)
        {
            AddDebugMessageFilters(settings);
        }

        if (settings.CheckEnabled())
        {
            _logger.LogInformation("Debug packet filters enabled");
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

        _logger.LogInformation("Debug packet filters disabled");
    }
}