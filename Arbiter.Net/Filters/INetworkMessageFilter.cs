using Arbiter.Net.Proxy;

namespace Arbiter.Net.Filters;

public delegate T? NetworkMessageFilterHandler<T>(ProxyConnection connection, T message, object? parameter = null) where T : class;

public interface INetworkMessageFilter<T> where T : class
{
    string? Name { get; set; }
    int Priority { get; set; }
    object? Parameter { get; }
    NetworkMessageFilterHandler<T> Handler { get; }
}