using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string DebugShowDialogFilterName = "Debug_ShowDialogFilter";
    private const string DebugShowDialogMenuFilterName = "Debug_ShowDialogMenuFilter";
    
    private NetworkFilterRef? _debugDialogFilter;
    private NetworkFilterRef? _debugDialogMenuFilter;
    
    private void AddDebugDialogFilters(DebugSettings settings)
    {
        if (!settings.ShowDialogId)
        {
            return;
        }

        _debugDialogFilter = _proxyServer.AddMessageFilter(new ServerMessageFilter<ServerShowDialogMessage>(HandleDialogMessage, settings)
        {
            Name = DebugShowDialogFilterName,
            Priority = DebugFilterPriority
        });

        _debugDialogMenuFilter = _proxyServer.AddMessageFilter(
            new ServerMessageFilter<ServerShowDialogMenuMessage>(HandleDialogMenuMessage, settings)
            {
                Name = DebugShowDialogMenuFilterName,
                Priority = DebugFilterPriority
            });
    }

    private void RemoveDebugDialogFilters()
    {
        _debugDialogFilter?.Unregister();
        _debugDialogMenuFilter?.Unregister();
    }

    private static NetworkPacket HandleDialogMessage(ProxyConnection connection, ServerShowDialogMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowDialogMessage> result)
    {
        if (parameter is not DebugSettings { ShowDialogId: true })
        {
            return result.Passthrough();
        }

        var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
        message.Name = $"{name} [0x{message.EntityId:X4}]";

        return result.Replace(message);
    }

    private static NetworkPacket HandleDialogMenuMessage(ProxyConnection connection,
        ServerShowDialogMenuMessage message, object? parameter,
        NetworkMessageFilterResult<ServerShowDialogMenuMessage> result)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (parameter is not DebugSettings { ShowDialogId: false })
        {
            return result.Passthrough();
        }

        var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
        message.Name = $"{name} [0x{message.EntityId:X4}]";

        return result.Replace(message);
    }
}