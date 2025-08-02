using System.Collections;
using System.Net.Sockets;

namespace Arbiter.Net;

public class NetworkPacket : IEnumerable<byte>
{
    public const byte Marker = 0xAA;
    public const int HeaderSize = 5;
    
    public byte Command { get; }
    public byte Sequence { get; }
    public byte[] Data { get; }

    public NetworkPacket(byte command, byte sequence, byte[] data)
    {
        Command = command;
        Sequence = sequence;
        Data = data;
    }

    public async ValueTask WriteToAsync(NetworkStream stream, Memory<byte> headerBuffer,
        CancellationToken cancellationToken = default)
    {
        var size = Data.Length + 2;

        var header = headerBuffer.Span;
        header[0] = Marker;
        header[1] = (byte)((size >> 8) & 0xFF);
        header[2] = (byte)(size & 0xFF);
        header[3] = Command;
        header[4] = Sequence;

        await stream.WriteAsync(headerBuffer[..HeaderSize], cancellationToken)
            .ConfigureAwait(false);

        if (Data.Length > 0)
        {
            await stream.WriteAsync(Data, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public IEnumerator<byte> GetEnumerator()
    {
        var size = Data.Length + 2;
        
        yield return Marker;
        yield return (byte)((size >> 8) & 0xFF);
        yield return (byte)(size & 0xFF);
        yield return Command;
        yield return Sequence;
        foreach (var dataByte in Data)
        {
            yield return dataByte;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => string.Join(' ', this.Select(x => x.ToString("X2")));
}