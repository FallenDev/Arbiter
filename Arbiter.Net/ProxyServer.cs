using System.Net;
using System.Net.Sockets;

namespace Arbiter.Net;

public class ProxyServer : IDisposable
{
    private bool _isDisposed;
    private IPEndPoint? _remoteEndpoint;
    private TcpListener? _listener;
    private CancellationTokenSource? _cancelTokenSource;
    private readonly Lock _connectionsLock = new();
    private readonly List<ProxyConnection> _connections = [];
    private int _nextConnectionId;
    
    public bool IsRunning => _listener is not null;
    public IPEndPoint? LocalEndpoint => _listener?.LocalEndpoint as IPEndPoint;
    public IPEndPoint? RemoteEndpoint => _remoteEndpoint;

    public event Action? Started;
    public event Action? Stopped;
    public event EventHandler<ProxyConnectionEventArgs>? ClientConnected;
    public event EventHandler<ProxyConnectionEventArgs>? ServerConnected;
    public event EventHandler<ProxyConnectionEventArgs>? ClientDisconnected;
    public event EventHandler<ProxyConnectionEventArgs>? ServerDisconnected;
    public event EventHandler<ProxyConnectionDataEventArgs>? PacketReceived;
    public event EventHandler<ProxyConnectionDataEventArgs>? PacketSent;

    public void Start(int listenPort, IPAddress remoteAddress, int remotePort) =>
        Start(listenPort, new IPEndPoint(remoteAddress, remotePort));

    public void Start(int listenPort, IPEndPoint remoteEndpoint)
    {
        CheckIfDisposed();

        if (IsRunning)
        {
            throw new InvalidOperationException("Proxy server is already running");
        }

        _remoteEndpoint = remoteEndpoint;

        _cancelTokenSource = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Loopback, listenPort);
        _listener.Start();

        _ = AcceptLoopAsync(_cancelTokenSource.Token);

        Started?.Invoke();
    }

    public void Stop()
    {
        CheckIfDisposed();

        if (!IsRunning)
        {
            return;
        }

        _cancelTokenSource?.Cancel();
        _listener?.Stop();
        _listener = null;

        Stopped?.Invoke();
    }

    private async Task AcceptLoopAsync(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener!.AcceptTcpClientAsync(token).ConfigureAwait(false);
            }
            catch when (token.IsCancellationRequested)
            {
                break;
            }

            var connectionId = Interlocked.Increment(ref _nextConnectionId);
            var connection = new ProxyConnection(connectionId, client);
            {
                using var _ = _connectionsLock.EnterScope();
                _connections.Add(connection);
            }

            ClientConnected?.Invoke(this, new ProxyConnectionEventArgs(connection));

            _ = HandleConnectionAsync(connection, token);
        }
    }

    private async Task HandleConnectionAsync(ProxyConnection connection, CancellationToken token)
    {
        connection.ClientDisconnected += OnClientDisconnected;
        connection.ServerConnected += OnServerConnected;
        connection.ServerDisconnected += OnServerDisconnected;
        connection.PacketReceived += OnRecv;
        connection.PacketSent += OnSend;

        try
        {
            await connection.ConnectToRemoteAsync(_remoteEndpoint!, token).ConfigureAwait(false);
            await connection.SendRecvLoopAsync(token).ConfigureAwait(false);
        }
        finally
        {
            connection.ClientDisconnected -= OnClientDisconnected;
            connection.ServerConnected -= OnServerConnected;
            connection.ServerDisconnected -= OnServerDisconnected;
            connection.PacketReceived -= OnRecv;
            connection.PacketSent -= OnSend;

            using var _ = _connectionsLock.EnterScope();
            _connections.Remove(connection);
            connection.Dispose();
        }

        return;
        
        // Forwarding handlers for events
        void OnClientDisconnected(object? sender, EventArgs e) =>
            ClientDisconnected?.Invoke(this, new ProxyConnectionEventArgs((sender as ProxyConnection)!));
        
        void OnServerConnected(object? sender, EventArgs e) =>
            ServerConnected?.Invoke(this, new ProxyConnectionEventArgs((sender as ProxyConnection)!));
        
        void OnServerDisconnected(object? sender, EventArgs e) =>
            ServerDisconnected?.Invoke(this, new ProxyConnectionEventArgs((sender as ProxyConnection)!));

        void OnRecv(object? sender, NetworkPacketEventArgs e) =>
            PacketReceived?.Invoke(this,
                new ProxyConnectionDataEventArgs((sender as ProxyConnection)!, e.Packet, e.Direction));

        void OnSend(object? sender, NetworkPacketEventArgs e) =>
            PacketSent?.Invoke(this,
                new ProxyConnectionDataEventArgs((sender as ProxyConnection)!, e.Packet, e.Direction));
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
            _cancelTokenSource?.Cancel();
            _listener?.Stop();

            using var _ = _connectionsLock.EnterScope();
            foreach (var connection in _connections)
            {
                connection.Dispose();
            }

            _connections.Clear();
        }

        _listener = null;
        _isDisposed = true;
    }

    private void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_isDisposed, "Proxy server is disposed");
}