using System.Net;

namespace Arbiter.Net.Serialization;

public interface INetworkPacketReader
{
    byte? Sequence { get; }
    
    int Position { get; set; }
    int Length { get; }

    bool ReadBoolean();
    sbyte ReadSByte();
    byte ReadByte();
    short ReadInt16();
    ushort ReadUInt16();
    int ReadInt32();
    uint ReadUInt32();
    long ReadInt64();
    ulong ReadUInt64();
    string ReadString8();
    string ReadString16();
    string ReadFixedString(int length);
    string ReadNullTerminatedString();
    string ReadLine();
    IPAddress ReadIPv4Address();
    
    byte[] ReadBytes(int length);
    void ReadBytes(Span<byte> destination, int count);
    byte[] ReadToEnd();
    void ReadToEnd(Span<byte> destination);
    
    void Skip(int length);
}