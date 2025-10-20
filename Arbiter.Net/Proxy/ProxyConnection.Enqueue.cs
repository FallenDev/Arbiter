using Arbiter.Net.Client;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.Net.Proxy;

public partial class ProxyConnection
{
    public void EnqueuePacketAfter(NetworkPacket packet, TimeSpan delay,
        NetworkPriority priority = NetworkPriority.Normal)
    {
        CheckIfDisposed();

        _ = Task.Run(async () =>
        {
            await Task.Delay(delay);
            EnqueuePacket(packet, priority);
        });
    }

    public bool EnqueuePacket(NetworkPacket packet, NetworkPriority priority = NetworkPriority.Normal)
    {
        CheckIfDisposed();

        // Prioritize outgoing client heartbeat packets
        if (packet is ClientPacket { Command: ClientCommand.Heartbeat })
        {
            priority = NetworkPriority.High;
        }

        var writer = priority switch
        {
            NetworkPriority.High => _prioritySendQueue.Writer,
            _ => _sendQueue.Writer,
        };

        if (!writer.TryWrite(packet))
        {
            return false;
        }

        PacketQueued?.Invoke(this, new NetworkPacketEventArgs(packet));
        return true;
    }

    public void EnqueueMessageAfter(IClientMessage message, TimeSpan delay,
        NetworkPriority priority = NetworkPriority.Normal)
    {
        CheckIfDisposed();

        _ = Task.Run(async () =>
        {
            await Task.Delay(delay);
            EnqueueMessage(message, priority);
        });
    }

    public void EnqueueMessagesAfter(IEnumerable<IClientMessage> messages, TimeSpan delay,
        NetworkPriority priority = NetworkPriority.Normal)
    {
        CheckIfDisposed();

        _ = Task.Run(async () =>
        {
            await Task.Delay(delay);
            EnqueueMessages(messages, priority);
        });
    }

    public bool EnqueueMessages(IEnumerable<IClientMessage> messages, NetworkPriority priority = NetworkPriority.Normal)
    {
        CheckIfDisposed();
        return messages.All(message => EnqueueMessage(message, priority));
    }

    public bool EnqueueMessage(IClientMessage message, NetworkPriority priority = NetworkPriority.Normal)
    {
        CheckIfDisposed();

        var command = _clientMessageFactory.GetMessageCommand(message.GetType());
        if (command is null)
        {
            return false;
        }

        var builder = new NetworkPacketBuilder(command.Value);
        try
        {
            message.Serialize(ref builder);
            var packet = builder.ToPacket();

            // Prioritize outgoing client heartbeat packets
            if (packet is ClientPacket { Command: ClientCommand.Heartbeat })
            {
                priority = NetworkPriority.High;
            }

            return EnqueuePacket(packet, priority);
        }
        finally
        {
            builder.Dispose();
        }
    }

    public void EnqueueMessageAfter(IServerMessage message, TimeSpan delay,
        NetworkPriority priority = NetworkPriority.Normal)
    {
        CheckIfDisposed();

        _ = Task.Run(async () =>
        {
            await Task.Delay(delay);
            EnqueueMessage(message, priority);
        });
    }

    public void EnqueueMessagesAfter(IEnumerable<IServerMessage> messages, TimeSpan delay,
        NetworkPriority priority = NetworkPriority.Normal)
    {
        CheckIfDisposed();

        _ = Task.Run(async () =>
        {
            await Task.Delay(delay);
            EnqueueMessages(messages, priority);
        });
    }

    public bool EnqueueMessages(IEnumerable<IServerMessage> messages, NetworkPriority priority = NetworkPriority.Normal)
    {
        CheckIfDisposed();
        return messages.All(message => EnqueueMessage(message, priority));
    }

    public bool EnqueueMessage(IServerMessage message, NetworkPriority priority = NetworkPriority.Normal)
    {
        CheckIfDisposed();

        var command = _serverMessageFactory.GetMessageCommand(message.GetType());
        if (command is null)
        {
            return false;
        }

        var builder = new NetworkPacketBuilder(command.Value);
        try
        {
            message.Serialize(ref builder);
            var packet = builder.ToPacket();

            // Prioritize incoming server heartbeat packets
            if (packet is ServerPacket { Command: ServerCommand.Heartbeat })
            {
                priority = NetworkPriority.High;
            }

            return EnqueuePacket(packet, priority);
        }
        finally
        {
            builder.Dispose();
        }
    }
}