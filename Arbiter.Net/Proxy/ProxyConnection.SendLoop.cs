using System.Buffers;
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
                // Determine which stream we need to send to
                var destinationStream = decryptedPacket switch
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
                INetworkPacketEncryptor? encryptor = decryptedPacket switch
                {
                    ClientPacket => _clientEncryptor,
                    ServerPacket => _serverEncryptor,
                    _ => null
                };

                // Determine the next sequence number
                var nextSequence = decryptedPacket switch
                {
                    ClientPacket => _clientSequence++,
                    ServerPacket => _serverSequence++,
                    _ => (byte)0x00
                };

                // Encrypt the packet if necessary
                var encryptedPacket = encryptor?.IsEncrypted(decryptedPacket.Command) ?? false
                    ? encryptor.Encrypt(decryptedPacket, nextSequence)
                    : decryptedPacket;

                // Do not send the 0x42 Client Exception packet to the server to avoid suspicion
                if (encryptedPacket is ClientPacket { Command: ClientCommand.Exception })
                {
                    PacketException?.Invoke(this, new NetworkPacketEventArgs(decryptedPacket));
                    continue;
                }

                // Write the encrypted packet to the network stream
                await encryptedPacket.WriteToAsync(destinationStream, headerBuffer.AsMemory(), token)
                    .ConfigureAwait(false);

                // Notify that we have sent a packet
                PacketSent?.Invoke(this, new NetworkTransferEventArgs(NetworkDirection.Send, encryptedPacket, decryptedPacket));
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