using System.Collections.Generic;
using Arbiter.App.Models;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string FilterPrefix = nameof(ProxyViewModel);
    
    // Almost highest, but allows for other filters to run first
    private const int DebugFilterPriority = int.MaxValue - 100;

    public void ApplyDebugFilters(DebugSettings settings, List<MessageFilter> filters)
    {
        RemoveDebugFilters();

        // Ideally, these would all be moved to Lua scripts
        // For now they are done in code for simplicity

        AddDebugEntityFilters(settings);
        AddDebugDialogFilters(settings);
        AddDebugPlayerFilters(settings);
        AddDebugEffectsFilters(settings);
        AddDebugMapFilters(settings);
        AddDebugMessageFilters(settings, filters);
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