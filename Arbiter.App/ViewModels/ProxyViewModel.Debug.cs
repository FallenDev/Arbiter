using Arbiter.App.Models;
using Arbiter.Net.Filters;
using Arbiter.Net.Server;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class ProxyViewModel
{
    public void ApplyDebugFilters(DebugSettings settings)
    {
        RemoveDebugFilters();
        
        // Add debug filters (named filters will be replaced, not duplicated)
        if (settings.ShowMonsterId || settings.ShowNpcId)
        {
            AddDebugEntityFilters(settings);
        }

        if (settings.ShowDialogId)
        {
            AddDebugDialogFilters(settings);
        }

        if (settings.ShowHiddenPlayers)
        {
            AddDebugPlayerFilters(settings);
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

        _logger.LogInformation("Debug packet filters disabled");
    }
}