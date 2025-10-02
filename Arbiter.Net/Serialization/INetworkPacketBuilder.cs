using System.Net;

namespace Arbiter.Net.Serialization;

public interface INetworkPacketBuilder
{
    byte Command { get; }
    bool IsClient { get; }
    bool IsServer { get; }
    int Length { get; }

    void AppendBoolean(bool value);
    void AppendSByte(sbyte value);
    void AppendByte(byte value);
    void AppendInt16(short value);
    void AppendUInt16(ushort value);
    void AppendInt32(int value);
    void AppendUInt32(uint value);
    void AppendInt64(long value);
    void AppendUInt64(ulong value);
    void AppendString8(string value);
    void AppendString16(string value);
    void AppendNullTerminatedString(string value);
    void AppendLine(string value);
    void AppendIPv4Address(IPAddress address);
    void AppendBytes(byte[] bytes);
    void AppendBytes(byte[] bytes, int start, int count);
    void AppendBytes(IEnumerable<byte> bytes);
    public void AppendBytes(ReadOnlySpan<byte> bytes);

    NetworkPacket ToPacket();
}