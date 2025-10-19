using Arbiter.Net.Proxy;

namespace Arbiter.Net.Observers;

internal interface IObserverRegistration
{
    bool IsActive { get; set; }
    Task HandleAsync(ProxyConnection connection, INetworkMessage message);
}