using System.Buffers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
            // Keep polling for any available outgoing packets
            await foreach (var decryptedPacket in _sendQueue.Reader.ReadAllAsync(token))
            {
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