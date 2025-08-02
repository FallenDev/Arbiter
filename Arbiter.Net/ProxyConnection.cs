using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace Arbiter.Net;

public class ProxyConnection : IDisposable
{
    private const int RecvBufferSize = 4096;
    
    private bool _isDisposed;
    private readonly TcpClient _client;
    private TcpClient? _server;
    private NetworkStream? _clientStream;
    private NetworkStream? _serverStream;
    private readonly Channel<QueuedNetworkPacket> _sendQueue = Channel.CreateUnbounded<QueuedNetworkPacket>();
    
    public int Id { get; }
    public IPEndPoint? LocalEndpoint => _client.Client.LocalEndPoint as IPEndPoint;
    public IPEndPoint? RemoteEndpoint => _client.Client.RemoteEndPoint as IPEndPoint;
    public bool IsConnected => IsClientConnected && IsServerConnected;
    public bool IsClientConnected => _client.Connected;
    public bool IsServerConnected => _server?.Connected ?? false;

    public event EventHandler? ServerConnected;
    public event EventHandler? ServerDisconnected;
    
    public event EventHandler<NetworkPacketEventArgs>? PacketReceived;
    public event EventHandler<NetworkPacketEventArgs>? PacketSent;
    
    public ProxyConnection(int id, TcpClient client)
    {
        Id = id;
        
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
        
        ServerConnected?.Invoke(this, EventArgs.Empty);
    }

    internal Task SendRecvLoopAsync(CancellationToken token = default)
    {
        var linked = CancellationTokenSource.CreateLinkedTokenSource(token);

        var clientRecvTask = RecvLoopAsync(_clientStream!, ProxyDirection.ClientToServer, linked.Token);
        var serverRecvTask = RecvLoopAsync(_serverStream!, ProxyDirection.ServerToClient, linked.Token);
        var senderTask = SendLoopAsync(linked.Token);

        return Task.WhenAll(clientRecvTask, serverRecvTask, senderTask);
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
                    if (direction == ProxyDirection.ServerToClient)
                    {
                        ServerDisconnected?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                }

                parser.Append(recvBuffer, 0, recvCount);

                while (parser.TryTakePacket(out var packet))
                {
                    var queuedPacket = new QueuedNetworkPacket(packet, direction);
                    PacketReceived?.Invoke(this, new NetworkPacketEventArgs(queuedPacket.Packet, queuedPacket.Direction));

                    await _sendQueue.Writer.WriteAsync(queuedPacket, token).ConfigureAwait(false);
                }
            }
        }
        catch when (token.IsCancellationRequested)
        {
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(recvBuffer);
            _sendQueue.Writer.TryComplete();
        }
    }

    private async Task SendLoopAsync(CancellationToken token = default)
    {
        var headerBuffer = ArrayPool<byte>.Shared.Rent(NetworkPacket.HeaderSize);

        try
        {
            await foreach (var queuedPacket in _sendQueue.Reader.ReadAllAsync(token))
            {
                var destinationStream = queuedPacket.Direction switch
                {
                    ProxyDirection.ClientToServer => _serverStream!,
                    ProxyDirection.ServerToClient => _clientStream!,
                    _ => null
                };

                if (destinationStream is null)
                {
                    continue;
                }

                await queuedPacket.Packet.WriteToAsync(destinationStream, headerBuffer.AsMemory(), token)
                    .ConfigureAwait(false);

                PacketSent?.Invoke(this,
                    new NetworkPacketEventArgs(queuedPacket.Packet, queuedPacket.Direction));
            }
        }
        catch when (token.IsCancellationRequested)
        {
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(headerBuffer);
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