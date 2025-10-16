using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.App.ViewModels.Client;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Arbiter.Net.Client;
using Arbiter.Net.Server;

namespace Arbiter.App.ViewModels.Send;

public partial class SendPacketViewModel
{
    private CancellationTokenSource? _cancellationTokenSource;

    private async Task RunSendLoopAsync(CancellationToken token)
    {
        int? iterations = LoopEnabled
            ? LoopCount >= 0 ? LoopCount : null
            : 1;

        var sendItems = _parsedItems.ToList();

        var initialDelay = SelectedDelay;
        var interval = SelectedRate;

        try
        {
            var client = SelectedClient;
            if (client is null || sendItems.Count == 0)
            {
                return;
            }
            if (initialDelay > TimeSpan.Zero)
            {
                await Task.Delay(initialDelay, token).ConfigureAwait(false);
            }

            do
            {
                for (var i = 0; i < sendItems.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var item = sendItems[i];

                    // Ignore the last wait item if it's the last iteration
                    if (item.IsWait && i == sendItems.Count - 1 && iterations is 1)
                    {
                        continue;
                    }

                    await HandleSendItemAsync(client, item, token);

                    // Do not apply interval after explicit wait line
                    if (item.IsWait)
                    {
                        continue;
                    }
                    
                    // Apply interval between packets
                    var hasMore = i < sendItems.Count - 1;
                    if (interval > TimeSpan.Zero && (hasMore || iterations is not 0))
                    {
                        await Task.Delay(interval, token).ConfigureAwait(false);
                    }
                }

                // Decrement the repeat count
                if (iterations is > 0)
                {
                    iterations--;
                }

                // Add a small delay between iterations to prevent busy loop
                await Task.Delay(1, token);
            } while (!token.IsCancellationRequested && iterations is not 0);
        }
        catch (OperationCanceledException)
        {
            // Do nothing
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending packets");
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            // Notify the UI that we are done sending
            Dispatcher.UIThread.Post(() => { IsSending = false; }, DispatcherPriority.Background);
        }
    }

    private async Task HandleSendItemAsync(ClientViewModel client, SendEntry entry, CancellationToken token = default)
    {
        if (entry.IsDisconnect)
        {
            client.Disconnect();
            return;
        }

        if (entry.IsWait && entry.Wait > TimeSpan.Zero)
        {
            await Task.Delay(entry.Wait.Value, token);
            return;
        }

        var packet = entry.Packet;
        if (packet is null && entry is { Command: not null, DataTemplate: { } template })
        {
            var packetBuffer = ArrayPool<byte>.Shared.Rent(template.Length);

            try
            {
                // Build data by resolving entity references if any
                Buffer.BlockCopy(template, 0, packetBuffer, 0, template.Length);
                if (entry.EntityReferences.Length > 0)
                {
                    foreach (var (name, offset) in entry.EntityReferences)
                    {
                        var entity = _entityStore.Entities.FirstOrDefault(e =>
                            string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
                        uint id = 0;
                        if (entity.Name is null ||
                            !string.Equals(entity.Name, name, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("Failed to resolve entity '@{Name}'. Using zero entity ID!", name);
                        }
                        else
                        {
                            id = (uint)entity.Id;
                        }

                        // Write 4-byte big-endian
                        BinaryPrimitives.WriteUInt32BigEndian(packetBuffer.AsSpan(offset), id);
                    }
                }

                var span = packetBuffer.AsSpan(0, template.Length);
                packet = entry.IsServerPacket
                    ? new ServerPacket(entry.Command.Value, span)
                    : new ClientPacket(entry.Command.Value, span);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building packet for send");
                return;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(packetBuffer);
            }
        }

        if (packet is null)
        {
            return;
        }

        // Handle the packet and send to the client
        var queued = client.EnqueuePacket(packet);
        if (!queued)
        {
            _logger.LogWarning("[{ClientName}] Failed to enqueue packet", client.Name);
        }
    }
}