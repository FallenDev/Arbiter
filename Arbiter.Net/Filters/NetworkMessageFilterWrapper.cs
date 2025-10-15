using Arbiter.Net.Proxy;

namespace Arbiter.Net.Filters;

internal abstract class NetworkMessageFilterWrapper
{
    public Type MessageType { get; }
    public string? Name { get; }
    public int Priority { get; }
    public object? Parameter { get; }

    public NetworkMessageFilterWrapper(Type messageType, string? name, int priority, object? parameter = null)
    {
        MessageType = messageType;
        Name = name;
        Priority = priority;
        Parameter = parameter;
    }

    public abstract object? Invoke(ProxyConnection connection, object message);
}

internal class NetworkMessageFilterWrapper<T> : NetworkMessageFilterWrapper where T : class
{
    private readonly NetworkMessageFilterHandler<T> _handler;

    public NetworkMessageFilterWrapper(INetworkMessageFilter<T> filter)
        : base(typeof(T), filter.Name, filter.Priority, filter.Parameter)
    {
        _handler = filter.Handler;
    }

    public override object? Invoke(ProxyConnection connection, object message)
    {
        return _handler(connection, (T)message, Parameter);
    }
}