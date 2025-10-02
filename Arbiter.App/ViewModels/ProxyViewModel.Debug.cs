using Arbiter.App.Models;
using Arbiter.Net.Filters;
using Arbiter.Net.Server;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class ProxyViewModel
{
    private const string DebugAddEntityFilterName = "Debug_AddEntityFilter";
    private const string DebugShowDialogFilterName = "Debug_ShowDialogFilter";
    private const string DebugShowDialogMenuFilterName = "Debug_ShowDialogMenuFilter";
    
    public bool DebugFiltersEnabled { get; private set; }
    
    public void ApplyDebugFilters(DebugSettings settings)
    {
        // Add debug filters (named filters will be replaced, not duplicated)
        _proxyServer.AddFilter(ServerCommand.AddEntity, new NetworkPacketFilter(HandleAddEntityPacket, settings)
        {
            Name = DebugAddEntityFilterName,
            Priority = int.MaxValue
        });
        
        _proxyServer.AddFilter(ServerCommand.ShowDialog, new NetworkPacketFilter(HandleDialogPacket, settings)
        {
            Name = DebugShowDialogFilterName,
            Priority = int.MaxValue
        });

        _proxyServer.AddFilter(ServerCommand.ShowDialogMenu, new NetworkPacketFilter(HandleDialogMenuPacket, settings)
        {
            Name = DebugShowDialogMenuFilterName,
            Priority = int.MaxValue
        });
        
        DebugFiltersEnabled = true;
        _logger.LogInformation("Debug packet filters enabled");
    }

    public void RemoveDebugFilters()
    {
        if (!DebugFiltersEnabled)
        {
            return;
        }
        
        // Remove debug filters
        _proxyServer.RemoveFilter(ServerCommand.AddEntity, DebugAddEntityFilterName);
        _proxyServer.RemoveFilter(ServerCommand.ShowDialog, DebugShowDialogFilterName);
        _proxyServer.RemoveFilter(ServerCommand.ShowDialogMenu, DebugShowDialogMenuFilterName);
        
        DebugFiltersEnabled = false;
        _logger.LogInformation("Debug packet filters disabled");
    }
}