namespace Arbiter.Net.Client;

public class ClientPacket(byte command, ReadOnlySpan<byte> payload) : NetworkPacket(command, payload);