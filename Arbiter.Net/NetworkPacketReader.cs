﻿using System.Net;
using System.Text;
using Arbiter.Net.Client;
using Arbiter.Net.Server;

namespace Arbiter.Net;

public class NetworkPacketReader(NetworkPacket packet, Encoding? encoding = null)
{
    private int _position;
    private readonly byte[] _buffer = packet.Data;
    private readonly Encoding _encoding = encoding ?? Encoding.ASCII;
    
    public byte? Sequence => packet switch
    {
        ClientPacket clientPacket => clientPacket.Sequence,
        ServerPacket serverPacket => serverPacket.Sequence,
        _ => null
    };
    
    public int Position
    {
        get => _position;
        set
        {
            if (value < 0 || value > _buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _position = value;
        }
    }
    
    public int Length => _buffer.Length;

    public bool ReadBoolean() => ReadByte() != 0;

    public sbyte ReadSByte() => (sbyte)ReadByte();

    public byte ReadByte()
    {
        EnsureCanRead(1);
        return _buffer[_position++];
    }

    public short ReadInt16() => (short)ReadUInt16();

    public ushort ReadUInt16()
    {
        EnsureCanRead(2);
        return (ushort)((_buffer[_position++] << 8) | _buffer[_position++]);
    }

    public int ReadInt32() => (int)ReadUInt32();

    public uint ReadUInt32()
    {
        EnsureCanRead(4);
        return ((uint)_buffer[_position++] << 24) | ((uint)_buffer[_position++] << 16) |
               ((uint)_buffer[_position++] << 8) | _buffer[_position++];
    }

    public long ReadInt64() => (long)ReadUInt64();

    public ulong ReadUInt64()
    {
        EnsureCanRead(8);
        return ((ulong)_buffer[_position++] << 56) | ((ulong)_buffer[_position++] << 48) |
               ((ulong)_buffer[_position++] << 40) | ((ulong)_buffer[_position++] << 32) |
               ((ulong)_buffer[_position++] << 24) | ((ulong)_buffer[_position++] << 16) |
               ((ulong)_buffer[_position++] << 8) | _buffer[_position++];
    }

    public string ReadString8() => ReadFixedString(ReadByte());

    public string ReadString16() => ReadFixedString(ReadUInt16());

    public string ReadFixedString(int length)
    {
        switch (length)
        {
            case < 0:
                throw new ArgumentOutOfRangeException(nameof(length));
            case 0:
                return string.Empty;
        }

        EnsureCanRead(length);
        var stringValue =  _encoding.GetString(_buffer, _position, length);
        
        _position += length;
        return stringValue;
    }

    public string ReadNullTerminatedString()
    {
        var length = 0;
        while (_buffer[_position + length] != 0)
        {
            EnsureCanRead(1);
            length++;
        }

        var stringValue = _encoding.GetString(_buffer, _position, length);
        
        _position += length + 1;
        return stringValue;
    }

    public string ReadLine()
    {
        var length = 0;
        while (_buffer[_position + length] != '\n' && _buffer[_position + length] != '\r')
        {
            EnsureCanRead(1);
            length++;
        }

        var stringValue = _encoding.GetString(_buffer, _position, length);

        _position += length + 1;
        return stringValue;
    }

    public IPAddress ReadIPv4Address()
    {
        EnsureCanRead(4);
        
        Span<byte> ipBytes = stackalloc byte[4];
        ipBytes[3] = _buffer[_position++];
        ipBytes[2] = _buffer[_position++];
        ipBytes[1] = _buffer[_position++];
        ipBytes[0] = _buffer[_position++];
        
        return new IPAddress(ipBytes);
    }

    public byte[] ReadBytes(int length)
    {
        switch (length)
        {
            case < 0:
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than or equal to 0");
            case 0:
                return [];
        }

        EnsureCanRead(length);
        var bytes = _buffer.AsSpan(_position, length).ToArray();

        _position += length;
        return bytes;
    }

    public void ReadBytes(Span<byte> buffer, int count)
    {
        EnsureCanRead(count);
        _buffer.AsSpan(_position, count).CopyTo(buffer);
        _position += count;
    }

    public byte[] ReadToEnd()
    {
        var length = Length - _position;
        return ReadBytes(length);
    }

    public void Skip(int length)
    {
        _position += length;
    }

    private void EnsureCanRead(int length)
    {
        if (_position + length > _buffer.Length)
        {
            throw new IndexOutOfRangeException("Cannot read past end of buffer");
        }
    }
}