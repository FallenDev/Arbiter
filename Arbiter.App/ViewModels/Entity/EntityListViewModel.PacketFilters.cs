using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels.Entity;

public partial class EntityListViewModel
{
    private readonly IServerMessageFactory _serverMessageFactory = new ServerMessageFactory();
    
    private void AddPacketFilters(ProxyServer proxyServer)
    {
        proxyServer.AddFilter(ServerCommand.AddEntity, new NetworkPacketFilter(OnAddEntityPacket)
        {
            Name = "EntityView_AddEntityFilter",
            Priority = int.MaxValue
        });   
    }

    private NetworkPacket OnAddEntityPacket(ProxyConnection connection, NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type
        if (packet is not ServerPacket serverPacket ||
            !_serverMessageFactory.TryCreate<ServerAddEntityMessage>(serverPacket, out var message))
        {
            return packet;
        }
        
        // Do not alter the packet
        return packet;
    }
}