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
                
                // Do not send the 0x42 Client Exception packet to the server to avoid suspicion
                if (encrypted is ClientPacket { Command: ClientCommand.Exception })
                {
                    PacketException?.Invoke(this,
                        new NetworkPacketEventArgs(NetworkAction.None, packet, encrypted.ToList()));
                    continue;
                }

                await encrypted.WriteToAsync(destinationStream, headerBuffer.AsMemory(), token)
                    .ConfigureAwait(false);

                // Raise the event with the decrypted (plaintext) packet
                var rawPacket = encrypted.ToList();
                PacketSent?.Invoke(this,
                    new NetworkPacketEventArgs(NetworkAction.Send, packet, rawPacket));
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