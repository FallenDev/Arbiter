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
        _logger.LogInformation("{}");
        // Do not alter the packet
        return result.Passthrough();
    }
    
    private NetworkPacket OnDialogMenuMessage(ProxyConnection connection, ServerShowDialogMenuMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowDialogMenuMessage> result)
    {
        // Do not alter the packet
        return result.Passthrough();
    }
}