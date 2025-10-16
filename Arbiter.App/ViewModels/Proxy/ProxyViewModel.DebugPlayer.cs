using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private NetworkFilterRef? _debugShowUserFilter;

    private void AddDebugPlayerFilters(DebugSettings settings)
    {
        _debugShowUserFilter = _proxyServer.AddFilter(
            new ServerMessageFilter<ServerShowUserMessage>(HandleShowUserMessage, settings)
            {
                Name = "Debug_ShowUserFilter",
                Priority = DebugFilterPriority
            });
    }

    private void RemoveDebugPlayerFilters()
    {
        _debugShowUserFilter?.Unregister();
    }

    private static NetworkPacket HandleShowUserMessage(ProxyConnection connection, ServerShowUserMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowUserMessage> result)
    {
        if (parameter is not DebugSettings filterSettings || filterSettings is
                { ShowHiddenPlayers: false, ShowPlayerNames: false })
        {
            return result.Passthrough();
        }

        var hasChanges = false;
        if (message.IsHidden)
        {
            message.Name = "[Hidden]";
            message.NameStyle = NameTagStyle.Hostile;
            message.BodySprite = BodySprite.MaleInvisible;
            message.IsTranslucent = true;
            message.IsHidden = false;

            hasChanges = true;
        }
        else if (filterSettings.ShowPlayerNames)
        {
            // Always show names instead of mouse over
            message.NameStyle = NameTagStyle.Neutral;
            hasChanges = true;
        }

        return hasChanges ? result.Replace(message) : result.Passthrough();
    }
}