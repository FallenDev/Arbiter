using System.Collections;

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

    public IEnumerator<byte> GetEnumerator()
    {
        var size = Data.Length + HeaderSize;
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