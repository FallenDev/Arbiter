using System.Net;
using System.Text;
using Arbiter.Net.Client;
using Arbiter.Net.Server;

namespace Arbiter.Net.Serialization;

public class NetworkPacketBuilder : INetworkPacketBuilder
{
    private readonly List<byte> _buffer = [];
    private readonly Encoding _encoding;

    public byte Command { get; }
    public bool IsClient { get; }
    public bool IsServer { get; }

    public int Length => _buffer.Count;

    public NetworkPacketBuilder(ClientCommand command, Encoding? encoding = null)
    {
        Command = (byte)command;
        IsClient = true;

        _encoding = encoding ?? Encoding.ASCII;
    }

    public NetworkPacketBuilder(ServerCommand command, Encoding? encoding = null)
    {
        Command = (byte)command;
        IsServer = true;

        _encoding = encoding ?? Encoding.ASCII;
    }

    public void AppendBoolean(bool value) => AppendByte(value ? (byte)1 : (byte)0);

    public void AppendSByte(sbyte value) => AppendByte((byte)value);
    public void AppendByte(byte value) => _buffer.Add(value);

    public void AppendInt16(short value) => AppendUInt16((ushort)value);

    public void AppendUInt16(ushort value)
    {
        // Big endian
        _buffer.Add((byte)(value >> 8));
        _buffer.Add((byte)value);
    }

    public void AppendInt32(int value) => AppendUInt32((uint)value);

    public void AppendUInt32(uint value)
    {
        // Big endian
        _buffer.Add((byte)(value >> 24));
        _buffer.Add((byte)(value >> 16));
        _buffer.Add((byte)(value >> 8));
        _buffer.Add((byte)value);
    }

    public void AppendInt64(long value) => AppendUInt64((ulong)value);

    public void AppendUInt64(ulong value)
    {
        // Big endian
        _buffer.Add((byte)(value >> 56));
        _buffer.Add((byte)(value >> 48));
        _buffer.Add((byte)(value >> 40));
        _buffer.Add((byte)(value >> 32));
        _buffer.Add((byte)(value >> 24));
        _buffer.Add((byte)(value >> 16));
        _buffer.Add((byte)(value >> 8));
        _buffer.Add((byte)value);
    }

    public void AppendString8(string value)
    {
        var length = Math.Min(value.Length, byte.MaxValue);
        var bytes = _encoding.GetBytes(value[..length]);
        AppendByte((byte)bytes.Length);
        _buffer.AddRange(bytes);
    }

    public void AppendString16(string value)
    {
        var length = Math.Min(value.Length, ushort.MaxValue);
        var bytes = _encoding.GetBytes(value[..length]);
        AppendUInt16((ushort)bytes.Length);
        _buffer.AddRange(bytes);
    }

    public void AppendNullTerminatedString(string value)
    {
        var bytes = _encoding.GetBytes(value);
        _buffer.AddRange(bytes);
        AppendByte(0);
    }

    public void AppendLine(string value)
    {
        var bytes = _encoding.GetBytes(value);
        _buffer.AddRange(bytes);
        AppendByte(0x0A);
    }

    public void AppendIPv4Address(IPAddress address)
    {
        Span<byte> ipBytes = stackalloc byte[4];
        address.TryWriteBytes(ipBytes, out _);

        _buffer.Add(ipBytes[3]);
        _buffer.Add(ipBytes[2]);
        _buffer.Add(ipBytes[1]);
        _buffer.Add(ipBytes[0]);
    }

    public void AppendBytes(byte[] bytes) => _buffer.AddRange(bytes);
    
    public void AppendBytes(byte[] bytes, int start, int count) => _buffer.AddRange(bytes[start..(start + count)]);

    public void AppendBytes(ReadOnlySpan<byte> bytes) => _buffer.AddRange(bytes);

    public void AppendZero(int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            _buffer.Add(0);
        }
    }

    public NetworkPacket ToPacket() =>
        IsClient ? new ClientPacket(Command, _buffer) : new ServerPacket(Command, _buffer);

    public override string ToString()
    {
        var sb = new StringBuilder(Command.ToString("X2"));

        if (_buffer.Count > 0)
        {
            sb.Append(' ');
            sb.Append(string.Join(' ', _buffer.Select(x => x.ToString("X2"))));
        }

        return sb.ToString();
    }
}