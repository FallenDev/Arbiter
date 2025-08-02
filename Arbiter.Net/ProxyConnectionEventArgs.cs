namespace Arbiter.Net;

public class ProxyConnectionEventArgs(ProxyConnection connection) : EventArgs
{
    public ProxyConnection Connection { get; } = connection;
}