using System.Net;
using System.Net.Sockets;

namespace Arbiter.Net;

public class ProxyConnection : IDisposable
{
    private bool _isDisposed;
    private readonly TcpClient _client;

    public ProxyConnection(TcpClient client)
    {
        _client = client;
        _client.NoDelay = true;
    }

    internal async Task ConnectToRemoteAsync(IPEndPoint remoteEndpoint, CancellationToken token)
    {
        
    }

    internal async Task SendRecvLoopAsync(CancellationToken token)
    {
        
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (isDisposing)
        {
            _client.Dispose();
        }
        
        _isDisposed = true;
    }
    
    private void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_isDisposed, "Proxy connection is disposed");
}