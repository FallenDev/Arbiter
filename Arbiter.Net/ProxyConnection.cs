using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace Arbiter.Net;

public class ProxyConnection : IDisposable
{
    private const int RecvBufferSize = 4096;
    private const int SendBufferSize = 4096;
    
    private bool _isDisposed;
    private readonly TcpClient _client;
    private TcpClient? _server;
    private NetworkStream? _clientStream;
    private NetworkStream? _serverStream;
    private readonly Channel<QueuedNetworkPacket> _sendQueue = Channel.CreateUnbounded<QueuedNetworkPacket>();
    
    public ProxyConnection(TcpClient client)
    {
        _client = client;
        _client.NoDelay = true;
    }

    internal async Task ConnectToRemoteAsync(IPEndPoint remoteEndpoint, CancellationToken token=default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token);
        _server = new TcpClient { NoDelay = true };

        await _server.ConnectAsync(remoteEndpoint, linked.Token).ConfigureAwait(false);
        
        _clientStream = _client.GetStream();
        _serverStream = _server.GetStream();
    }

    internal Task SendRecvLoopAsync(CancellationToken token = default)
    {
        var linked = CancellationTokenSource.CreateLinkedTokenSource(token);

        var clientRecvTask = RecvLoopAsync(_clientStream!, ProxyDirection.ClientToServer, linked.Token);
        var serverRecvTask = RecvLoopAsync(_serverStream!, ProxyDirection.ServerToClient, linked.Token);

        return Task.WhenAll(clientRecvTask, serverRecvTask);
    }

    private async Task RecvLoopAsync(NetworkStream stream, ProxyDirection direction, CancellationToken token = default)
    {
        var parser = new NetworkPacketParser();
        var recvBuffer = ArrayPool<byte>.Shared.Rent(RecvBufferSize);

        try
        {
            while (!token.IsCancellationRequested)
            {
                var recvCount = await stream.ReadAsync(recvBuffer, token).ConfigureAwait(false);

                if (recvCount == 0)
                {
                    break;
                }

                parser.Append(recvBuffer, 0, recvCount);
            }
        }
        catch when (token.IsCancellationRequested)
        {
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(recvBuffer);
        }
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