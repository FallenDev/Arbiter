namespace Arbiter.Net.Filters;

public class NetworkMessageFilter<T> : INetworkMessageFilter<T> where T : class
{
    public string? Name { get; set; }
    public int Priority { get; set; } = 10;
    public object? Parameter { get; }
    public NetworkMessageFilterHandler<T> Handler { get; }

    public NetworkMessageFilter(NetworkMessageFilterHandler<T> handler, object? parameter = null)
    {
        Handler = handler;
        Parameter = parameter;
    }
}