using Arbiter.App.Models;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    public void ApplyDebugFilters(DebugSettings settings)
    {
        RemoveDebugFilters();

        if (settings.ShowMonsterId || settings.ShowNpcId)
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

        if (settings.EnableTabMap || settings.DisableWeatherEffects)
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
        RemoveDebugMapFilters();
        RemoveDebugMessageFilters();

        _logger.LogInformation("Debug packet filters disabled");
    }
}