namespace Arbiter.Net.Server;

public class ServerPacket(byte command, ReadOnlySpan<byte> payload) : NetworkPacket(command, payload);