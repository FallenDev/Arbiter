using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;
using Avalonia.Threading;

namespace Arbiter.App.ViewModels.Client;

public class ClientViewModel(ProxyConnection connection) : ViewModelBase
{
    public int Id { get; init; }
    public required string Name { get; set; }
    
    public int? Level { get; set; }

    public void Subscribe()
    {
        connection.PacketReceived += OnPacketReceived;
    }

    public void Unsubscribe()
    {
        connection.PacketReceived -= OnPacketReceived;
    }

    private void OnPacketReceived(object? sender, NetworkPacketEventArgs e)
    {
        if (e.Packet is ClientPacket clientPacket)
        {
            if (ClientMessageFactory.Default.TryCreate(clientPacket, out var message))
            {
                Dispatcher.UIThread.Post(() => HandleClientMessage(message));
            }
        }

        if (e.Packet is ServerPacket serverPacket)
        {
            if (ServerMessageFactory.Default.TryCreate(serverPacket, out var message))
            {
                Dispatcher.UIThread.Post(() => HandleServerMessage(message));
            }
        }
    }

    private void HandleClientMessage(IClientMessage message)
    {
        // Handle packets sent by the client
    }

    private void HandleServerMessage(IServerMessage message)
    {
        // Handle packets sent by the server

        if (message is ServerUpdateStatsMessage statsMessage)
        {
            if (statsMessage.Level.HasValue)
            {
                Level = statsMessage.Level;
            }
        }
    }
}