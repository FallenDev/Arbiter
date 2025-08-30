using System.Buffers;
using System.Net.Sockets;
using Arbiter.Net.Client;
using Arbiter.Net.Security;
using Arbiter.Net.Server;

namespace Arbiter.Net;

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
                // Attempt to ready from the stream for any available bytes
                int recvCount;
                try
                {
                    recvCount = await stream.ReadAsync(recvBuffer, token).ConfigureAwait(false);
                }
                catch when (token.IsCancellationRequested)
                {
                    recvCount = 0;
                }

                // Handle socket disconnect when read returns zero
                if (recvCount == 0)
                {
                    if (direction == ProxyDirection.ServerToClient)
                    {
                        ServerDisconnected?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        ClientDisconnected?.Invoke(this, EventArgs.Empty);
                    }

                    // This will trigger the other send/recv tasks to also cancel
                    await tokenSource.CancelAsync();
                    break;
                }

                // Append all received bytes into the packet buffer
                packetBuffer.Append(recvBuffer, 0, recvCount);

                // Attempt to dequeue all available packets
                while (packetBuffer.TryTakePacket(out var packet))
                {
                    // Decrypt the packet if necessary
                    var decrypted = encryptor?.IsEncrypted(packet.Command) ?? false
                        ? encryptor.Decrypt(packet)
                        : packet;
                    
                    switch (decrypted)
                    {
                        // Handle server encryption, we need to update encryption parameters
                        case ServerPacket { Command: ServerCommand.SetEncryption }:
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
                        // Handle client auth request, we need to update encryption parameters
                        case ClientPacket { Command: ClientCommand.Authenticate }:
                            HandleClientAuthRequest(decrypted);
                            break;
                    }

                    // Raise the event with the decrypted packet
                    PacketReceived?.Invoke(this, new NetworkPacketEventArgs(NetworkAction.Receive, decrypted, packet.ToList()));
                    await _sendQueue.Writer.WriteAsync(packet, token).ConfigureAwait(false);
                }
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