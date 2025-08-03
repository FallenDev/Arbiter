using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Arbiter.Net.Client;
using Arbiter.Net.Server;

namespace Arbiter.Net;

public class ProxyConnection : IDisposable
{
    private const int RecvBufferSize = 4096;

    private bool _isDisposed;
    private readonly TcpClient _client;

    private NetworkStream? _clientStream;
    private readonly NetworkPacketBuffer _clientPacketBuffer = new((command, data) => new ClientPacket(command, data));
    private readonly ClientPacketEncryptor _clientEncryptor = new();

    private TcpClient? _server;
    private NetworkStream? _serverStream;
    private readonly NetworkPacketBuffer _serverPacketBuffer = new((command, data) => new ServerPacket(command, data));
    private readonly ServerPacketEncryptor _serverEncryptor = new();

    private readonly Channel<NetworkPacket> _sendQueue = Channel.CreateUnbounded<NetworkPacket>();

    public int Id { get; }
    public IPEndPoint? LocalEndpoint => _client.Client.LocalEndPoint as IPEndPoint;
    public IPEndPoint? RemoteEndpoint => _server?.Client.RemoteEndPoint as IPEndPoint;
    public bool IsConnected => IsClientConnected && IsServerConnected;
    public bool IsClientConnected => _client.Connected;
    public bool IsServerConnected => _server?.Connected ?? false;

    public event EventHandler? ClientDisconnected;
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

    internal async Task ConnectToRemoteAsync(IPEndPoint remoteEndpoint, CancellationToken token = default)
    {
        _server = new TcpClient(AddressFamily.InterNetwork) { NoDelay = true };
        
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token);
        await _server.ConnectAsync(remoteEndpoint, linked.Token).ConfigureAwait(false);

        _clientStream = _client.GetStream();
        _serverStream = _server.GetStream();

        ServerConnected?.Invoke(this, EventArgs.Empty);
    }

    internal Task SendRecvLoopAsync(CancellationToken token = default)
    {
        var linked = CancellationTokenSource.CreateLinkedTokenSource(token);

        var clientRecvTask = RecvLoopAsync(_clientStream!, _clientPacketBuffer, _clientEncryptor,
            ProxyDirection.ClientToServer, linked);
        var serverRecvTask = RecvLoopAsync(_serverStream!, _serverPacketBuffer, _serverEncryptor,
            ProxyDirection.ServerToClient, linked);
        var senderTask = SendLoopAsync(linked.Token);

        return Task.WhenAll(clientRecvTask, serverRecvTask, senderTask);
    }

    private async Task RecvLoopAsync(NetworkStream stream, NetworkPacketBuffer packetBuffer,
        NetworkPacketEncryptor encryptor, ProxyDirection direction, CancellationTokenSource tokenSource)
    {
        var token = tokenSource.Token;
        var recvBuffer = ArrayPool<byte>.Shared.Rent(RecvBufferSize);

        try
        {
            while (!token.IsCancellationRequested)
            {
                int recvCount;
                try
                {
                    recvCount = await stream.ReadAsync(recvBuffer, token).ConfigureAwait(false);
                }
                catch when (token.IsCancellationRequested)
                {
                    recvCount = 0;
                }

                if (recvCount == 0)
                {
                    if (direction == ProxyDirection.ServerToClient)
                    {
                        ServerDisconnected?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        ClientDisconnected?.Invoke(this, EventArgs.Empty);
                    }

                    await tokenSource.CancelAsync();
                    break;
                }

                packetBuffer.Append(recvBuffer, 0, recvCount);

                while (packetBuffer.TryTakePacket(out var packet))
                {
                    var decryptedPacket = encryptor.Decrypt(packet);

                    PacketReceived?.Invoke(this, new NetworkPacketEventArgs(decryptedPacket, direction));
                    await _sendQueue.Writer.WriteAsync(packet, token).ConfigureAwait(false);
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
            await foreach (var packet in _sendQueue.Reader.ReadAllAsync(token))
            {
                var destinationStream = packet switch
                {
                    ClientPacket => _serverStream!,
                    ServerPacket => _clientStream!,
                    _ => null
                };

                if (destinationStream is null)
                {
                    continue;
                }

                NetworkPacketEncryptor? encryptor = packet switch
                {
                    ClientPacket => _clientEncryptor,
                    ServerPacket => _serverEncryptor,
                    _ => null
                };

                var encryptedPacket = encryptor?.Encrypt(packet) ?? packet;
                
                await encryptedPacket.WriteToAsync(destinationStream, headerBuffer.AsMemory(), token)
                    .ConfigureAwait(false);

                var outgoingDirection = packet switch
                {
                    ClientPacket => ProxyDirection.ServerToClient,
                    _ => ProxyDirection.ClientToServer
                };

                PacketSent?.Invoke(this,
                    new NetworkPacketEventArgs(packet, outgoingDirection));
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
            _sendQueue.Writer.TryComplete();

            _clientStream?.Dispose();
            _client.Dispose();

            _serverStream?.Dispose();
            _server?.Dispose();
        }

        _isDisposed = true;
    }
}