using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Arbiter.Net.Client;
using Arbiter.Net.Server;

namespace Arbiter.Net;

public partial class ProxyConnection : IDisposable
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
    public bool HasAuthenticated { get; private set; }
    public bool IsLoggedIn { get; private set; }
    
    public IPEndPoint? LocalEndpoint => _client.Client.LocalEndPoint as IPEndPoint;
    public IPEndPoint? RemoteEndpoint => _server?.Client.RemoteEndPoint as IPEndPoint;
    public bool IsConnected => IsClientConnected && IsServerConnected;
    public bool IsClientConnected => _client.Connected;
    public bool IsServerConnected => _server?.Connected ?? false;

    public event EventHandler? ClientAuthenticated;
    public event EventHandler? ClientLoggedIn;
    public event EventHandler? ClientLoggedOut;
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