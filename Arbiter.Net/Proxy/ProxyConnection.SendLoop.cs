﻿using System.Buffers;
using Arbiter.Net.Client;
using Arbiter.Net.Security;
using Arbiter.Net.Server;

namespace Arbiter.Net.Proxy;

public partial class ProxyConnection
{
    private async Task SendLoopAsync(CancellationToken token = default)
    {
        var headerBuffer = ArrayPool<byte>.Shared.Rent(NetworkPacket.HeaderSize);

        try
        {
            while (!token.IsCancellationRequested)
            {
                NetworkPacket decryptedPacket;

                // Prefer priority queue packets first
                if (_prioritySendQueue.Reader.TryRead(out var priorityPacket))
                {
                    decryptedPacket = priorityPacket;
                }
                else
                {
                    // Wait until any queue has data
                    var priorityWait = _prioritySendQueue.Reader.WaitToReadAsync(token).AsTask();
                    var normalWait = _sendQueue.Reader.WaitToReadAsync(token).AsTask();
                    await Task.WhenAny(priorityWait, normalWait).ConfigureAwait(false);

                    if (_prioritySendQueue.Reader.TryRead(out priorityPacket))
                    {
                        decryptedPacket = priorityPacket;
                    }
                    else if (_sendQueue.Reader.TryRead(out var normalPacket))
                    {
                        decryptedPacket = normalPacket;
                    }
                    else
                    {
                        // Both queues completed or canceled
                        if ((priorityWait.IsCompleted && priorityWait.Result == false) &&
                            (normalWait.IsCompleted && normalWait.Result == false))
                        {
                            break;
                        }

                        continue;
                    }
                }

                // Do not send the 0x42 Client Exception packet to the server to avoid suspicion
                if (decryptedPacket is ClientPacket { Command: ClientCommand.Exception })
                {
                    PacketException?.Invoke(this, new NetworkPacketEventArgs(decryptedPacket));
                    continue;
                }

                // Determine which stream we need to send to
                var destinationStream = decryptedPacket switch
                {
                    ClientPacket => _serverStream!,
                    ServerPacket => _clientStream!,
                    _ => throw new InvalidOperationException("Invalid packet type")
                };

                // Determine which encryption to use based on the outgoing packet type
                INetworkPacketEncryptor encryptor = decryptedPacket switch
                {
                    ClientPacket => _clientEncryptor,
                    ServerPacket => _serverEncryptor,
                    _ => throw new InvalidOperationException("Invalid packet type")
                };

                NetworkPacket encryptedPacket;

                // Encrypt the packet if necessary
                if (encryptor.ShouldEncrypt(decryptedPacket.Command))
                {
                    // Generate the next sequence number
                    var nextSequence = decryptedPacket switch
                    {
                        ClientPacket => (byte)(Interlocked.Increment(ref _clientSequence) % 256 - 1),
                        ServerPacket => (byte)(Interlocked.Increment(ref _serverSequence) % 256 - 1),
                        _ => throw new InvalidOperationException("Invalid packet type")
                    };

                    // Encrypt the packet with the next sequence number
                    encryptedPacket = encryptor.Encrypt(decryptedPacket, nextSequence);
                }
                else
                {
                    // No encryption is required, just return the packet as-is
                    encryptedPacket = decryptedPacket;
                }

                try
                {
                    // Write the encrypted packet to the network stream
                    await encryptedPacket.WriteToAsync(destinationStream, headerBuffer.AsMemory(), token)
                        .ConfigureAwait(false);
                }
                catch (IOException)
                {
                    // Socket was disconnected, cancel the send operation
                    continue;
                }

                // Notify that we have sent a packet
                PacketSent?.Invoke(this,
                    new NetworkTransferEventArgs(NetworkDirection.Send, encryptedPacket, decryptedPacket));
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
}