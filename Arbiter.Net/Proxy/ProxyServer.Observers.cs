using Arbiter.Net.Client;
using Arbiter.Net.Observers;
using Arbiter.Net.Server;

namespace Arbiter.Net.Proxy;

public partial class ProxyServer
{
    private NetworkObserverDispatcher? _observerDispatcher;

    private void NotifyObservers(ProxyConnection connection, NetworkPacket packet)
    {
        switch (packet)
        {
            case ServerPacket serverPacket:
            {
                if (!_serverMessageFactory.TryCreate(serverPacket, out var message))
                {
                    return;
                }

                _observerDispatcher?.TryNotify(connection, message);
                break;
            }
            case ClientPacket clientPacket:
            {
                if (!_clientMessageFactory.TryCreate(clientPacket, out var message))
                {
                    return;
                }

                _observerDispatcher?.TryNotify(connection, message);
                break;
            }
        }
    }
}