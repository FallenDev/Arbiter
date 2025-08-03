using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Arbiter.Net.Client;
using Arbiter.Net.Security;
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

    private byte _clientSequence;
    private byte _serverSequence;
    private readonly Channel<NetworkPacket> _sendQueue = Channel.CreateUnbounded<NetworkPacket>();
    
    public int Id { get; }
    public string? Name { get; set; }
    public long? UserId { get; set; }
    public bool IsAuthenticated { get; private set; }
    
    public IPEndPoint? LocalEndpoint => _client.Client.LocalEndPoint as IPEndPoint;
    public IPEndPoint? RemoteEndpoint => _server?.Client.RemoteEndPoint as IPEndPoint;
    public bool IsConnected => IsClientConnected && IsServerConnected;
    public bool IsClientConnected => _client.Connected;
    public bool IsServerConnected => _server?.Connected ?? false;

    public event EventHandler? ClientAuthenticated;
    public event EventHandler? ClientDisconnected;
    public event EventHandler? ServerConnected;
    public event EventHandler? ServerDisconnected;

    public event EventHandler<NetworkRedirectEventArgs>? ClientRedirected;
    public event EventHandler<NetworkPacketEventArgs>? PacketReceived;
    public event EventHandler<NetworkPacketEventArgs>? PacketSent;

    public ProxyConnection(int id, TcpClient client)
    {
        Id = id;

        _client = client;
        _client.NoDelay = true;
    }
    
    public bool EnqueueToClient(ServerPacket packet) =>
        _sendQueue.Writer.TryWrite(packet);
    
    public bool EnqueueToServer(ClientPacket packet) =>
        _sendQueue.Writer.TryWrite(packet);

    internal async Task ConnectToRemoteAsync(IPEndPoint remoteEndpoint, CancellationToken token = default)
    {
        // Create the TCP client to connect to the remote server
        _server = new TcpClient(AddressFamily.InterNetwork) { NoDelay = true };
        
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token);
        await _server.ConnectAsync(remoteEndpoint, linked.Token).ConfigureAwait(false);

        // Get the client and server network streams
        _clientStream = _client.GetStream();
        _serverStream = _server.GetStream();

        ServerConnected?.Invoke(this, EventArgs.Empty);
    }

    internal Task SendRecvLoopAsync(CancellationToken token = default)
    {
        var linked = CancellationTokenSource.CreateLinkedTokenSource(token);

        // Start the client/server recv and send queue tasks in the background
        var clientRecvTask = RecvLoopAsync(_clientStream!, _clientPacketBuffer, _clientEncryptor,
            ProxyDirection.ClientToServer, linked);
        var serverRecvTask = RecvLoopAsync(_serverStream!, _serverPacketBuffer, _serverEncryptor,
            ProxyDirection.ServerToClient, linked);
        var senderTask = SendLoopAsync(linked.Token);

        return Task.WhenAll(clientRecvTask, serverRecvTask, senderTask);
    }

    private async Task RecvLoopAsync(NetworkStream stream, NetworkPacketBuffer packetBuffer,
        INetworkPacketEncryptor encryptor, ProxyDirection direction, CancellationTokenSource tokenSource)
    {
        var token = tokenSource.Token;
        var recvBuffer = ArrayPool<byte>.Shared.Rent(RecvBufferSize);

        try
        {
            while (!token.IsCancellationRequested)
            {
                // Attempt to ready from the stream for any available bytes
                int recvCount;
                try
                {
                    recvCount = await stream.ReadAsync(recvBuffer, token).ConfigureAwait(false);
                }
                catch when (token.IsCancellationRequested)
                {
                    recvCount = 0;
                }

                // Handle socket disconnect when read returns zero
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

                    // This will trigger the other send/recv tasks to also cancel
                    await tokenSource.CancelAsync();
                    break;
                }

                // Append all received bytes into the packet buffer
                packetBuffer.Append(recvBuffer, 0, recvCount);

                // Attempt to dequeue all available packets
                while (packetBuffer.TryTakePacket(out var packet))
                {
                    // Decrypt the packet if necessary
                    var decrypted = encryptor?.IsEncrypted(packet.Command) ?? false
                        ? encryptor.Decrypt(packet)
                        : packet;

                    switch (decrypted)
                    {
                        // Handle server redirects, we need to hijack the redirect
                        case ServerPacket { Command: ServerCommand.Redirect }:
                            HandleServerRedirect(decrypted);
                            break;
                        // Handle server setting user ID, this confirms a valid game connection
                        case ServerPacket { Command: ServerCommand.SetUserId }:
                            HandleServerSetUserId(decrypted);
                            break;
                        // Handle client auth request, we need to update encryption parameters
                        case ClientPacket { Command: ClientCommand.Authenticate }:
                            HandleClientAuthRequest(decrypted);
                            break;
                    }

                    // Raise the event with the decrypted packet
                    PacketReceived?.Invoke(this, new NetworkPacketEventArgs(packet, decrypted.Data));
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
            // Keep polling for any available outgoing packets
            await foreach (var packet in _sendQueue.Reader.ReadAllAsync(token))
            {
                // Determine which stream we need to send to
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

                // Determine which encryption to use based on the outgoing packet type
                INetworkPacketEncryptor? encryptor = packet switch
                {
                    ClientPacket => _clientEncryptor,
                    ServerPacket => _serverEncryptor,
                    _ => null
                };

                // Determine the next sequence number
                var nextSequence = packet switch
                {
                    ClientPacket => _clientSequence++,
                    ServerPacket => _serverSequence++,
                    _ => (byte)0x00
                };

                // Encrypt the packet if necessary
                var encrypted = encryptor?.IsEncrypted(packet.Command) ?? false
                    ? encryptor.Encrypt(packet, nextSequence)
                    : packet;

                await encrypted.WriteToAsync(destinationStream, headerBuffer.AsMemory(), token)
                    .ConfigureAwait(false);

                // Raise the event with the decrypted (plaintext) packet
                PacketSent?.Invoke(this,
                    new NetworkPacketEventArgs(packet, packet.Data));
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

    private void HandleServerRedirect(NetworkPacket packet)
    {
        var reader = new NetworkPacketReader(packet);
        var remoteIpAddress = reader.ReadIPv4Address();
        var remotePort = reader.ReadUInt16();

        reader.ReadByte();

        var seed = reader.ReadByte();
        var keyLength = reader.ReadByte();
        var key = reader.ReadBytes(keyLength);

        var name = reader.ReadString8();
        var clientId = reader.ReadUInt32();

        // Redirect the client to the local endpoint instead
        var localIp = LocalEndpoint!.Address.GetAddressBytes();
        packet.Data[0] = localIp[3];
        packet.Data[1] = localIp[2];
        packet.Data[2] = localIp[1];
        packet.Data[3] = localIp[0];

        var localPort = LocalEndpoint!.Port;
        packet.Data[4] = (byte)((localPort >> 8) & 0xFF);
        packet.Data[5] = (byte)(localPort & 0xFF);

        // Update connection name based on server response
        Name = name;

        // Notify the proxy server that the redirect is taking place
        var remoteEndpoint = new IPEndPoint(remoteIpAddress, remotePort);
        ClientRedirected?.Invoke(this, new NetworkRedirectEventArgs(name, remoteEndpoint));
    }

    private void HandleClientAuthRequest(NetworkPacket packet)
    {
        var reader = new NetworkPacketReader(packet);
        var seed = reader.ReadByte();
        var keyLength = reader.ReadByte();
        var key = reader.ReadBytes(keyLength);
        var name = reader.ReadString8();
        var id = reader.ReadUInt32();
        
        // Update connection name based on client request
        Name = name;

        var encryptionParameters = new NetworkEncryptionParameters(seed, key, name);
        
        // Update the client/server encryption parameters together
        _clientEncryptor.Parameters = encryptionParameters;
        _serverEncryptor.Parameters = encryptionParameters;
    }

    private void HandleServerSetUserId(NetworkPacket packet)
    {
        var reader = new NetworkPacketReader(packet);
        var userId = reader.ReadUInt32();

        IsAuthenticated = true;

        // Update the user ID based on server response
        UserId = userId;

        ClientAuthenticated?.Invoke(this, EventArgs.Empty);
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