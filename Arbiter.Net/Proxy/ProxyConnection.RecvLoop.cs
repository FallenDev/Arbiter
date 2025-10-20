﻿using System.Buffers;
using System.Net.Sockets;
using Arbiter.Net.Client;
using Arbiter.Net.Filters;
using Arbiter.Net.Security;
using Arbiter.Net.Server;

namespace Arbiter.Net.Proxy;

public partial class ProxyConnection
{
    private async Task RecvLoopAsync(NetworkStream stream, NetworkPacketBuffer packetBuffer,
        INetworkPacketEncryptor encryptor, ProxyDirection direction, CancellationTokenSource tokenSource)
    {
        var token = tokenSource.Token;
        var recvBuffer = ArrayPool<byte>.Shared.Rent(RecvBufferSize);

        try
        {
            while (!token.IsCancellationRequested)
            {
                // Attempt to read from the stream for any available bytes
                int recvCount;
                try
                {
                    recvCount = await stream.ReadAsync(recvBuffer, token).ConfigureAwait(false);
                }
                catch (IOException)
                {
                    // Socket was disconnected, cancel the read operation
                    recvCount = 0;
                }
                catch when (token.IsCancellationRequested)
                {
                    recvCount = 0;
                }

                // Handle socket disconnect when read returns zero
                if (recvCount == 0)
                {
                    // This will trigger the other send/recv tasks to also cancel
                    await tokenSource.CancelAsync();
                    break;
                }

                // Append all received bytes into the packet buffer
                packetBuffer.Append(recvBuffer, 0, recvCount);

                // Attempt to dequeue all available packets
                while (packetBuffer.TryTakePacket(out var encryptedPacket))
                {
                    // Decrypt the packet if necessary
                    var decrypted = encryptor.Decrypt(encryptedPacket);

                    switch (decrypted)
                    {
                        // Handle server encryption, we need to update encryption parameters
                        case ServerPacket { Command: ServerCommand.ServerList }:
                            HandleServerSetEncryption(decrypted);
                            break;
                        // Handle server redirects, we need to hijack the redirect
                        case ServerPacket { Command: ServerCommand.Redirect }:
                            HandleServerRedirect(decrypted);
                            break;
                        // Handle server setting user ID, this confirms a valid game connection
                        case ServerPacket { Command: ServerCommand.UserId }:
                            HandleServerSetUserId(decrypted);
                            break;
                        // Handle server exit response, to clear state
                        case ServerPacket { Command: ServerCommand.ExitResponse }:
                            HandleServerExitResponse(decrypted);
                            break;
                        // Handle client auth request, we need to update encryption parameters
                        case ClientPacket { Command: ClientCommand.Authenticate }:
                            HandleClientAuthRequest(decrypted);
                            break;
                    }

                    // Process any filters for the packet
                    var filterResult = FilterPacket(decrypted);

                    // Notify that we have received a packet
                    PacketReceived?.Invoke(this,
                        new NetworkTransferEventArgs(NetworkDirection.Receive, encryptedPacket, decrypted,
                            filterResult));

                    NotifyObservers(this, decrypted);
                    
                    // If the packet was blocked, do not send it to the other end
                    if (filterResult.Action == NetworkFilterAction.Block)
                    {
                        continue;
                    }

                    if (filterResult.Output is not null)
                    {
                        // Prioritize server heartbeat packets to the client, and enqueue others normally
                        var output = filterResult.Output;
                        var isServerHeartbeat = output is ServerPacket { Command: ServerCommand.Heartbeat };
                        var writer = isServerHeartbeat ? _prioritySendQueue.Writer : _sendQueue.Writer;

                        // Send the decrypted packet to the other end of the connection (it will be re-encrypted)
                        var queuedPacket = new QueuedNetworkPacket(output, NetworkPacketSource.Network);
                        await writer.WriteAsync(queuedPacket, token).ConfigureAwait(false);
                    }

                    // Notify when a filter throws an exception
                    if (filterResult.Exception is not null)
                    {
                        FilterException?.Invoke(this, new NetworkFilterEventArgs(filterResult));
                    }
                }
            }
            
            if (direction == ProxyDirection.ClientToServer)
            {
                ClientDisconnected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ServerDisconnected?.Invoke(this, EventArgs.Empty);
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
}