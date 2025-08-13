using System.Buffers;
using Arbiter.Net.Client;
using Arbiter.Net.Security;
using Arbiter.Net.Server;

namespace Arbiter.Net;

public partial class ProxyConnection
{
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
                    new NetworkPacketEventArgs(packet, packet.Data, nextSequence));
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