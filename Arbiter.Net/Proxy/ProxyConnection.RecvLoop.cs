using System.Buffers;
using System.Net.Sockets;
using Arbiter.Net.Client;
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

                    // Notify that we have received a packet
                    PacketReceived?.Invoke(this,
                        new NetworkTransferEventArgs(NetworkDirection.Receive, encryptedPacket, decrypted));
                    
                    // Send the decrypted packet to the other end of the connection (it will be re-encrypted)
                    await _sendQueue.Writer.WriteAsync(decrypted, token).ConfigureAwait(false);
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