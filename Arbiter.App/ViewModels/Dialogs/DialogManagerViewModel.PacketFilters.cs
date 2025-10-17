using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Dialogs;

public partial class DialogManagerViewModel
{
    private void AddPacketFilters()
    {
        _proxyServer.AddFilter(new ServerMessageFilter<ServerShowDialogMessage>(OnDialogMessage)
        {
            Name = "DialogManager_ShowDialogMessage",
            Priority = int.MaxValue
        });
        
        _proxyServer.AddFilter(new ServerMessageFilter<ServerShowDialogMenuMessage>(OnDialogMenuMessage)
        {
            Name = "DialogManager_ShowDialogMenuMessage",
            Priority = int.MaxValue
        });
    }

    private NetworkPacket OnDialogMessage(ProxyConnection connection, ServerShowDialogMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowDialogMessage> result)
    {
        if (_clientManager.TryGetClient(connection.Id, out var client))
        {
            _logger.LogInformation("[{Client}] Received dialog message: {Message}", connection.Name, message.Name);
        }
        else
        {
            _logger.LogWarning("Unable to find client with ID {Id}", connection.Id);
        }
        
        // Do not alter the packet
        return result.Passthrough();
    }
    
    private NetworkPacket OnDialogMenuMessage(ProxyConnection connection, ServerShowDialogMenuMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowDialogMenuMessage> result)
    {
        if (_clientManager.TryGetClient(connection.Id, out var client))
        {
            _logger.LogInformation("[{Client}] Received dialog menu: {Message}", connection.Name, message.Name);
        }
        else
        {
            _logger.LogWarning("Unable to find client with ID {Id}", connection.Id);
        }
        
        // Do not alter the packet
        return result.Passthrough();
    }
}