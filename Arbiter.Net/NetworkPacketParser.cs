namespace Arbiter.Net;

public class NetworkPacketParser
{
    public const int DefaultBufferSize = 4096;

    private byte[] _buffer;
    private int _bufferIndex;

    public NetworkPacketParser(int bufferSize = DefaultBufferSize)
    {
        _buffer = new byte[Math.Max(1, bufferSize)];
    }

    public void Append(byte[] buffer, int offset, int count)
    {
        EnsureCapacity(count);
        Buffer.BlockCopy(buffer, offset, _buffer, _bufferIndex, count);
        _bufferIndex += count;
    }

    private void EnsureCapacity(int additional)
    {
        var required = _bufferIndex + additional;
        if (required <= _buffer.Length)
        {
            return;
        }

        var newSize = _buffer.Length * 2;
        while (newSize < required)
        {
            newSize *= 2;
        }

        var newBuffer = new byte[newSize];
        Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _bufferIndex);
        _buffer = newBuffer;
    }

    public bool TryTakePacket(out NetworkPacket packet)
    {
        packet = null!;

        // Check we have at least the packet header ready
        if (_bufferIndex < NetworkPacket.HeaderSize)
        {
            return false;
        }

        // Check that we have the entire packet ready, from header size
        var dataSize = (_buffer[2] << 8 | _buffer[3]) - 2;
        var packetSize = dataSize + NetworkPacket.HeaderSize;
        if (_bufferIndex < packetSize)
        {
            return false;
        }

        // Copy the command, sequence, and data bytes
        var command = _buffer[4];
        var sequence = _buffer[5];
        var data = dataSize > 0 ? new byte[dataSize] : Array.Empty<byte>();
        if (dataSize > 0)
        {
            Buffer.BlockCopy(_buffer, NetworkPacket.HeaderSize, data, 0, dataSize);
        }

        packet = new NetworkPacket(command, sequence, data);

        // Copy the rest of the data back to the front of the buffer
        var remaining = _bufferIndex - packetSize;
        if (remaining > 0)
        {
            Buffer.BlockCopy(_buffer, packetSize, _buffer, 0, remaining);
        }

        _bufferIndex = remaining;
        return true;
    }
}