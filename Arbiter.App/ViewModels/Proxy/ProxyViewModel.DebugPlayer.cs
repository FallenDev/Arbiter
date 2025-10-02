using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string DebugShowUserFilterName = "Debug_ShowUserFilter";

    private void AddDebugPlayerFilters(DebugSettings settings)
    {
        _proxyServer.AddFilter(ServerCommand.ShowUser, new NetworkPacketFilter(HandleShowUserPacket, settings)
        {
            Name = DebugShowUserFilterName,
            Priority = int.MaxValue
        });
    }

    private void RemoveDebugPlayerFilters() =>
        _proxyServer.RemoveFilter(ServerCommand.ShowUser, DebugShowUserFilterName);

    private NetworkPacket HandleShowUserPacket(NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { ShowHiddenPlayers: false, ShowPlayerNames: false } ||
            !_serverMessageFactory.TryCreate<ServerShowUserMessage>(serverPacket, out var message) || !message.IsHidden)
        {
            return packet;
        }

        if (message.IsHidden)
        {
            message.Name = "[Hidden]";
            message.NameStyle = NameTagStyle.Hostile;
            message.BodySprite = BodySprite.MaleInvisible;
            message.IsTranslucent = true;
            message.IsHidden = false;
        }
        else if (filterSettings.ShowPlayerNames)
        {
            // Always show names instead of mouse over
            message.NameStyle = NameTagStyle.Neutral;
        }

        // Build a new packet with the modified user data
        var builder = new NetworkPacketBuilder(ServerCommand.ShowUser);
        message.Serialize(builder);

        return builder.ToPacket();
    }
}