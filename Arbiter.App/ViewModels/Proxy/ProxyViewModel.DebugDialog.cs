using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private NetworkFilterRef? _debugDialogFilter;
    private NetworkFilterRef? _debugDialogMenuFilter;

    private void AddDebugDialogFilters(DebugSettings settings)
    {
        if (!settings.ShowDialogId)
        {
            return;
        }

        _debugDialogFilter = _proxyServer.AddFilter(
            new ServerMessageFilter<ServerShowDialogMessage>(HandleDialogMessage, settings)
            {
                Name = "Debug_ShowDialogFilter",
                Priority = DebugFilterPriority
            });

        _debugDialogMenuFilter = _proxyServer.AddFilter(
            new ServerMessageFilter<ServerShowDialogMenuMessage>(HandleDialogMenuMessage, settings)
            {
                Name = "Debug_ShowDialogMenuFilter",
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
        if (parameter is not DebugSettings { ShowDialogId: true })
        {
            return result.Passthrough();
        }

        var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
        message.Name = $"{name} [0x{message.EntityId:X4}]";

        return result.Replace(message);
    }
}